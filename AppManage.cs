using OeipCommon;
using OeipCommon.FileTransfer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WinLiveManage.NewLoginfo;

namespace WinLiveManage
{
    public class AppManage : MSingleton<AppManage>
    {
        private bool bInnerNet = false;

        private RequestList selfRequestList = new RequestList();
       
        
        protected override void Init()
        {
            Task.Run(() =>
            {
                string remotePath = SettingManager.Instance.Setting.launchpadSetting.SelfUpdateAddressInternal;
                RequestItem requestItem = RequestItem.GetRequestItem(remotePath);
                requestItem.SetPath(remotePath, string.Empty);
                bInnerNet = requestItem.IsHaveRemote(500);
                //发送当前程序的本地与远程版本
                LiveAppHub.Hub.Clients.All.SendInnerNet(bInnerNet);
                if (!bInnerNet)
                {
                    SettingManager.Instance.Setting.launchpadSetting.IsInternal = false;
                }
            });

            selfRequestList.OnDownloadProgressEvent += (ProgressArgs progressArg, ProgressType progressType) =>
            {
                LiveAppHub.Hub.Clients.All.DownFile("", progressArg, progressType);
            };
            selfRequestList.LocalDownloadList = "LaunchpadManifest.txt";
            selfRequestList.LocalListCheck = "LaunchpadManifest.checksum";
            selfRequestList.RemoteListParent = "launcher/bin";
            selfRequestList.RemoteDownloadList = "launcher/LaunchpadManifest.txt";
            selfRequestList.RemoteListCheck = "launcher/LaunchpadManifest.checksum";
            selfRequestList.RemoteVersion = "launcher/LauncherVersion.txt";
        }

