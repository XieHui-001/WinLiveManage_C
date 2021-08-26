using OeipCommon;
using OeipCommon.FileTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OeipCommon.FileTransfer.Download_failed;
using static OeipCommon.FileTransfer.LSinfo;
using static OeipCommon.FileTransfer.RequestList;

namespace WinLiveManage
{
    public class LiveProgram
    {
        public ProgramInfo Info { get; set; }
        public ProgramSetting ProgramPath { get; set; }
        public RequestList Request { get; set; }

        public Version RemoteVersion { get; set; } = null;
        public Version LocalVersion { get; set; } = null;

        public string ProcessName { get; set; } = string.Empty;
        public string ProcessPath { get; set; } = string.Empty;

        public LauncherMode LauncherMode
        {
            get
            {
                if (!Request.IsComplete())
                {
                    LogHelper.LogMessage(ProgramPath.Path + "文件下载不完整");
                    return LauncherMode.Install;
                }
                if (!Request.ISPath()) {
                    return LauncherMode.Install;
                }
                if (!Request.IsGame_Path()) {
                    LogHelper.LogFile("文件下载不完整");
                    return LauncherMode.Install;
                }
                if (Request.IsHash()) {
                    LogHelper.LogMessage("Hash下载不完整");
                    return LauncherMode.Install;
                }


                if (RemoteVersion == null)
                    return LauncherMode.Inactive;
                else if ( LocalVersion == null)
                    return LauncherMode.Install;
                else if (LocalVersion != RemoteVersion || LocalVersion < RemoteVersion)
                    return LauncherMode.file_error;
                else if (LocalVersion < RemoteVersion)
                    return LauncherMode.Update;
                else
                    return LauncherMode.Run;
            }
        }
        //更新课件
        public void Install()
        {
            LogHelper.LogMessage($"{Info.app_name} 开始更新");
            //先下载服务器的文件列表
            bool bGetFileList = Request.GetRemoteTransferList();
            if (bGetFileList)
            {
                //下载文件
                Request.DownloadList();
                //验证文件完整
                //  Request.Verify();
            } else
            {
                LiveAppHub.SendMessage($"课件:{Info?.app_name} 没有正常下载文件列表,请检查网络,或硬盘是否支持下载课件空间,如果网络没问题,请联系管理员", OeipLogLevel.OEIP_ERROR, 5000);
            }
        }
      
        public void Verify()
        {
            //先下载服务器的文件列表
            bool bGetFileList = Request.GetRemoteTransferList();
            if (bGetFileList)
            {
                LiveAppHub.SendMessage($"课件:{Info?.app_name}开始验证，请等待", OeipLogLevel.OEIP_INFO);
                //验证文件完整
                Request.Verify();
                LiveAppHub.SendMessage($"课件:{Info?.app_name}:验证即将完成，请等待",OeipLogLevel.OEIP_INFO);
            }
            else
            {
                LiveAppHub.SendMessage($"课件:{Info?.app_name} 没有正常下载文件列表,请检查网络,或硬盘是否支持下载课件空间,如果网络没问题,请联系管理员", OeipLogLevel.OEIP_ERROR, 5000);
            }
        }
        public bool CheckProcess()
        {
            try
            {
                if (ProgramPath == null)
                {
                    /*LiveAppHub.SendMessage($"课件:{Info?.app_name} 路径{ProgramPath.Path} 没有找到,请检查", OeipLogLevel.OEIP_ERROR, 5000);*/
                   // LiveAppHub.SendMessage($"课件:{Info?.app_name} 路径{ProgramPath.Path} 没有找到", OeipLogLevel.OEIP_ERROR, 5000);
                    LiveAppHub.SendMessage($"系统已自动进行重新下载:{Info?.app_name} 请稍后", OeipLogLevel.OEIP_ERROR, 5000);
                   /* Download_Error.Log(Info?.app_name);*/
                    Install();
                    return false;
                }
                var path = Path.Combine(ProgramPath.Path, "Win64");
                var exePaths = Directory.GetFiles(path, "*.exe");
                if (exePaths.Length <= 0)
                {
                    //LiveAppHub.SendMessage($"课件:{Info?.app_name} 路径{ProgramPath.Path} 没有运行程序,请检查", OeipLogLevel.OEIP_ERROR, 5000);
                    LiveAppHub.SendMessage($"检测到课件:{Info?.app_name} 异常将自动进行从新载入请稍后", OeipLogLevel.OEIP_ERROR, 5000);
                  /*  Download_Error.Log(Info?.app_name);*/
                    Verify();
                    return false;
                }
                var executable = exePaths[0];
                ProcessName = Path.GetFileNameWithoutExtension(executable);
                ProcessPath = Path.Combine(path, ProcessName, "Binaries/Win64", Path.GetFileName(executable));
                if (!File.Exists(ProcessPath))
                {
                   // LiveAppHub.SendMessage($"课件:{Info?.app_name} 程序{ProcessPath} 路径不存在,请检查", OeipLogLevel.OEIP_ERROR, 5000);
                    LiveAppHub.SendMessage($"没有检测到课件:{Info?.app_name} 程序{ProcessPath} 路径不存在,将自动进行从新下载", OeipLogLevel.OEIP_ERROR, 5000);
                    /*Download_Error.Log(Info?.app_name);*/
                    Install();
                    return false;
                }
                //复制手柄文件
                string confPath = Path.Combine(Application.StartupPath, "Config/x360ce.ini");
                if (File.Exists(confPath))
                {
                    var runPath = Path.Combine(path, ProcessName);
                    try
                    {
                        string exePath = Path.Combine(runPath, "Binaries/Win64", "x360ce.ini");
                        string oldConfPath = Path.Combine(runPath, "Plugins/iVRealKit/ThirdParty/360controller", "x360ce.ini");
                        File.Copy(confPath, oldConfPath, true);
                        File.Copy(confPath, exePath, true);
                        LogHelper.LogMessage($"手柄文件从:{confPath} 复制到{runPath} 下对应目录", OeipLogLevel.OEIP_INFO);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogMessageEx($"手柄文件从:{confPath} 复制到{runPath} 下对应目录出错", ex);
                    }
                }
                else
                {
                    LogHelper.LogMessage($"目录{confPath}下没有手柄文件");
                }
                string zmfDirectory = Path.Combine(path, ProcessName, "Plugins/iVRealKit/ThirdParty/ZmfUE4Dll/bin");//"zmf.dll"
                if (Directory.Exists(zmfDirectory))
                {
                    if (File.Exists(Path.Combine(zmfDirectory, "zmf.dll")))
                    {
                        LogHelper.LogMessage($"目录{zmfDirectory}查找到zmf文件,开始生成AgoraLog目录用于生成agora日志");
                        Directory.CreateDirectory(Path.Combine(zmfDirectory, "AgoraLog"));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("CheckProcess", ex);
                return false;
            }
        }
    }
}
