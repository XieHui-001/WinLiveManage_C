using AocePackage;
using Microsoft.Owin.Hosting;
using OeipCommon;
using OeipCommon.FileTransfer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*using ZmfWrapper;*/
using static WinLiveManage.NewLoginfo;

namespace WinLiveManage
{
    public class Program
    {
        public static bool bExit = false;

        private static void OpenApp()
        {

#if !DEBUG
            try
            {
                Process[] processCollection = Process.GetProcessesByName("talkdoo");
                if (processCollection.Length > 0)
                {
                    foreach (var process in processCollection)
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            //ZmfHelper.setForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                    LogHelper.LogMessage("课件管理前台已经打开");
                    return;
                }
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "win-unpacked/talkdoo.exe");
                if (File.Exists(path))
                {
                    Process.Start(path);
                }
                else
                {
                    string appUri = AppUri();
                    Process.Start($"http://{appUri}");
                }
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("openapp error", ex);
            }
#endif
        }

        static void Main(string[] args)
        {

            try
            {
                //WinSystemInfo.Instance.GetHardware(true);
                LogHelper.LogMessage("启动课件管理后台");
                LogUtility.Log("启动课件管理后台", LogType.Trace);
                //创建接收SteamVR日志信息文件   如果已有将跳过
                File_creation.Filepath.CreateDirectory();
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processCollection = Process.GetProcessesByName(currentProcess.ProcessName);
                if (processCollection.Length > 1)
                {
                    LogHelper.LogMessage("程序已经打开.");
                    LogUtility.Log("程序已经打开", LogType.Trace);
                    //OpenApp();
                    //return;
                    var oprocess = processCollection.Where(p => p.Id != currentProcess.Id);
                    foreach (var prc in oprocess)
                    {
                        ProcessStartInfo procStopInfo = new ProcessStartInfo()
                        {
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = "cmd.exe",
                            Arguments = $"/c taskkill /pid {prc.Id} /f",
                        };
                        Process.Start(procStopInfo).WaitForExit(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("winlivemanager 进程分析出错", ex);
                LogUtility.Log("winlivemanager 进程分析出错" + ex, LogType.Trace);
            }
            try
            {
                //获得当前登录的Windows用户标示 
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
#if DEBUG
                if (true)
#else
                if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
#endif
                {
                    string url = AppManage.Instance.SelfHost;
                    Console.WriteLine("Server running on {0}", url);

                    using (WebApp.Start<Startup>(new StartOptions(url)))
                    {
                        //创建HttpCient测试webapi 
                        HttpClient client = new HttpClient();
                        //通过get请求数据
                        var response = client.GetAsync(url).Result;
                        //打印请求结果
                        Console.WriteLine(response);
                        LogHelper.LogMessage(response.ToString());
                        OpenApp();
                        AoceManager.Instance.Getinit();
                        while (!bExit)
                        {
                            System.Threading.Thread.Sleep(10);                            
                        }
                        //SettingManager.Instance.SaveSetting();
                        //Console.ReadLine();
                    }
                    //退出当前程序
                    Environment.Exit(0);
                    Application.Exit();
                }
                else
                {
                    //创建启动对象 
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    //设置运行文件 
                    startInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
                    //设置启动参数 
                    //startInfo.Arguments = String.Join(" ", Args);
                    //设置启动动作,确保以管理员身份运行 
                    startInfo.Verb = "runas";
                    //如果不是管理员，则启动UAC 
                    Process.Start(startInfo);
                    //退出 
                    System.Windows.Forms.Application.Exit();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("web app start error:", ex);
                LogUtility.Log("web app start error:" + ex, LogType.Trace);
                //Console.ReadLine();
            }
        }

        public static string AppUri()
        {
            var appUri = ConfigurationManager.AppSettings["appuri"];
            if (SettingManager.Instance.Setting.launchpadSetting.IsInternal)
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["testappuri"]))
                {
                    appUri = ConfigurationManager.AppSettings["testappuri"];
                }
            }
            return appUri;
        }
    }
}