        public HostVersion UploadSelfPath(string gamePath)
        {
            HostVersion hostVersion = new HostVersion();
            try
            {
                //定义文件传输新目录
                string uri = SettingManager.Instance.Setting.launchpadSetting.SelfUpdateAddressInternal;
                selfRequestList.SetDirectory(uri, gamePath);
                string exeName = Path.GetFileName(GetType().Assembly.Location);
                string path = Path.Combine(gamePath, exeName);
                hostVersion.remoteVersion = selfRequestList.GetRemoteVersion()?.ToString();
                if (File.Exists(path))
                {
                    var assembly = Assembly.LoadFile(path);
                    hostVersion.localVersion = assembly.GetName().Version.ToString();
                    LogHelper.LogMessage("选择上传后台版本:" + hostVersion.localVersion);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx($"get host {gamePath} version error:", ex);
            }
            return hostVersion;
        }

        public bool UploadSelf(string gamePath, string localVersion)
        {
            try
            {
                if (!Directory.Exists(gamePath))
                {
                    LiveAppHub.SendMessage($"课件管理程序路径不存在:{gamePath}.", OeipLogLevel.OEIP_WARN);
                    return false;
                }
                //生成一个版本文件
                var gameVersionPath = Path.Combine(Directory.GetParent(gamePath).FullName, "LauncherVersion.txt");
                File.WriteAllText(gameVersionPath, localVersion.ToString());
                //定义文件传输新目录
                string uri = SettingManager.Instance.Setting.launchpadSetting.SelfUpdateAddressInternal;
                selfRequestList.SetDirectory(uri, gamePath);
                bool bSucess = selfRequestList.Upload((string path) =>
                {
                    return ToolHelper.FilterFile(path, true);
                }).Result;
                //上传版本文件
                RequestItem requestItem = selfRequestList.GetRequestItem();
                string remoteVersion = Path.Combine(selfRequestList.RemoteAddress, selfRequestList.RemoteVersion);
                requestItem.SetPath(remoteVersion, string.Empty);
                requestItem.Delete();
                requestItem.Upload(gameVersionPath);
                return bSucess;
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx($"upload host {gamePath} error:", ex);
                return false;
            }
        }
        //更新程序自身方法

        public void UpdateHost()
        {
            try
            {
                //先把文件下载到一个临时目录
                string localPath = GetHostTempPath();
                selfRequestList.SetDirectory(SettingManager.Instance.Setting.launchpadSetting.GetUpdateAddress(),
                    localPath);
                //先下载服务器的文件列表
                bool bGetFileList = selfRequestList.GetRemoteTransferList();
                if (bGetFileList)
                {
                    //下载文件
                    selfRequestList.DownloadList();
                    //验证文件完整
                     selfRequestList.Verify();
                }
                else
                {
                    LiveAppHub.SendMessage($"程序后台更新没有正常下载文件列表,请检查网络,如果网络没问题,请联系管理员", OeipLogLevel.OEIP_ERROR, 5000);
                    return;
                }
                string updateScriptPath = $@"{localPath}launchpad_update.bat";
                string scriptStr = GetUpdateScript();
                File.WriteAllText(updateScriptPath, scriptStr);
                var updateShellProcess = new ProcessStartInfo
                {
                    FileName = updateScriptPath,
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                // 这个目录里如果有配置文件
                string settingPath = Path.Combine(GetHostTempPath(), "AppSetting.xml");
                if (File.Exists(settingPath))
                {
                    File.Delete(settingPath);
                }
                //开始复制临时文件到当前目录
                Process.Start(updateShellProcess);
                LiveAppHub.Hub.Clients.All.SendUpdateHost(0);
                Thread.Sleep(500);
                //退出当前程序
                CloseApp();
                //Environment.Exit(0);
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("upldate host error:", ex);
            }
        }

        public string SelfHost
        {
            get
            {
                var server = ConfigurationManager.AppSettings["server"];
                var port = ConfigurationManager.AppSettings["port"];
                string url = string.Format("http://{0}:{1}", server, port);
                return url;
            }
        }

        //重启
        public void CloseApp(bool bRestart = false)
        {
            if (bRestart)
            {
                LogHelper.LogMessage("进程二秒后重启");
                string localPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string updateScriptPath = $@"{localPath}restart.bat";
                string scriptStr = GetRestartScript();
                File.WriteAllText(updateScriptPath, scriptStr);
                var updateShellProcess = new ProcessStartInfo
                {
                    FileName = updateScriptPath,
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                //开始复制临时文件到当前目录
                Process.Start(updateShellProcess);
            }
            LogHelper.LogMessage("用户选择关闭程序");
            LiveAppHub.SendBackMessage(1001, "用户选择关闭程序");
            //taskkill /pid xxx /f
            int pid = Process.GetCurrentProcess().Id;
            ProcessStartInfo procStopInfo = new ProcessStartInfo()
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "cmd.exe",
                Arguments = $"/c taskkill /pid {pid} /f",
            };
            //正常退出
            Program.bExit = true;
            LogUtility.Log("程序退出", LogType.Trace);
            //退出当前程序
            Environment.Exit(0);
            Application.Exit();
           
            //开始复制临时文件到当前目录
            //Process.Start(procStopInfo).WaitForExit(100);
        }

        public string GetHostTempPath()
        {
            try
            {
                var rootPath = SettingManager.Instance.GetPathSetting();
                string path = Path.Combine(rootPath.GamePath, "launchpad", "launcher");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("get host tmep path error", ex, OeipLogLevel.OEIP_WARN);
            }
            string localPath = Path.Combine(Path.GetTempPath(), "launchpad", "launcher");
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
            return localPath;
        }

        public string GetUpdateScript()
        {
            // Load the script from the embedded resources
            var localAssembly = Assembly.GetExecutingAssembly();

            var scriptSource = string.Empty;
            var resourceName = "WinLiveManage.Resources.launchpad_update.bat";// GetUpdateScriptResourceName();
            using (var resourceStream = localAssembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream != null)
                {
                    using (var reader = new StreamReader(resourceStream))
                    {
                        scriptSource = reader.ReadToEnd();
                    }
                }
            }
            var transientScriptSource = scriptSource;
            string tempPath = GetHostTempPath();
            transientScriptSource = transientScriptSource.Replace("%temp%", tempPath);
            transientScriptSource = transientScriptSource.Replace("%localDir%", Path.GetDirectoryName(localAssembly.Location));
            transientScriptSource = transientScriptSource.Replace("%launchpadExecutable%", Path.GetFileName(localAssembly.Location));

            return transientScriptSource;
        }

        public string GetRestartScript()
        {
            // Load the script from the embedded resources
            var localAssembly = Assembly.GetExecutingAssembly();

            var scriptSource = string.Empty;
            var resourceName = "WinLiveManage.Resources.restart.bat";// GetUpdateScriptResourceName();
            using (var resourceStream = localAssembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream != null)
                {
                    using (var reader = new StreamReader(resourceStream))
                    {
                        scriptSource = reader.ReadToEnd();
                    }
                }
            }
            var transientScriptSource = scriptSource;
            transientScriptSource = transientScriptSource.Replace("%launchpadExecutable%", Path.GetFileName(localAssembly.Location));

            return transientScriptSource;
        }

        public bool InnerNet
        {
            get
            {
                return bInnerNet;
            }
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
