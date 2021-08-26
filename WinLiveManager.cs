using OeipCommon;
using OeipCommon.FileTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Threading;
/*using ZmfWrapper.Live;
using ZmfWrapper;*/
using System.Reflection;
using System.Windows.Forms;
using System.IO.Compression;
using System.Net.Http;
using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using AudioSwitcher.AudioApi.CoreAudio;
using static WinLiveManage.NewLoginfo;
using System.Management;
using OpenHardwareMonitor.Hardware;
using Test2.WinRing0;
using Spire.Doc;
using System.Security.AccessControl;
using static OeipCommon.FileTransfer.Mixed;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;
using TaskScheduler;
using Microsoft.Win32.TaskScheduler;
using Microsoft.Win32;
using static OeipCommon.FileTransfer.UpLoginfo_;
using static OeipCommon.FileTransfer.Download_failed;
using static OeipCommon.FileTransfer.All_LiveInfo;
using static WinLiveManage.RunTimeInterFase;
using System.Xml;
using System.ComponentModel;
using static OeipCommon.FileTransfer.LSinfo;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using AocePackage;
using System.Timers;

namespace WinLiveManage
{

    [Serializable]

    public class ProgramInfo
    {

        public string internal_update_address = string.Empty;
        public string update_address = string.Empty;
        public string app_id = string.Empty;
        public string app_name = string.Empty;
        public string version = string.Empty;
        public string server_exe_name = string.Empty;

        public string GetUpdateAddress()
        {
            if (SettingManager.Instance.Setting.launchpadSetting.IsInternal && AppManage.Instance.InnerNet)
                return internal_update_address;
            return update_address;
        }
    }


    [Serializable]
    public enum LauncherMode
    {

        //得不到服务器地址
        [JsonProperty("inactive")]
        Inactive,
        //还没有安装
        [JsonProperty("install")]
        Install,
        //需要更新
        [JsonProperty("update")]
        Update,
        //可以运行
        [JsonProperty("run")]
        Run,
        //文件错误
        [JsonProperty("file_error")]
        file_error,
    }

    [Serializable]
    public class LiveProcessInfo
    {

        public bool bOpen = false;
        public string processName = string.Empty;
        public string appName = string.Empty;
    }

    [Serializable]
    public class WindowLiveInfo
    {
        public float fps = 0;
        public float bitrate = 0;
        public string windowTitle = string.Empty;
        public bool bCapture = false;
    }

    /// <summary>
    /// 给前端展示用
    /// </summary>
    [Serializable]
    public class AppProgram
    {
        public string id = string.Empty;
        public string remoteVersion = string.Empty;
        public string localVersion = string.Empty;
        public LauncherMode launcherMode = LauncherMode.Inactive;
    }

    public class WinLiveManager : OeipCommon.MSingleton<WinLiveManager>
    {
        private object obj = new object();
        private object renderObj = new object();
        private Process gameProcess = null;
        private LiveProcessInfo liveProcessInfo = new LiveProcessInfo();
        private List<LiveProgram> livePrograms = new List<LiveProgram>();
        private string lessonId = string.Empty;
        private WindowLiveInfo windowLiveInfo = new WindowLiveInfo();
        //private bool bOldUpdate = false;
        private Version localVersion = null;
        private bool forceOpen = false;
        //记录当前运行程序的路径
        //private LiveProgram runProgram = null;
        private string gamePath = string.Empty;
        private int updateFileCount = 0;
        private int updateFileDelta = 100;
        private bool bInit = false;
        public bool BOpenLive = false;
        private string WindowsName = "PlaySceneWindow";
        private string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        protected override void Init()
        {
            lock (obj)
            {
                LiveAppHub.SendBackMessage(1000, "用户开始程序");
                localVersion = GetType().Assembly.GetName().Version;
                LogHelper.LogMessage("当前后台版本:" + localVersion);
                bool bTestEnv = SettingManager.Instance.UserInfo.bTestEnv;
                LogHelper.LogMessage($"CDN以{ (bTestEnv ? "测试" : "正式")}环境启动");
                /* 环境变量写入   ExecuteWithOutput();*/
                File_permission();
                ForeachFile(currentDir);
                System.Timers.Timer aTimer = new System.Timers.Timer();
                aTimer.Elapsed += Gets;
                aTimer.Interval = 30;
                aTimer.Enabled = true;
                bInit = true;
            }
        }
        private void Gets(object source, ElapsedEventArgs e)
        {
            lock (obj)
            {
                bool bRender = AoceManager.Instance.Tick();
                if (!bRender)
                {
                   /* LiveAppHub.SendMessage("窗体下沉，请尝试手动置顶",OeipLogLevel.OEIP_WARN);*/
                    // AoceManager.Instance.LiveWindow = null;
                }
            }
        }
        /// <summary>
        /// 每8秒进入一次
        /// </summary>
        /// <param name="bOpen">是否开始课程</param>
        /// <param name="lessonId">课程ID</param>
        public void loadWindows(bool bOpen, string lessonId)
        {
            lock (obj)
            {
                LogHelper.LogMessage("8秒监测状态"+bOpen+"课件ID："+lessonId);
                if (!bOpen)
                {
                    LogHelper.LogMessage("8秒监测课程已经关闭:"+bOpen);
                    CloseLiveS();
                    return;
                }
                //已经在推流
                if (bOpen && AoceManager.Instance.LiveWindow == null)
                {
                    LogHelper.LogMessage("8秒监测课程已经开启但是窗口没有找到:" + bOpen+"---Wind:"+ AoceManager.Instance.LiveWindow);
                    AoceManager.Instance.StartCapture(WindowsName);
                    return;
                }
                //先查找课件进程是否存在
                if (!IsOpened)
                {
                    LiveAppHub.SendMessage($"课件还没有打开,请先运行程序才能推流.", OeipLogLevel.OEIP_WARN);
                    LogHelper.LogMessage("8课件还没有打开,请先运行程序才能推流");
                    return;
                }
                OpenLive(lessonId, 1);
            }

        }
        /// <summary>
        /// 开始推流
        /// </summary>
        /// <param name="lessonId">课程ID</param>
        /// <param name="liveMode">默认为1 代表声网</param>
        private void OpenLive(string lessonId, int liveMode)
        {
            LogHelper.LogMessage("准备进入推流环节：" + lessonId+"----LiveMode:"+liveMode+"是否开启课程状态:"+BOpenLive);
            if (!BOpenLive)
            {
                AoceManager.Instance.LoginLive(lessonId, SettingManager.Instance.UserInfo.bTestEnv);
                bool Btest = SettingManager.Instance.UserInfo.bTestEnv;
                LiveAppHub.SendMessage("用户开始推流", OeipLogLevel.OEIP_INFO, 2000);
                LogHelper.LogMessage("用户开始推流" + (Btest ? "测试环境" : "正式环境"));
                LogUtility.Log("用户开始推流以" + (Btest ? "测试环境" : "正式环境"), LogType.Trace);
            }
            else {
                LogHelper.LogMessage("用户没有正常进行推流,当前OpenLive状态:"+ BOpenLive);
            }

        }
        /// <summary>
        /// 结束推流 
        /// </summary>
        public void CloseLiveS()
        {
            lock (obj) {
                if (AoceManager.Instance.BPush)
                {
                    AoceManager.Instance.LogoutLive();
                    BOpenLive = false;
                    LogHelper.LogMessage("用户关闭推流");
                    LogUtility.Log("用户关闭推流", LogType.Trace);
                }
                else {
                    LogHelper.LogMessage("用户没用进行正常关闭推流---当前BPush状态："+ AoceManager.Instance.BPush+"进程强制关闭");
                     AoceManager.Instance.LogoutLive();
                }
            }
        }
        /// <summary>
        /// 文件权限判断
        /// </summary>
        private void File_permission()
        {
            try { 
            string TestPath = @"D:\IVREAL_MR_Test";
            string NotTestPath = @"D:\IVREAL_MR";
            DirectorySecurity dirSec = Directory.GetAccessControl(SettingManager.Instance.UserInfo.bTestEnv ? TestPath : NotTestPath, AccessControlSections.All);
            List<string> IFEver = new List<string>();
            AuthorizationRuleCollection rules = dirSec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
            foreach (FileSystemAccessRule rule in rules)
            {
                IFEver.Add(rule.IdentityReference + "");
            }
            bool FileQx =false;
            if (IFEver.Count() > 0)
            {
                for (var i = 0; i < IFEver.Count(); i++)
                {
                    if (IFEver[i].ToString().Equals("Everyone"))
                    {
                        FileQx =true;
                        Console.WriteLine(IFEver[i].ToString());
                        LogHelper.LogMessage(SettingManager.Instance.UserInfo.bTestEnv ? TestPath : NotTestPath + "写入文件EveryOne权限");
                        break;
                    }
                }
            }
            if (!FileQx) {
                if (SettingManager.Instance.UserInfo.bTestEnv)
                {
                    Console.WriteLine("测试环境执行权限写入");
                    SetFileRole(currentDir);
                    SetFileRole(TestPath);
                }
                else {
                    Console.WriteLine("正式环境执行权限写入");
                    SetFileRole(currentDir);
                    SetFileRole(NotTestPath);
                }
            }
            }
            catch (Exception ex) {
                LogHelper.LogMessage("Failed to execute file permission " + ex);
            }
        }
        /// <summary>
        /// 写入环境变量 Cuda文件
        /// </summary>
        /// 
        private void ConsoleEvar()
        {
            try
            {
                ////765235832 文件大小
                string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string SetPath = Path.Combine(currentDir, "CudaVar");
                string SetVarLog = Path.Combine(currentDir, "SystEmVar");
                string SetTemp = string.Empty;
                if (Directory.Exists(SetPath))
                {
                    if (!GetVarInfo.Contains(SetPath)&&!Directory.Exists(SetVarLog))
                    {
                        SetTemp = Environment.GetEnvironmentVariable("Path");
                        Environment.SetEnvironmentVariable("Path", SetTemp + ";" + SetPath+ @";%SystemRoot%\system32;%SystemRoot%;%SystemRoot%\System32\Wbem;C:\Windows\SysWOW64", EnvironmentVariableTarget.Machine);
                        LiveAppHub.SendMessage("植入环境变量成功!", OeipLogLevel.OEIP_INFO);
                        LogHelper.LogMessage("环境变量写入成功");
                        ClassInfoS.Log(SetTemp + ";" + SetPath);
                        return;
                    }
                }
                else {
                    LogHelper.LogMessage("没有找到CudaVar文件");
                    strinfo.Add("<<没有找到CudaVar文件请联系，工作人员进行处理>>");
                }
            }
            catch (Exception ex)
            {
                strinfo.Add("<<CudaVar文件环境变量写入失败 !!!>>");
                LogHelper.LogMessage("System environment variable implantation failed ：" + ex);
            }
        }
        /// <summary>
        /// 保存Path 系统变量里的内容
        /// </summary>
        private string GetVarInfo = string.Empty;
        /// <summary>
        /// 输入CMD 命令 获取 Path环境变量内容
        /// </summary>
        /// <param name="command">环境变量名</param>
        public void ExecuteWithOutput()
        {
            try { 
            var processInfo = new ProcessStartInfo("cmd.exe", "/S /C " + "set path")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true
            };
            var process = new Process { StartInfo = processInfo };
            process.Start();
            var outpup = process.StandardOutput.ReadToEnd();
            GetVarInfo = outpup;
            process.WaitForExit();
            process.Close();
            ConsoleEvar();
            }
            catch (Exception ex) {
                LogHelper.LogMessage("System environment variable implantation failed ： " + ex);
            }
        }
        public void SetLivePorgrams(List<LiveProgram> livePrograms)
        {
            List<LiveProgram> programs = new List<LiveProgram>();
            lock (obj)
            {
                this.livePrograms = livePrograms;
                programs.AddRange(livePrograms);
            }
            foreach (var program in programs)
            {
                program.Request.OnDownloadProgressEvent += (ProgressArgs progressArg, ProgressType progressType) =>
                {
                    Request_OnDownloadProgressEvent(program, progressArg, progressType);
                };
            }
        }

        private void Request_OnDownloadProgressEvent(LiveProgram program, ProgressArgs arg1, ProgressType arg2)
        {
            if (program == null)
                return;
            //更新课件文件时,发送频率过高过快
            if (arg2 == ProgressType.File)
            {
                //每隔updateFileDelta次发送一次
                if (updateFileCount++ % updateFileDelta != 0)
                    return;
            }
            else
            {
                updateFileCount = 0;
            }
            LiveAppHub.Hub.Clients.All.DownFile(program.Info.app_id, arg1, arg2);
        }

        private bool checkPid(string pid, out LiveProgram liveProgram)
        {

            lock (obj)
            {
                try
                {
                    if (livePrograms == null || livePrograms.Count <= 0)
                    {
                        liveProgram = null;
                        LogHelper.LogMessage("课件列表为空", OeipLogLevel.OEIP_ERROR);
                        return false;
                    }
                }
                catch { }
                liveProgram = livePrograms.First(p => p.Info.app_id == pid);
                if (pid == null)
                {
                    LogHelper.LogMessage("pid:" + pid + " no program.", OeipLogLevel.OEIP_ERROR);
                    return false;
                }
                return true;
            }
        }

        //判断课件
        public void RunProgram(string pid, LauncherMode launcherMode, string args)
        {
            if (checkPid(pid, out LiveProgram liveProgram))
            {//判断有可安装和更新的包体
                if (launcherMode == LauncherMode.Install || launcherMode == LauncherMode.Update)
                {
                    liveProgram.Install();
                    LiveAppHub.Hub.Clients.All.DownComplete(pid);
                }//正常可以运行
                else if (launcherMode == LauncherMode.Run)
                {
                    if (liveProgram.CheckProcess())
                    {
                        RunProcess(liveProgram, args);
                    }//找到无法正常开启的课件进行重新下载后自动打开
                    else
                    {
                        liveProgram.Install();
                        if (liveProgram.CheckProcess())
                        {
                            RunProcess(liveProgram, args);
                        }
                    }
                }//文件错误并验证错误文件进行更新
                else if (launcherMode == LauncherMode.file_error)
                {
                    liveProgram.Verify();
                    LiveAppHub.Hub.Clients.All.DownComplete(pid);
                }
            }
        }

        /// <summary>
        /// 课件全部更新
        /// </summary>
        /// <param name="type"></param>
        public void AllUpdate(int type)
        {
            WinLiveManager.Instance.SetLivePorgrams(livePrograms);
            List<LiveProgram> programs = new List<LiveProgram>();
            lock (obj)
            {
                programs.AddRange(livePrograms);
            }
            //并行统一更新 可使用 写法 Parallel.ForEach()可用于并行同步下载
            /* Parallel.ForEach()*/
            foreach (var program in programs)
            {
                bool bMatch = false;
                if (program.LauncherMode == LauncherMode.Install || program.LauncherMode == LauncherMode.Update || program.LauncherMode == LauncherMode.file_error)
                {
                    if (type == 0)
                        bMatch = true;
                    else if (type == 1 && program.LauncherMode == LauncherMode.Install)
                        bMatch = true;
                    else if (type == 2 && program.LauncherMode == LauncherMode.Update)
                        bMatch = true;
                    else if (type == 4 && program.LauncherMode == LauncherMode.file_error)
                        bMatch = true;
                }
                if (bMatch)
                {
                    program.Install();
                    LiveAppHub.Hub.Clients.All.DownComplete(program.Info.app_id);
                }
            }
        }
        /// <summary>
        /// 打开运行库安装包
        /// </summary>
        public void InstallRunTime()
        {
            if (bInit == true)
            {
                try
                {
                    RunTimeInterFase inter = new Runtime();
                    if (inter.Runtim_().ToString() != "")
                    {

                    }
                    else
                    {
                        LiveAppHub.SendMessage("打开运行库安装程序失败", OeipLogLevel.OEIP_ERROR, 5000);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessageEx("找不到运行库合集安装程序：", ex);
                }
            }
            else
            {
                LiveAppHub.SendMessage("正在初始化,请稍后尝试", OeipLogLevel.OEIP_ERROR, 5000);
            }
        }
        /// <summary>
        /// 检测历史版本VS运行库是否缺失
        /// </summary>
        public void GetRuntime()
        {
            try
            {
                if (bInit == true)
                {
                    string strname1 = "";
                    string strname2 = "";
                    string strname3 = "";
                    string strname4 = "";
                    List<string> vs2005 = new List<string>();
                    List<string> vs2008 = new List<string>();
                    List<string> vs2010 = new List<string>();
                    List<string> vs2012 = new List<string>();
                    List<string> vs2013 = new List<string>();
                    List<string> vs2019 = new List<string>();
                    using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false))
                    {
                        if (key != null)
                        {
                            foreach (string keyName in key.GetSubKeyNames())
                            {
                                using (RegistryKey key2 = key.OpenSubKey(keyName, false))
                                {
                                    if (key2 != null)
                                    {
                                        string softwareName = key2.GetValue("DisplayName", "").ToString();
                                        if (softwareName.Contains("Microsoft"))
                                        {
                                            strname1 = softwareName;

                                            if (strname1.Contains("Visual"))
                                            {
                                                strname2 = strname1;

                                                if (strname2.Contains("C++"))
                                                {
                                                    strname3 = strname2;
                                                    if (strname3.Contains("2019"))
                                                    {
                                                        strname4 = strname3;
                                                        vs2019.Add(strname4);
                                                    }
                                                    else if (strname3.Contains("2005"))
                                                    {
                                                        vs2005.Add(strname3);
                                                    }
                                                    else if (strname3.Contains("2008"))
                                                    {
                                                        vs2008.Add(strname3);
                                                    }
                                                    else if (strname3.Contains("2010"))
                                                    {
                                                        vs2010.Add(strname3);
                                                    }
                                                    else if (strname3.Contains("2012"))
                                                    {
                                                        vs2012.Add(strname3);
                                                    }
                                                    else if (strname3.Contains("2013"))
                                                    {
                                                        vs2013.Add(strname3);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            LogHelper.LogMessage("*********************此注册表无法获取当前计算机运行库列表");
                        }
                        if (vs2019.Count >= 2)
                        {
                        }
                        else
                        {
                            /* LiveAppHub.SendMessage("检测到当前电脑vs2019运行库，缺失请联系FEA工作人员安装", OeipLogLevel.OEIP_ERROR);*/
                            LogUtility.Log("当前电脑未检vs2019运行库缺失", LogType.Error);
                            strinfo.Add("检测到当前电脑vs2019运行库缺失请点击上方红色按钮《下载运行库合集》进行安装");
                        }
                        if (vs2005.Count >= 1)
                        {
                        }
                        else
                        {
                            /* LiveAppHub.SendMessage("检测到当前电脑vs2005运行库，缺失请联系FEA工作人员安装", OeipLogLevel.OEIP_ERROR);*/
                            LogUtility.Log("当前电脑未检vs2005运行库缺失", LogType.Error);
                            strinfo.Add("检测到当前电脑vs2005运行库缺失请点击上方红色按钮《下载运行库合集》进行安装");
                        }
                        if (vs2008.Count >= 1)
                        {
                        }
                        else
                        {
                            /*  LiveAppHub.SendMessage("检测到当前电脑vs2008运行库，缺失请联系FEA工作人员安装", OeipLogLevel.OEIP_ERROR);*/
                            LogUtility.Log("当前电脑未检vs2008运行库缺失", LogType.Error);
                            strinfo.Add("检测到当前电脑vs2008运行库缺失请点击上方红色按钮《下载运行库合集》进行安装");
                        }
                        if (vs2010.Count >= 1)
                        {
                        }
                        else
                        {
                            /* LiveAppHub.SendMessage("检测到当前电脑vs2010运行库，缺失请联系FEA工作人员安装", OeipLogLevel.OEIP_ERROR);*/
                            LogUtility.Log("当前电脑未检vs2010运行库缺失", LogType.Error);
                            strinfo.Add("检测到当前电脑vs2010运行库缺失请点击上方红色按钮《下载运行库合集》进行安装");
                        }
                        if (vs2012.Count >= 2)
                        {
                        }
                        else
                        {
                            /* LiveAppHub.SendMessage("检测到当前电脑vs2012运行库，缺失请联系FEA工作人员安装", OeipLogLevel.OEIP_ERROR);*/
                            LogUtility.Log("当前电脑未检vs2012运行库缺失", LogType.Error);
                            strinfo.Add("检测到当前电脑vs2012运行库缺失请点击上方红色按钮《下载运行库合集》进行安装");
                        }
                        if (vs2013.Count >= 2)
                        {
                        }
                        else
                        {
                            /* LiveAppHub.SendMessage("检测到当前电脑VS2013运行库，缺失请联系FEA工作人员安装", OeipLogLevel.OEIP_ERROR);*/
                            LogUtility.Log("当前电脑未检vs2013运行库缺失", LogType.Error);
                            strinfo.Add("检测到当前电脑VS2013运行库缺失请点击上方红色按钮《下载运行库合集》进行安装");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage("运行库监测时错误:" + ex);
            }
        }
        /// <summary>
        /// 监测当前电脑是否存在USB 3.0接口
        /// </summary>
        public void GetUsbs()
        {
            List<String> Get3 = new List<string>();
            string exist = "";
            ManagementObjectCollection ollection;
            using (var searchar = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                ollection = searchar.Get();
            foreach (var device in ollection)
            {
                Get3.Add(device["Description"].ToString().Trim());
            }
            if (Get3.Count > 0)
            {
                for (var i = 0; i < Get3.Count(); i++)
                {
                    string newstr = Get3[i];
                    if (newstr.Contains("(USB 3.0)"))
                    {
                        exist = newstr;
                        GetUsbJk();
                    }
                }

                if (exist.Length <= 0)
                {
                    LiveAppHub.SendMessage("当前电脑未检测到USB 3.0接口存在,可能会导致头盔无法使用,如果能够使用请忽略此信息", OeipLogLevel.OEIP_ERROR, 5000);
                    LogUtility.Log("当前电脑未检测到USB 3.0接口存在", LogType.Error);
                }

            }
        }
        /// <summary>
        /// 头盔是否加入USB 3.0接口
        /// </summary>
        public void GetUsbJk()
        {
            string Getjk = "";
            List<string> USBlist = new List<string>();
            ManagementObjectCollection ollection;
            using (var serchar = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                ollection = serchar.Get();
            foreach (var device in ollection)
            {
                USBlist.Add(device["Description"].ToString().Trim());
            }
            if (USBlist.Count > 0)
            {
                for (var i = 0; i < USBlist.Count; i++)
                {
                    string Usbinfo = USBlist[i];
                    if (Usbinfo.Contains("SuperSpeed"))
                    {
                        Getjk = Usbinfo;
                    }
                }
                if (Getjk.Length <= 0)
                {
                    LiveAppHub.SendMessage("未检测到头盔插入USB 3.0接口请检查", OeipLogLevel.OEIP_ERROR, 5000);
                    LogUtility.Log("未检测到头盔插入3.0接口信号", LogType.Error);
                }
            }
        }


        /// <summary>
        /// USB 头盔监测
        /// </summary>
        public void GetAudio()
        {
            if (bInit == true)
            {
                List<string> Audios = new List<string>();
                string GetUsb = "";
                string uss = SettingManager.Instance.UserInfo.role;
                try
                {
                    ManagementObjectSearcher VoiceDeviceSearcher = new ManagementObjectSearcher("select * from Win32_SoundDevice");//声明一个用于检索设备管理信息的对象
                    foreach (ManagementObject VoiceDeviceObject in VoiceDeviceSearcher.Get())//循环遍历WMI实例中的每一个对象
                    {
                        // VoiceDeviceObject["ProductName"].ToString(); //在当前文本框中显示声音设备的名称
                        //  VoiceDeviceObject["PNPDeviceID"].ToString();//在当前文本框中显示声音设备的PNPDeviceID
                        Audios.Add(VoiceDeviceObject["ProductName"].ToString());
                    }
                    if (uss != "teacher")
                    {
                        if (Audios.Count > 0)
                        {
                            for (var i = 0; i < Audios.Count; i++)
                            {
                                string USBinfo = Audios[i];
                                if (USBinfo.Contains("USB"))
                                {
                                    GetUsb = USBinfo;
                                    GetUsbs();
                                }
                            }
                            if (GetUsb.Length <= 0)
                            {
                                LiveAppHub.SendMessage("获取VR头盔失败，请检查", OeipLogLevel.OEIP_ERROR, 5000);
                                LogUtility.Log("未检测到头盔USB信号", LogType.Error);
                            }
                            else
                            {
                                GetStudentAudio();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.Log("获取头盔USB信息失败原因:" + ex, LogType.Error);
                }
            }
        }
        /// <summary>
        /// 定义开机启动任务名称
        /// </summary>
        public string TaskName = "Login";
        /// <summary>
        /// 开机自启
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="fileName">文件路径</param>
        /// <param name="description">任务描述</param>
        public static void AutoStart(string taskName, string fileName, string description)
        {

            if (string.IsNullOrEmpty(taskName) || string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException();
            }
            string TaskName = taskName;
            var logonUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string taskDescription = description;
            string deamonFileName = fileName;

            using (var taskService = new TaskService())
            {
                var tasks = taskService.RootFolder.GetTasks(new System.Text.RegularExpressions.Regex(TaskName));
                foreach (var t in tasks)
                {
                    taskService.RootFolder.DeleteTask(t.Name);
                }
                var task = taskService.NewTask();
                task.RegistrationInfo.Description = taskDescription;
                task.Settings.DisallowStartIfOnBatteries = false;//当使用电源时，也运行此计划任务
                task.Triggers.Add(new LogonTrigger { UserId = logonUser });
                task.Principal.RunLevel = TaskRunLevel.Highest;
                task.Actions.Add(new ExecAction(deamonFileName));
                taskService.RootFolder.RegisterTaskDefinition(TaskName, task);
                LiveAppHub.SendMessage("已开启", OeipLogLevel.OEIP_INFO, 5000);
            }
        }
        //关闭开机自启并删除任务
        public void DeleteTask(string taskName)
        {
            if (bInit == true)
            {
                TaskSchedulerClass ts = new TaskSchedulerClass();
                ts.Connect(null, null, null, null);
                ITaskFolder folder = ts.GetFolder("\\");
                folder.DeleteTask(taskName, 0);
                LiveAppHub.SendMessage("已关闭", OeipLogLevel.OEIP_INFO, 5000);
            }
        }
        public class Getssinfo_s
        {
            public string infos_str = string.Empty;
        }
        /// <summary>
        /// 改变传统全部更新方式
        /// </summary>
        public Getssinfo_s GetSSInfo()
        {
            Getssinfo_s infos = new Getssinfo_s();
            if (bInit == true)
            {
                string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string selflogs2 = Path.Combine(currentDirs, "Download_Failed/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                string selflogs3 = Path.Combine(currentDirs, "LsInfo/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                if (File.Exists(selflogs2))
                {
                    try
                    {
                        var lines_ = File.ReadAllLines(selflogs2);
                        DeleteBlank_Download_Failed();
                        var query_ = lines_.ToArray();

                        var XieH = "";
                        if (File.Exists(selflogs2) && !File.Exists(selflogs3))
                        {
                            if (query_.Count() > 0)
                            {
                                for (var i = 0; i < query_.Count(); i++)
                                {
                                    XieH = query_[0];
                                }
                                infos.infos_str = XieH;
                            }
                            else
                            {
                                File.Delete(selflogs2);
                            }
                        }
                    }
                    catch { }
                }
            }
            return infos;
        }
        /// <summary>
        /// 删除info文件 删除Download文件第一行
        /// </summary>
        public void Delete_infoStr()
        {
            lock (obj)
            {
                if (bInit == true)
                {
                    try
                    {
                        string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string selflogs2 = Path.Combine(currentDirs, "Download_Failed/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                        string selflogs3 = Path.Combine(currentDirs, "LsInfo/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                        var XieH = string.Empty;
                        if (File.Exists(selflogs2) && File.Exists(selflogs3))
                        {
                            var lines_ = File.ReadAllLines(selflogs2);
                            var lines = File.ReadAllLines(selflogs3);
                            DeleteBlank_Download_Failed();
                            var query_ = lines_.ToArray();
                            var query = lines.ToArray();
                            if (query.Count() > 0)
                            {
                                for (var i = 0; i < query.Count(); i++)
                                {
                                    XieH = query[0];
                                }
                                if (XieH.Contains("完成"))
                                {
                                    File.Delete(selflogs3);
                                    if (query_.Count() > 0)
                                    {
                                        Delete_Row_Download_Failed();
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }
        /// <summary>
        /// 杀死游戏进程
        /// </summary>
        /// <param name="processName">进程名称</param>
        private static void KillProcess(string processName)
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName.Contains(processName))
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(); // possibly with a timeout
                        Console.WriteLine($"已杀{processName}进程");
                    }
                    catch (Win32Exception e)
                    {
                        Console.WriteLine(e.Message.ToString());
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine(e.Message.ToString());
                    }
                }
            }
        }
        private void Delete_Row_Download_Failed()
        {
            string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string selflogs1 = Path.Combine(currentDirs, "Download_Failed/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
            StringBuilder Buider_ = new StringBuilder();
            FileStream FileStr = new FileStream(selflogs1, FileMode.Open, FileAccess.Read);
            StreamReader StrRed = new StreamReader(FileStr);
            string GetStrRed = StrRed.ReadLine();
            int Xall = 0;
            while (GetStrRed != null)
            {
                Xall++;
                if (Xall > 1) //只添加第1行以后的数据
                {
                    Buider_.AppendLine(GetStrRed);
                }
                GetStrRed = StrRed.ReadLine();
            }
            StrRed.Close();
            FileStr.Close();
            FileStream fsWrite = new FileStream(selflogs1, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter StrWrit = new StreamWriter(fsWrite);
            StrWrit.Write(Buider_.ToString());
            StrWrit.Close();
            fsWrite.Close();
        }
        /// <summary>
        /// 存储EXE文件路径
        /// </summary>
        private string ExePathAll = string.Empty;
        /// <summary>
        /// 存储服务器临时MD5
        /// </summary>
        private string SeverMD5 = string.Empty;
        /// <summary>
        /// 文件完整性监测最新版本
        /// </summary>
        /// 
        public void NewTest_()
        {

            if (bInit == true)
            {
                lock (obj)
                {
                    try
                    {
                        string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string selflogs2 = Path.Combine(currentDirs, "Download_Failed/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                        string selflogs3 = Path.Combine(currentDirs, "LsInfo/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                        if (File.Exists(selflogs3))
                        {
                            var lines_ = File.ReadAllLines(selflogs3);
                            var query_ = lines_.ToArray();
                            var lsinfo = "";
                            if (query_.Count() > 0)
                            {
                                for (var s = 0; s < query_.Count(); s++)
                                {
                                    lsinfo = query_[0];
                                }
                                if (lsinfo.Contains("完成"))
                                {

                                    if (File.Exists(selflogs2))
                                    {
                                        var lines = File.ReadAllLines(selflogs2);
                                        var query = lines.ToArray();
                                        DeleteBlank_Download_Failed();
                                        var ExeName = "";
                                        var ExePath = "";
                                        var ThreeStr = "";
                                        var Errors = "";
                                        var ExePaths = "";
                                        var ExePaths_s = "";
                                        var fullStr = "";
                                        if (query.Count() >= 3)
                                        {
                                            for (var i = 0; i < query.Count(); i++)
                                            {
                                                ExeName = query[0];
                                                ExePath = query[1];
                                                ThreeStr = query[2];
                                                SeverMD5 = ExePath;
                                                if (query[i].ToString().Contains("SeverLength"))
                                                {
                                                    ExePaths = ExePath.Split(':')[2];
                                                    ExePaths_s = "D:" + ExePaths;
                                                    ExePathAll = ExePaths_s;
                                                    if (query[i].Split(':')[1] != query[i].Split(':')[3])
                                                    {
                                                        //字符串截取 定位开始坐标
                                                        int To_coordinate = ExePaths_s.IndexOf("D");
                                                        //字符串截取 定位结束坐标
                                                        int End_coordinates = ExePaths_s.IndexOf("Game");
                                                        //整合字符串
                                                        fullStr = ExePaths_s.Substring(To_coordinate, End_coordinates - To_coordinate + 2);
                                                        //去除结尾多余字符串
                                                        fullStr = fullStr.Substring(0, fullStr.Length - 3);
                                                        Errors = ExeName + "课件：" + query[i] + ":下载失败请重新下载";
                                                        break;
                                                    }
                                                    if (ExeName.Contains("SeverLength"))
                                                    {
                                                        Errors = ExeName + "位置：" + ":下载失败请重新下载";
                                                        break;
                                                    }
                                                    if (ExeName.Contains("本地存放路径"))
                                                    {
                                                        Errors = ExeName + "位置：" + ":下载失败请重新下载";
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (File.Exists(selflogs2) && File.Exists(selflogs3))
                                                    {
                                                        Errors = ExeName + "位置：" + ":下载失败请重新下载";
                                                        File.Delete(selflogs2);
                                                        File.Delete(selflogs3);
                                                    }
                                                    if (File.Exists(selflogs2) && !File.Exists(selflogs3))
                                                    {
                                                        Errors = ExeName + "位置：" + ":下载失败请重新下载";
                                                        File.Delete(selflogs2);
                                                    }
                                                    if (!File.Exists(selflogs2) && File.Exists(selflogs3))
                                                    {
                                                        Errors = ExeName + "位置：" + ":下载失败请重新下载";
                                                        File.Delete(selflogs3);
                                                    }
                                                }
                                            }
                                            SeverMD5 = SeverMD5.Split(':')[4];
                                            string SeverMD5_ = ComputeMD5(ExePathAll).ToUpper();
                                            if (SeverMD5_ == SeverMD5 && Errors == "")
                                            {
                                                LiveAppHub.SendMessage("1下载成功", OeipLogLevel.OEIP_INFO, 5000);
                                                if (File.Exists(selflogs2) && File.Exists(selflogs3))
                                                {
                                                    File.Delete(selflogs2);
                                                    File.Delete(selflogs3);
                                                }
                                            }
                                            else
                                            {
                                                if (Errors == "")
                                                {
                                                    if (Directory.Exists(fullStr) && File.Exists(selflogs2))
                                                    {
                                                        LiveAppHub.SendMessage("2下载错误,请重新下载", OeipLogLevel.OEIP_ERROR, 5000);
                                                        File.Delete(selflogs2);
                                                        Directory.Delete(fullStr, true);
                                                    }
                                                }
                                                else
                                                {
                                                    if (File.Exists(selflogs2))
                                                    {
                                                        LiveAppHub.SendMessage(Errors, OeipLogLevel.OEIP_ERROR, 5000);
                                                        File.Delete(selflogs2);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (File.Exists(selflogs2) && Network_swallow < 1)
                        {
                            var lines = File.ReadAllLines(selflogs2);
                            var query = lines.ToArray();
                            var ExeName = "";
                            var ExePath = "";
                            var Errors = "";
                            var ExePaths = "";
                            var ExePaths_s = "";
                            var fullStr = "";
                            if (query.Count() >= 2)
                            {
                                for (var i = 0; i < query.Count(); i++)
                                {
                                    ExeName = query[0];
                                    ExePath = query[1];
                                    if (query.Count() < 5)
                                    {
                                        ExePaths = ExePath.Split(':')[2];
                                        ExePaths_s = "D:" + ExePaths;
                                        ExePathAll = ExePaths_s;
                                        //字符串截取 定位开始坐标
                                        int To_coordinate = ExePaths_s.IndexOf("D");
                                        //字符串截取 定位结束坐标
                                        int End_coordinates = ExePaths_s.IndexOf("Game");
                                        //整合字符串
                                        fullStr = ExePaths_s.Substring(To_coordinate, End_coordinates - To_coordinate + 2);
                                        //去除结尾多余字符串
                                        fullStr = fullStr.Substring(0, fullStr.Length - 3);
                                        Errors = ExeName + "课件：" + query[i] + ":下载失败请重新下载";
                                        break;
                                    }

                                }
                                if (Errors != "")
                                {
                                    LiveAppHub.SendMessage(Errors + "3下载失败,请重新下载！", OeipLogLevel.OEIP_ERROR, 5000);
                                    LogHelper.LogMessage(ExeName + "课件检测到下载错误,进行自动卸载");
                                    if (Directory.Exists(fullStr) && File.Exists(selflogs2))
                                    {
                                        File.Delete(selflogs2);
                                        Directory.Delete(fullStr, true);

                                    }
                                    else if (!Directory.Exists(fullStr) && File.Exists(selflogs2))
                                    {
                                        File.Delete(selflogs2);
                                    }
                                }
                                /* if (Errors == "")
                                 {
                                     if (File.Exists(selflogs3))
                                     {
                                         var lines_ = File.ReadAllLines(selflogs3);
                                         var query_ = lines_.ToArray();
                                         if (query.Count() > 0)
                                         {
                                             for (var i = 0; i < query_.Count(); i++)
                                             {
                                                 if (query_[0].Contains("完成"))
                                                 {
                                                     LiveAppHub.SendMessage("下载成功", OeipLogLevel.OEIP_ERROR, 5000);
                                                     break;
                                                 }
                                             }
                                         }
                                     }
                                 }*/
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogMessage("监测包体完整性时错误:" + ex);
                    }
                }
            }
        }
        /// <summary>
        /// 加载MD5   
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        private static String ComputeMD5(String fileName)
        {
            String hashMD5 = String.Empty;
            //检查文件是否存在，如果文件存在则进行计算，否则返回空值
            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    //计算文件的MD5值
                    System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
                    Byte[] buffer = calculator.ComputeHash(fs);
                    calculator.Clear();
                    //将字节数组转换成十六进制的字符串形式
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        stringBuilder.Append(buffer[i].ToString("x2"));
                    }
                    hashMD5 = stringBuilder.ToString();
                }//关闭文件流
            }//结束计算
            return hashMD5;
        }//ComputeMD5
        /// <summary>
        /// 去除SeverClassInfo文件空格部分
        /// </summary>
        private void DeleteBlank_SeverClassInfo()
        {
            try
            {
                string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string selflogs1 = Path.Combine(currentDirs, "SeverClassInfo/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                var lines = File.ReadAllLines(selflogs1).Where(arg => !string.IsNullOrWhiteSpace(arg));
                File.WriteAllLines(selflogs1, lines);
            }
            catch (Exception ex)
            {
                LogUtility.Log("删除错误日志空白行失败:" + ex, LogType.Error);
            }
        }
        /// <summary>
        /// 去除Download_Failed文件空格部分
        /// </summary>
        private void DeleteBlank_Download_Failed()
        {
            lock (obj)
            {
                try
                {
                    string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string selflogs1 = Path.Combine(currentDirs, "Download_Failed/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                    var lines = File.ReadAllLines(selflogs1).Where(arg => !string.IsNullOrWhiteSpace(arg));
                    File.WriteAllLines(selflogs1, lines);
                }
                catch (Exception ex)
                {
                    LogUtility.Log("删除错误日志空白行失败:" + ex, LogType.Error);
                }
            }
        }
        /// <summary>
        /// 传入前端开机自启动
        /// </summary>
        public void OpenTask()
        {
            if (bInit == true)
            {
                string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string selflogsr = Path.Combine(currentDir, "WinLiveManage.exe");
                AutoStart(TaskName, $"{selflogsr}", "开机启动课件程序");
            }
        }
        /// <summary>
        /// 关闭开机自启并删除任务
        /// </summary>
        public void CloseTask()
        {
            if (bInit == true)
            {
                DeleteTask(TaskName);
            }
        }
        #region 获取教师端麦克风设备
        public void GetAuDio_Sup()
        {
            lock (obj)
            {
                if (bInit == true)
                {
                    try
                    {
                        string uss = SettingManager.Instance.UserInfo.role;
                        if (uss == "teacher")
                        {
                            List<string> devs = new List<string>();
                            MMDeviceEnumerator enumberator = new MMDeviceEnumerator();
                            MMDeviceCollection deviceCollection = enumberator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All);
                            for (int waveInDevice = 0; waveInDevice < WaveIn.DeviceCount; waveInDevice++)
                            {
                                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                                foreach (MMDevice device in deviceCollection)
                                {
                                    if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                                    {
                                        devs.Add(device.FriendlyName);
                                        break;
                                    }
                                }
                            }
                            if (devs.Count() < 1)
                            {
                                LiveAppHub.SendMessage("当前PC未找到有效麦克风设备,请检查！", OeipLogLevel.OEIP_WARN, 5000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtility.Log("查找系统麦克风设备失败:" + ex);
                    }
                }
            }
        }
        /// <summary>
        /// 查找学生端 麦克风设备 是否存在
        /// </summary>
        private void GetStudentAudio()
        {
            lock (obj)
            {
                if (bInit == true)
                {
                    try
                    {
                        string uss = SettingManager.Instance.UserInfo.role;
                        if (uss != "teacher")
                        {
                            List<string> devs = new List<string>();
                            MMDeviceEnumerator enumberator = new MMDeviceEnumerator();
                            MMDeviceCollection deviceCollection = enumberator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All);
                            for (int waveInDevice = 0; waveInDevice < WaveIn.DeviceCount; waveInDevice++)
                            {
                                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                                foreach (MMDevice device in deviceCollection)
                                {
                                    if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                                    {
                                        devs.Add(device.FriendlyName);
                                        break;
                                    }
                                }
                            }
                            if (devs.Count() < 1)
                            {
                                LiveAppHub.SendMessage("当前PC没有找到有效VR头盔麦克风设备,请检查！", OeipLogLevel.OEIP_WARN, 5000);
                                LogUtility.Log("当前PC没有找到有效VR头盔麦克风设备");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtility.Log("查找系统麦克风设备失败:" + ex);
                    }
                }
            }
        }
        #endregion
        [Serializable]
        public class GetSleep
        {
            public int sleepOpen;
        }
        [Serializable]
        public class GetInfo
        {
            public List<string> Infos;
        }
        /// <summary>
        /// 传入前端错误信息
        /// </summary>
        /// <returns></returns>
        public GetInfo Getinfos()
        {
            GetInfo info = new GetInfo();
            List<string> GetValueinfos = new List<string>();
            try
            {
                for (var i = 0; i < strinfo.Count(); i++)
                {
                    GetValueinfos = strinfo.ToList();
                }
                if (true)
                {
                    //集合输出换行
                    /*for (int s=0;s<GetValueinfos.Count();s++) {
                        if (GetValueinfos[s]==",") {
                            info.Infos.Insert(++s, Environment.NewLine);
                        }
                    }*/
                    info.Infos = GetValueinfos.Distinct().ToList();
                    strinfo.ToList().Clear();
                }
            }
            catch (Exception ex)
            {
                LogUtility.Log("前端错误信息传入错误原因:" + ex, LogType.Error);
            }
            return info;

        }
        /// <summary>
        /// 错误信息临时存放位置  注(仅保存了 没有找到的运行库版本信息)
        /// </summary>
        public List<string> strinfo = new List<string>();


        /// <summary>
        /// 系统版本 
        /// </summary>
        private string _operatingSystem;
        /// <summary>
        /// 操作系统 32位 64位 
        /// </summary>
        private string _osArchitecture;

        [Serializable]

        public class GetSyem
        {
            public List<string> GSyems;
        }
        /// <summary>
        /// 获取系统版本信息
        /// </summary>
        /// <returns></returns>
        public GetSyem GetWinSyesm()
        {
            GetSyem Sye = new GetSyem();
            if (bInit == true)
            {
                try
                {
                    List<string> getlsStems = new List<string>();
                    using (ManagementObjectSearcher win32OperatingSystem = new ManagementObjectSearcher("select * from Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in win32OperatingSystem.Get())
                        {
                            _operatingSystem = obj["Caption"].ToString();
                            _osArchitecture = obj["OSArchitecture"].ToString();
                            break;
                        }
                    }
                    getlsStems.Add(_operatingSystem + _osArchitecture);
                    Sye.GSyems = getlsStems.ToList();
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessage("获取系统版本信息失败" + ex);
                }
            }
            return Sye;
        }
        /// <summary>
        /// 获取PC本地 DireatX版本
        /// </summary>
        /// <returns></returns>
        private static int checkdxversion_dxdiag()
        {
            object objc = new object();
            lock (objc)
            {
                Process.Start("dxdiag", "/x dxv.xml");
                while (!File.Exists("dxv.xml"))
                    Thread.Sleep(1000);
                XmlDocument doc = new XmlDocument();
                doc.Load("dxv.xml");
                XmlNode dxd = doc.SelectSingleNode("//DxDiag");
                XmlNode dxv = dxd.SelectSingleNode("//DirectXVersion");
                return Convert.ToInt32(dxv.InnerText.Split(' ')[1]);
            }
        }
        [Serializable]
        public class GetDxVersion
        {
            public string GetDx = string.Empty;
        }
        /// <summary>
        /// 零食变量 获取 DX 版本信息用作于 关闭课件时记录版本
        /// </summary>
        private string PC_GetDxVersion = string.Empty;
        public GetDxVersion GetDxVer()
        {
            GetDxVersion DXversion = new GetDxVersion();
            lock (obj)
            {
                if (bInit == true)
                {
                    try
                    {
                        Process.Start("dxdiag", "/x dxv.xml");
                        while (!File.Exists("dxv.xml"))
                            Thread.Sleep(1000);
                        XmlDocument doc = new XmlDocument();
                        doc.Load("dxv.xml");
                        XmlNode dxd = doc.SelectSingleNode("//DxDiag");
                        XmlNode dxv = dxd.SelectSingleNode("//DirectXVersion");
                        DXversion.GetDx = "DirectXVersion:" + dxv.InnerText.Split(' ')[1].ToString();
                        PC_GetDxVersion = DXversion.GetDx.ToString();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogMessageEx("获取DX版本信息出错:", ex);
                    }
                }
            }
            return DXversion;
        }
        //休眠时间
        public GetSleep Sleep()
        {
            GetSleep get = new GetSleep();
            if (bInit == true)
            {
                Vpo po = new Vpo();
                get.sleepOpen = Convert.ToInt32(po.sleep());
            }
            return get;
        }
        public void NetWrok_sub()
        {
            if (bInit == true)
            {
                try
                {
                    Network_signal();
                }
                catch (Exception ex)
                {
                    LogUtility.Log("" + ex, LogType.Error);
                }
            }
        }
        public NetWork_wsll NetWord_sw()
        {
            NetWork_wsll work = new NetWork_wsll();
            if (bInit == true)
            {
                work.NetWork_ = Network_swallow;
            }
            return work;
        }
        public NetWork_to Network_to()
        {
            NetWork_to to = new NetWork_to();
            if (bInit == true)
            {
                to.NetWork_to_ = Network_to_vomit;
            }
            return to;
        }
        [Serializable]
        public class NetWork_wsll
        {
            public int NetWork_;
        }
        [Serializable]
        public class NetWork_to
        {
            public int NetWork_to_;
        }
        /// <summary>
        /// 网络吞量
        /// </summary>
        public int Network_swallow;
        public int Network_to_vomit;
        /// <summary>
        /// 当前系统网络吞吐量
        /// </summary>
        public void Network_signal()
        {
            if (bInit == true)
            {
                try
                {
                    var iftable1 = IpHlpApi.GetIfTable();
                    long inSpeed1 = iftable1.Sum(m => m.dwInOctets);
                    long outSpeed1 = iftable1.Sum(m => m.dwOutOctets);
                    Thread.Sleep(1000);
                    var iftable2 = IpHlpApi.GetIfTable();
                    var inSpeed2 = iftable2.Sum(m => m.dwInOctets);
                    var outSpeed2 = iftable2.Sum(m => m.dwOutOctets);
                    var inSpeed = inSpeed2 - inSpeed1;
                    var outSpeed = outSpeed2 - outSpeed1;
                    var ada = IpHlpApi.GetInterfaceInfo();
                    ulong total = 0;
                    foreach (var a in ada.Adapter)
                    {
                        MIB_IF_ROW2 row = new MIB_IF_ROW2(a.Index);
                        IpHlpApi.GetIfEntry2(ref row);
                        if (row.InOctets > 0)
                        {
                            total += row.ReceiveLinkSpeed;
                        }
                    }
                    total = total / 8;
                    // memoryLoad = memoryLoad,
                    //   cpuLoad = cpuLoad,
                    inSpeed = Convert.ToInt32((double)inSpeed * 100 / Convert.ToDouble(total)) * 2;
                    outSpeed = Convert.ToInt32((double)outSpeed * 100 / Convert.ToDouble(total));
                    //吞
                    Network_swallow = Convert.ToInt32(inSpeed);
                    //吐
                    Network_to_vomit = Convert.ToInt32(outSpeed);
                }
                catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }
            }
        }
        private List<string> getpid = new List<string>();
        private int COunts;
        //打开课件目录
        public void OpenProgramPath(string pid)
        {
            COunts = COunts + 1;
            if (COunts > 3)
            {
                LiveAppHub.SendMessage("当前点击频率过快，请稍后尝试", OeipLogLevel.OEIP_WARN, 5000);
                COunts = 0;
            }
            if (checkPid(pid, out LiveProgram liveProgram))
            {
                string path = liveProgram.ProgramPath.Path;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                Process.Start("explorer.exe", path);
                COunts = COunts - 1;
            }
        }
        [Serializable]
        public class DeleteHin
        {
            public string Hint = string.Empty;
        }
        private string GetDeleteHin = string.Empty;
        public DeleteHin Delete_hint(string Hints = "")
        {
            DeleteHin dle = new DeleteHin();
            dle.Hint = GetDeleteHin;
            return dle;
        }
        /// <summary>
        /// 手动卸载指定包体
        /// </summary>
        /// <param name="pid"></param>
        public void DleteFile_Sur(string pid)
        {
            if (bInit == true)
            {
                try
                {
                    if (checkPid(pid, out LiveProgram liveProgram))
                    {
                        string path = liveProgram.ProgramPath.Path;
                        if (Directory.Exists(path))
                        {
                            if (GetDirectoryLength(path) != 0)
                            {

                                Directory.Delete(path, true);
                                GetDeleteHin = "正在卸载";
                                LiveAppHub.SendMessage("正在卸载中", OeipLogLevel.OEIP_INFO);
                            }
                            else
                            {
                                LiveAppHub.SendMessage("此包体已不存在，请勿重复操作", OeipLogLevel.OEIP_WARN);
                            }
                        }
                        else
                        {
                            LiveAppHub.SendMessage("此包体不存在", OeipLogLevel.OEIP_INFO, 5000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessageEx($"{pid}:执行删除包体时出错:", ex);
                }
            }
        }
        //验证文件
        public void VerifyProgram(string pid)
        {
            if (checkPid(pid, out LiveProgram liveProgram))
            {
                liveProgram.Verify();
            }
        }
        [Serializable]
        public class Gettime
        {
            public string NewHour = string.Empty;
        }
        [Serializable]
        public class Gettime_Minute
        {
            public string Minutes = string.Empty;
        }
        //小时数
        public Gettime Newtime()
        {
            Gettime gets = new Gettime();
            if (bInit == true)
            {
                string time = Convert.ToDateTime(DateTime.Now).ToString("yyyy:MM:dd: hh:mm");//12小时制
                string Get1 = time.Split(':')[3];
                gets.NewHour = Get1;
            }
            return gets;
        }
        //当前分钟数
        public Gettime_Minute Newtime_Minute()
        {
            Gettime_Minute gets = new Gettime_Minute();
            if (bInit == true)
            {
                string time = Convert.ToDateTime(DateTime.Now).ToString("yyyy:MM:dd: hh:mm");//12小时制
                string Get1 = time.Split(':')[4];
                gets.Minutes = Get1;
            }
            return gets;
        }
        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware)
                    subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }



        //日志定时清除
        public void CleanFile()
        {
            lock (obj)
            {
                if (bInit == true)
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + "Newlog";
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo dir = new DirectoryInfo(path);
                        FileInfo[] files = dir.GetFiles();
                        foreach (FileInfo file in files)
                        {
                            if (file.LastWriteTime < DateTime.Now.AddDays(-1))
                            {
                                file.Delete();
                            }
                        }
                    }
                }
            }
        }
        //删除UploadLog
        public void ClearUploadLog()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "UploadLog";
            Directory.Delete(path);
        }
        //删除ClearUploadzip
        public void ClearUploadlogZip()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "UploadLog.zip";
            System.IO.File.Delete(path);
        }
        //日志定时清除
        public void CleanFile_logs()
        {
            lock (obj)
            {
                if (bInit == true)
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + "logs";
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo dir = new DirectoryInfo(path);
                        FileInfo[] files = dir.GetFiles();
                        foreach (FileInfo file in files)
                        {
                            if (file.LastWriteTime < DateTime.Now.AddDays(-1))
                            {
                                file.Delete();
                            }
                        }
                    }
                }
            }
        }
        //每次上传日志后清除logs日志
        public void ClearLogs()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "logs";
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
        }
        //每次上传日志后清除Newlog日志
        public void ClearNewlog()
        {
            if (bInit == true)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "NewLog";
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    file.Delete();
                }
            }
        }
        //删除Mixed_Reality_Portal文件多余日志
        public void CleanMixed()
        {
            lock (obj)
            {
                if (bInit == true)
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + "Mixed_Reality_Portal";
                    DirectoryInfo dir = new DirectoryInfo(path);
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (file.LastWriteTime < DateTime.Now.AddDays(-1))
                        {
                            file.Delete();
                        }
                    }
                }
            }
        }
        private Process GetProcess(string gameName)
        {
            try
            {
                var processArray = Process.GetProcesses();
                foreach (var porcess in processArray)
                {
                    if (porcess.ProcessName == gameName && !porcess.HasExited)
                    {
                        LogHelper.LogMessage("找到进程" + gameName);
                        return porcess;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx($"GetProcess{gameName} error", ex);
            }
            return null;
        }
        public HardwareInfo hardwareInfo = new HardwareInfo();
        //课件运行过程
        private void RunProcess(LiveProgram liveProgram, string args)
        {
            try
            {
                var setting = SettingManager.Instance.Setting;
                var userInfo = SettingManager.Instance.UserInfo;
                string defaultPath = SettingManager.Instance.GetPathSetting().SettingPath;
                var gameArguments = " -loginname=" + userInfo.name + " -loginpassword=" + userInfo.password +
                    " -logintype=" + (userInfo.IsTeacher() ? 4 : 2) + " -ivrealtestenv=" + (userInfo.bTestEnv ? "true" : "false") +
                    " -adminpath=" + defaultPath;
                //" -LivemMode=Agora/Zego"
                if (!string.IsNullOrEmpty(args))
                {
                    gameArguments += args;
                }
                if (gameProcess == null)
                {
                    gameProcess = GetProcess(liveProgram.ProcessName);
                    if (gameProcess != null)
                    {
                        LiveAppHub.SendMessage($"查找到已有的进程{liveProgram.ProcessName},请确保是当前课程,如果不是,请关闭后让程序自动打开.", OeipLogLevel.OEIP_INFO, 5000);
                    }
                }
                if (gameProcess == null || forceOpen)
                {
                    var gameStartInfo = new ProcessStartInfo
                    {
                        FileName = liveProgram.ProcessPath,
                        Arguments = gameArguments,
                        WorkingDirectory = Path.GetFullPath(liveProgram.ProcessPath)
                    };
                    gameProcess = new Process
                    {
                        StartInfo = gameStartInfo,
                        EnableRaisingEvents = true
                    };
                    LogHelper.LogMessage($"正在打开课件:{ liveProgram.Info?.app_name} {gameArguments}");
                    LiveAppHub.SendMessage($"正在打开课件:{liveProgram.Info?.app_name}", OeipLogLevel.OEIP_INFO, 1000);
                    gameProcess.Start();
                    forceOpen = false;
                    //int i = 0;
                    //while (i++ < 30)
                    //{
                    //    if (gameProcess.MainWindowHandle != IntPtr.Zero)
                    //    {
                    //        LogHelper.LogMessage($"课件:{liveProgram.Info?.app_name} 找到主窗口句柄", OeipLogLevel.OEIP_INFO);
                    //        break;
                    //    }
                    //    Thread.Sleep(1000);
                    //}                    
                    //this.runProgram = liveProgram;
                }

                liveProcessInfo.appName = liveProgram.Info?.app_name;
                liveProcessInfo.processName = liveProgram.ProcessName;
                liveProcessInfo.bOpen = (gameProcess != null && !gameProcess.HasExited);
                LiveAppHub.Hub.Clients.All.UpdateProcess(liveProcessInfo);
                gameProcess.Exited += (senderx, exArgs) =>
                {
                    lock (obj)
                    {
                        try
                        {
                            if (gameProcess != null)
                            {
                                gameProcess.Dispose();
                                gameProcess = null;
                            }
                            liveProcessInfo.appName = liveProgram.Info?.app_name;
                            liveProcessInfo.processName = liveProgram.ProcessName;
                            liveProcessInfo.bOpen = false;
                            LiveAppHub.Hub.Clients.All.UpdateProcess(liveProcessInfo);
                            LiveAppHub.SendMessage($"课件:{liveProgram.Info?.app_name} 关闭", OeipLogLevel.OEIP_INFO, 3000);
                            LogUtility.Log($"课件:{liveProgram.Info?.app_name} 关闭", NewLoginfo.LogType.Trace);
                            KillProcess("iVRealEdc");
                         /*   if (Start_Times != 0)
                            {
                                UploadInfoStr();
                            }*/
                            Vpo po = new Vpo();
                            try
                            {
                                LogUtility.Log("课程关闭上传硬件数据:CPU温度-------" + Convert.ToString(po.OpenGetTemptrue()), LogType.Trace);
                                LogUtility.Log("课程关闭上传硬件数据:CPU负荷-------" + Convert.ToString(po.GetCpu()), LogType.Trace);
                                LogUtility.Log("课程关闭上传硬件数据:类型AMD----GPU负荷-------" + Convert.ToString(po.GetAGPULOAD()), LogType.Trace);
                                LogUtility.Log("课程关闭上传硬件数据:类型AMD----GPU温度-------" + Convert.ToString(po.GetAGPU()), LogType.Trace);
                                LogUtility.Log("课程关闭上传硬件数据:类型NVIDIA----GPU负荷-------" + Convert.ToString(po.GetLoad_NGPU()), LogType.Trace);
                                LogUtility.Log("课程关闭上传硬件数据:类型NVIDIA----GPU温度-------" + Convert.ToString(po.GetGPU()), LogType.Trace);
                                LogUtility.Log("课程关闭上传硬件数据:硬盘温度-------" + Convert.ToString(po.GetHDD()), LogType.Trace);
                                LogUtility.Log("课程关闭上传硬件数据:硬盘负荷-------" + Convert.ToString(po.GetloadHdd_()), LogType.Trace);
                                LogUtility.Log("课程关闭上传硬件数据:内存负荷-------" + Convert.ToString(po.NewgetRam()), LogType.Trace);
                                LogUtility.Log("系统版本：" + _operatingSystem + _osArchitecture, LogType.Trace);
                                LogUtility.Log(PC_GetDxVersion, LogType.Trace);
                            }
                            catch (Exception ex)
                            {
                                LogUtility.Log("课程关闭时硬件日志保存失败---失败原因:--" + ex, LogType.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            LiveAppHub.SendMessage($"课件:{liveProgram.Info?.app_name} 正在关闭出错,请手动关闭.", OeipLogLevel.OEIP_INFO, 3000);
                            LogHelper.LogMessageEx($"课件:{liveProgram.Info?.app_name} 正在关闭出错", ex);
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                LiveAppHub.SendMessage($"课件:{liveProgram.Info?.app_name} 进程打开出错,请手动打开.", OeipLogLevel.OEIP_INFO, 3000);
                LogHelper.LogMessageEx($"课件:{liveProgram.Info?.app_name} 进程打开出错", ex);
                LogUtility.Log($"课件:{liveProgram.Info?.app_name} 进程打开出错" + ex, LogType.Error);
            }
        }
        /// <summary>
        /// 去除前端传回重复数据集合    临时存储
        /// </summary>
        private List<string> Repetition;
        ///关闭时就将上传一次日志
        ///
        public void UploadInfoStr()
        {
            try
            {
                lock (obj)
                {
                    if (bInit == true)
                    {
                        string time = Convert.ToDateTime(DateTime.Now).ToString("yyyy:MM:dd: hh:mm");//12小时制 小时
                        string Get1 = time.Split(':')[3];
                        int NowHour = Convert.ToInt32(Get1);
                        string time_h = Convert.ToDateTime(DateTime.Now).ToString("yyyy:MM:dd: hh:mm");//12小时制 分钟
                        string Get1_h = time_h.Split(':')[4];
                        int NowMinute = Convert.ToInt32(Get1_h);
                        var SumTime_ = Convert.ToString(NowHour + "" + NowMinute);
                        var SumTime = Convert.ToInt32(SumTime_);
                        if (bInit == true)
                        {
                            if (SumTime >= Start_Times || SumTime < End_Times)
                            {
                                string uid1 = string.Empty;
                                string appId1 = string.Empty;
                                string startTimeStamp1 = string.Empty;
                                string endTimeStamp1 = string.Empty;
                                string dspId1 = string.Empty;
                                Repetition = twotest.Distinct().ToList();
                                if (Repetition.Count() > 0)
                                {
                                    for (var i = 0; i < Repetition.Count(); i++)
                                    {
                                        uid1 = Repetition[0];
                                        appId1 = Repetition[1];
                                        startTimeStamp1 = Repetition[2];
                                        endTimeStamp1 = Repetition[3];
                                        dspId1 = Repetition[4];
                                    }
                                    LogError.Log("uid1-" + uid1 + "-appId1-" + appId1 + "-startTimeStamp1-" + startTimeStamp1 + "-endTimeStamp1-" + endTimeStamp1 + "-dspId1-" + dspId1, UpLoginfo_.LogType_E.Trace);
                                    string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                                    string selflogs1 = Path.Combine(currentDirs, "ErrorLog/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                                    ReadInfoFromFile(selflogs1);
                                    DeleteBlank();
                                    Repetition.ToList().Clear();
                                }
                                else
                                {
                                    LiveAppHub.SendMessage("没有获取到当前课件信息", OeipLogLevel.OEIP_INFO, 3000);
                                    KillProcess("iVRealEdc");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage("获取LessonInfo值失败原因:" + ex);
            }
        }
        /// <summary>
        /// 执行 后续上传日志方法
        /// </summary>
        public void Delete_qy()
        {
            lock (obj)
            {
                try
                {
                    string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string selflogs1 = Path.Combine(currentDirs, "ErrorLog/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                    if (File.Exists(selflogs1))
                    {
                        var lines = File.ReadAllLines(selflogs1);
                        var query = lines.ToArray();
                        var str = "";
                        List<string> str_ = new List<string>();
                        if (query.Count() > 0)
                        {
                            for (var i = 0; i < query.Count(); i++)
                            {
                                str = query[i];
                                str_.Add(str);
                            }
                            string str_s4 = "";
                            string str_s3 = "";
                            string str_s2 = "";
                            string str_s1 = "";
                            string str_s = "";
                            str_.Distinct().ToList();
                            for (var s = 0; s < str_.Count(); s++)
                            {
                                str_s = str_[0].Split('-')[1];
                                str_s1 = str_[0].Split('-')[3];
                                str_s2 = str_[0].Split('-')[5];
                                str_s3 = str_[0].Split('-')[7];
                                str_s4 = str_[0].Split('-')[9];
                            }
                            LessonInfo lesson = new LessonInfo();
                            lesson.uid = str_s;
                            lesson.appId = str_s1;
                            lesson.startTimeStamp = str_s2;
                            lesson.endTimeStamp = str_s3;
                            lesson.dspId = str_s4;
                            if (lesson != null)
                            {
                                UploadDirectory(lesson);
                                //去除所有行内容
                                //  File.WriteAllLines(selflogs1, query.Take(0));
                                //只去除第一行内容
                                Delete_Row();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessage("中途崩溃程序日志后续上传失败原因:" + ex);
                    LogUtility.Log("中途崩溃程序日志后续上传失败原因:" + ex, LogType.Error);
                }
            }
        }
        /// <summary>
        /// 每次读取去除第一行
        /// </summary>
        private void Delete_Row()
        {
            string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string selflogs1 = Path.Combine(currentDirs, "ErrorLog/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
            StringBuilder Buider_ = new StringBuilder();
            FileStream FileStr = new FileStream(selflogs1, FileMode.Open, FileAccess.Read);
            StreamReader StrRed = new StreamReader(FileStr);
            string GetStrRed = StrRed.ReadLine();
            int Xall = 0;
            while (GetStrRed != null)
            {
                Xall++;
                if (Xall > 1) //只添加第1行以后的数据
                {
                    Buider_.AppendLine(GetStrRed);
                }
                GetStrRed = StrRed.ReadLine();
            }
            StrRed.Close();
            FileStr.Close();
            FileStream fsWrite = new FileStream(selflogs1, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter StrWrit = new StreamWriter(fsWrite);
            StrWrit.Write(Buider_.ToString());
            StrWrit.Close();
            fsWrite.Close();
        }

        /// <summary>
        /// 删除日志多余空白行方法
        /// </summary>
        private void DeleteBlank()
        {
            try
            {
                string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string selflogs1 = Path.Combine(currentDirs, "ErrorLog/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                var lines = File.ReadAllLines(selflogs1).Where(arg => !string.IsNullOrWhiteSpace(arg));
                File.WriteAllLines(selflogs1, lines);
            }
            catch (Exception ex)
            {
                LogUtility.Log("删除错误日志空白行失败:" + ex, LogType.Error);
            }
        }
        /// <summary>
        /// 去除重复错误信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        private static void ReadInfoFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    List<string> list = new List<string>();
                    using (StreamReader ErrorOne = new StreamReader(filePath, Encoding.GetEncoding("UTF-8")))
                    {
                        while (!ErrorOne.EndOfStream) //读到结尾退出
                        {
                            string temp = ErrorOne.ReadLine();
                            if (!list.Contains(temp)) //去除重复的行
                            {
                                list.Add(temp);
                            }
                        }
                    }
                    //写回去 Append = false 覆盖原来的
                    using (StreamWriter Cover = new StreamWriter(filePath, false, Encoding.GetEncoding("UTF-8")))
                    {
                        foreach (string line in list)
                        {
                            Cover.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage("错误信息去重失败:" + ex);
            }
        }
        private string Getinfostr_ = string.Empty;
        /// <summary>
        /// 临时保存前端传回的上传信息
        /// </summary>
        public List<string> twotest = new List<string>();
        //获取前端开课信息   
        public void GetinfoStr(string GetInfo_Tes)
        {
            Getinfostr_ = GetInfo_Tes;
        }



        //临时存储信息
        public string uid_ = string.Empty;
        public string appId_ = string.Empty;
        public string startTimeStamp_ = string.Empty;
        public string endTimeStamp_ = string.Empty;
        public string dspId_ = string.Empty;


        /// <summary>
        /// 获取前端开课信息保存
        /// </summary>
        /// <param name="uid_t"></param>
        /// <param name="appId_t"></param>
        /// <param name="startTimeStamp_t"></param>
        /// <param name="endTimeStamp_t"></param>
        /// <param name="dspId_t"></param>
        public void TestDX(string uid_t, string appId_t, string startTimeStamp_t, string endTimeStamp_t, string dspId_t)
        {
            if (bInit)
            {
                string uid_ts = uid_t;
                string appid_ts = appId_t;
                string startTime_ts = startTimeStamp_t;
                string endTime_ts = endTimeStamp_t;
                string dspid_ts = dspId_t;
                twotest.Add(uid_ts.ToString());
                twotest.Add(appid_ts.ToString());
                twotest.Add(startTime_ts.ToString());
                twotest.Add(endTime_ts.ToString());
                twotest.Add(dspid_ts.ToString());
            }
        }
        /// <summary>
        /// 获取前端实时返回的开课分钟数
        /// </summary>
        private int Start_Get_UpMinute;
        /// <summary>
        /// 获取前端实时返回的开课小时数
        /// </summary>
        private int Start_Get_Houre;
        /// <summary>
        /// 获取前端实时返回的开课结束时间
        /// </summary>
        private int End_Times;
        /// <summary>
        /// 获取前端实时返回的开课开始时间
        /// </summary>
        private int Start_Times;
        /// <summary>
        /// 实时接收前端返回的开课时间信息
        /// </summary>
        /// <param name="Minute_"></param>
        /// <param name="Houre_"></param>
        /// <param name="EndTime_"></param>
        public void Up_GetMinute(int Minute_, int Houre_, int EndTime_, int StartTime_)
        {
            lock (obj)
            {
                if (bInit == true)
                {
                    Start_Get_UpMinute = Minute_;
                    Start_Get_Houre = Houre_;
                    End_Times = EndTime_;
                    Start_Times = StartTime_;
                }
            }
        }
        /// <summary>
        /// 客户端链接后台需要同步的信息
        /// </summary>
        public void SendProcess()
        {
            if (liveProcessInfo.bOpen)
            {
                LiveAppHub.Hub.Clients.All.UpdateProcess(liveProcessInfo);
                //onPushQuality
            }
            if (AoceManager.Instance.BPush == true)
            {
                LiveAppHub.Hub.Clients.All.StartLive(lessonId, 1);
                LogHelper.LogMessage("发送后台开始录制课程ID："+ lessonId+"LiveMode:"+1);
            }
            else
            {
                LiveAppHub.Hub.Clients.All.StopLive(lessonId, 1);
                LogHelper.LogMessage("发送后台结束录制课程ID：" + lessonId + "LiveMode:" + 1);
            }
            // AudioManager.Instance.SendAudioList();
            LiveAppHub.Hub.Clients.All.SendInnerNet(AppManage.Instance.InnerNet);
        }

        public bool IsOpened
        {
            get
            {
                try
                {
                    if (gameProcess != null && gameProcess.Handle != IntPtr.Zero)
                    {
                        if (!gameProcess.HasExited)
                        {
                            return true;
                        }
                    }
                    liveProcessInfo.bOpen = false;
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessageEx("访问进程出错", ex);
                }
                return false;
            }
        }

        private void CloseProgram()
        {
            //int i = 0;
            //while (i++ < 3)
            {
                if (!IsOpened)
                    return;
                try
                {
                    LogHelper.LogMessage("课件管理自动关闭关闭课件.");
                    if (gameProcess != null)
                    {
                        if (!gameProcess.HasExited)
                        {
                            gameProcess.Kill();
                            Thread.Sleep(1000);
                            gameProcess.WaitForExit(3000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessageEx("课件进程关闭出错", ex);
                    LiveAppHub.SendMessage($"课件进程关闭出错,请手动关闭.", OeipLogLevel.OEIP_INFO);
                }
            }
        }

        public void AutoProgram(bool bOpen, string lessonId, string pid, string liveMode)
        {
            lock (obj)
            {
                if (!bOpen)
                {
                    CloseProgram();
                    return;
                }
                if (!IsOpened)
                {
                    LogHelper.LogMessage($"课件Id:{lessonId} 正在打开程序");
                    LogUtility.Log($"课件Id:{lessonId} 正在打开程序");
                    if (checkPid(pid, out LiveProgram liveProgram))
                    {
                        LogHelper.LogMessage($"课件Id:{lessonId} 程序:{liveProgram.ProgramPath.Path} 正在打开程序");
                        LogUtility.Log($"课件Id:{lessonId} 程序:{liveProgram.ProgramPath.Path} 正在打开程序");
                        if (liveProgram.CheckProcess())
                        {
                            RunProcess(liveProgram, $" -LivemMode={liveMode}");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 用户强制关闭课件
        /// </summary>
        public void ForceCloseProgram()
        {
            try
            {
                LogHelper.LogMessage("用户强制关闭已经存在的课件.");
                this.forceOpen = true;
                if (gameProcess != null)
                {
                    gameProcess.Kill();
                }
                liveProcessInfo.bOpen = false;
                LiveAppHub.Hub.Clients.All.UpdateProcess(liveProcessInfo);
            }
            catch (Exception ex)
            {
                // LiveAppHub.SendMessage($"进程{liveProcessInfo.processName}不可访问,会尝试强制打开课程.", OeipLogLevel.OEIP_WARN, 5000);
                LogHelper.LogMessageEx($"进程{liveProcessInfo.processName}强制关闭出错", ex);
            }
        }

        private string getUE4Version(string version)
        {
            try
            {
                // 得到UE4的版本
                Version ue4Version = new Version(version);
                Version upVersion = new Version(ue4Version.Major, ue4Version.Minor, ue4Version.Build, 0);
                return upVersion.ToString();
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("getUE4Version error:", ex);
            }
            return version;
        }

        public bool UploadGame(string pid, string gamePath, string newVersion)
        {
            try
            {
                if (!Directory.Exists(gamePath))
                {
                    LiveAppHub.SendMessage($"课件路径不存在:{gamePath}.", OeipLogLevel.OEIP_WARN);
                    return false;
                }
                if (checkPid(pid, out LiveProgram liveProgram))
                {
                    //写入游戏版本
                    var gameVersionPath = Path.Combine(gamePath, "GameVersion.txt");
                    File.WriteAllText(gameVersionPath, newVersion);
                    //写入游戏版本 \iVRealEdc\Saved\Config\WindowsNoEditor\Game.ini
                    var exePaths = Directory.GetFiles(gamePath, "*.exe");
                    if (exePaths.Length == 1)
                    {
                        string ue3Version = getUE4Version(newVersion);
                        var fileName = Path.Combine(gamePath, Path.GetFileNameWithoutExtension(exePaths[0]), "Saved/Config/WindowsNoEditor/Game.ini");
                        if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                        }
                        //if (File.Exists(fileName))
                        {//[/Script/EngineSettings.GeneralProjectSettings]        ProjectVersion=1.0.0.55
                            File.WriteAllLines(fileName, new string[] { "[/Script/EngineSettings.GeneralProjectSettings]", "ProjectVersion=" + ue3Version });
                        }
                    }
                    RequestList requestList = new RequestList();
                    requestList.OnDownloadProgressEvent += (ProgressArgs progressArg, ProgressType progressType) =>
                    {
                        LiveAppHub.Hub.Clients.All.DownFile(pid, progressArg, progressType);
                    };
                    requestList.SetDirectory(liveProgram.Info.internal_update_address, gamePath);
                    requestList.LocalDownloadList = "GameManifest.txt";
                    requestList.LocalListCheck = "GameManifest.checksum";
                    requestList.RemoteListParent = "game/Win64/bin";
                    requestList.RemoteDownloadList = "game/Win64/GameManifest.txt";
                    requestList.RemoteListCheck = "game/Win64/GameManifest.checksum";
                    requestList.RemoteVersion = "game/Win64/bin/GameVersion.txt";
                    bool bSucess = requestList.Upload((string path) =>
                      {
                          return ToolHelper.FilterFile(path, false);
                      }).Result;
                    return bSucess;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx($"upload game {gamePath} error:", ex);
                return false;
            }
        }

        private void copyDirectory(string srcDir, string destDir, bool delete = false)
        {
            if (!Directory.Exists(srcDir))
                return;
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    string relativePath = file.Replace(srcDir + "\\", "");
                    string destPath = Path.Combine(destDir, relativePath);
                    if (!Directory.Exists(Path.GetDirectoryName(destPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(file, destPath, true);
                    if (delete)
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessageEx("copy directory error:", ex);
                    continue;
                }
            }
        }

        private void copyFiles(string[] files, string destDir, bool delete = false)
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            foreach (string dumFile in files)
            {
                try
                {
                    File.Copy(dumFile, Path.Combine(destDir, Path.GetFileName(dumFile)), true);
                    if (delete)
                    {
                        File.Delete(dumFile);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessageEx("copy files error:", ex);
                    continue;
                }
            }
        }
        //区别Nlog日志写入   课件后台日志写入
        public void NewUploadinfo(string logstring)
        {
            try
            {
                LogUtility.Log(logstring, NewLoginfo.LogType.Trace);
            }
            catch (Exception ex)
            {
                LogUtility.Log("日志写入错误!" + ex, NewLoginfo.LogType.Error);
            }
        }

        //路径名 E盘 
        public string E_DirName = @"E:\Steam\logs";
        //路径名 D盘
        public string D_DirName = @"D:\Steam\logs";
        //路径名 D盘二
        public string Dprog_DirName = @"D:\Program Files (x86)\Steam\logs";
        //路径名 C盘
        public string C_DirName = @"C:\Program Files (x86)\Steam\logs";
        //路径名 F盘
        public string F_DirName = @"F:\Steam\logs";

        //路径名 G盘   可能会是U盘或者外部硬盘
        public string G_DirName = @"G:\Steam\logs";
        //路径名 H盘   可能会是U盘或者外部硬盘
        public string H_DirName = @"H:\Steam\logs";
        //本地测试路径
        public string Test_DirName = @"E:\SteamVR_loginfo";
        //文件中包含名
        public string FileName = "vr";
        /// <summary>
        /// G盘搜索
        /// </summary>

        /// <summary>
        /// E盘查找
        /// </summary>
        public void E_FilePath_()
        {
            if (Directory.Exists(E_DirName))
            {
                GetFileName(E_DirName, FileName);
            }
        }
        /// <summary>
        /// D盘查找
        /// </summary>
        public void D_FilePath_()
        {
            if (Directory.Exists(D_DirName))
            {
                GetFileName(D_DirName, FileName);
            }
        }
        /// <summary>
        /// D盘搜索\Program Files (x86)文件内
        /// </summary>
        public void Dprog_Filepath()
        {
            if (Directory.Exists(Dprog_DirName))
            {
                GetFileName(Dprog_DirName, FileName);
            }
        }

        /// <summary>
        /// C盘查找
        /// </summary>
        public void C_FilePath_()
        {
            if (Directory.Exists(C_DirName))
            {
                GetFileName(C_DirName, FileName);
            }
        }
        /// <summary>
        /// F盘查找
        /// </summary>
        public void F_FilePath_()
        {
            if (Directory.Exists(F_DirName))
            {
                GetFileName(F_DirName, FileName);
            }
        }
        /// <summary>
        /// 复制查找SteamVR日志复制到SteamVR_log
        /// </summary>
        /// <param name="DirName">地址</param>
        /// <param name="FileName">有关信息</param>
        public void GetFileName(string DirName, string FileName)
        {
            lock (obj)
            {
                if (bInit == true)
                {
                    string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string currentDirsr = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string logDir = Path.Combine(currentDir, "UploadLog");
                    //文件夹信息
                    DirectoryInfo dir = new DirectoryInfo(DirName);
                    //如果非根路径且是系统文件夹则跳过
                    if (null != dir.Parent && dir.Attributes.ToString().IndexOf("System") > -1)
                    {
                        return;
                    }
                    //取得所有文件
                    try
                    {
                        FileInfo[] finfo = dir.GetFiles();
                        string fname = string.Empty;
                        for (int i = 0; i < finfo.Length; i++)
                        {
                            fname = finfo[i].Name;
                            //判断文件是否包含查询名
                            if (fname.IndexOf(FileName) > -1)
                            {
                                // MessageBox.Show(finfo[i].FullName); finfo[i].FullName = "E:\\Steam\\logs\\vrssss.log"
                                string cut_out = finfo[i].FullName.ToString();
                                string cut1 = cut_out.Substring(0, cut_out.IndexOf("logs\\"));
                                string sr = cut1 + "logs";
                                SetFileRole(sr);
                                string selflogs = Path.Combine(currentDir, "SteamVR_log");
                                Delete_unrelatedFileLog(selflogs);
                                string vrlog = Path.Combine(currentDirs, "SteamVR_log");
                                string vrlogs = Path.Combine(logDir, "SteamVRlog");
                                copyDirectory(vrlog, vrlogs);
                                string mixed = Path.Combine(currentDirsr, "Mixed_Reality_Portal");
                                string mixeds = Path.Combine(logDir, "Mixed_Reality_PortalLog");
                                copyDirectory(mixed, mixeds);
                                if (FileSize(selflogs) > 0)
                                {
                                }
                                else
                                {
                                    LiveAppHub.SendMessage("获取SteamVR日志失败，为保证课件程序正常使用请按照提示进行配置，即将为您打开文档", OeipLogLevel.OEIP_WARN, 30000);
                                    LogUtility.Log("课程关闭上传SteamVR_Log文件为空", LogType.Error);
                                }
                                CopyDireToDire(@"" + sr + "", @"" + selflogs + "", @"C:\SteamVRlog_Backups");
                            }
                        }
                        //取得所有子文件夹
                        DirectoryInfo[] dinfo = dir.GetDirectories();
                        for (int i = 0; i < dinfo.Length; i++)
                        {
                            //查找子文件夹中是否有符合要求的文件
                            GetFileName(dinfo[i].FullName, FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtility.Log("自动查找SteamVR日志未找到" + ex, LogType.Error);
                    }
                }
            }
        }
        /// <summary>
        /// 打开SteamVR配置讲解HTML
        /// </summary>
        public void OpenDocx()
        {
            if (bInit == true)
            {
                try
                {
                    string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string selflogs = Path.Combine(currentDirs, "SteamVR_Config.html");
                    if (File.Exists(selflogs))
                    {
                        System.Diagnostics.Process.Start(selflogs);
                    }
                    else
                    {
                        LiveAppHub.SendMessage("没有找到SteamVR_Config.html文件，如有问题请联系FAE工作人员", OeipLogLevel.OEIP_WARN, 5000);
                    }

                }
                catch (Exception ex)
                {
                    LogUtility.Log("打开SteamVR_Config Html失败原因:" + ex, LogType.Error);
                }
            }
        }
        /// <summary>
        /// 可能会遇到的错误
        /// </summary>
        public void OpenErrorHtml()
        {
            if (bInit == true)
            {
                try
                {
                    string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string selflogs = Path.Combine(currentDirs, "OpenError.html");
                    if (File.Exists(selflogs))
                    {
                        System.Diagnostics.Process.Start(selflogs);
                    }
                    else
                    {
                        LiveAppHub.SendMessage("没有找到OpenError.html文件，如有问题请联系FAE工作人员", OeipLogLevel.OEIP_WARN, 5000);
                    }

                }
                catch (Exception ex)
                {
                    LogUtility.Log("打开可能会遇到的问题 Html失败原因:" + ex, LogType.Error);
                }
            }
        }
        /// <summary>
        /// 安装运行库合集教程
        /// </summary>
        public void InstallRunTime_JC()
        {
            if (bInit == true)
            {
                try
                {
                    string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string selflogs = Path.Combine(currentDirs, "RunTime_H.html");
                    if (File.Exists(selflogs))
                    {
                        System.Diagnostics.Process.Start(selflogs);
                    }
                    else
                    {
                        LiveAppHub.SendMessage("没有找到RunTime_H.html文件，如有问题请联系FAE工作人员", OeipLogLevel.OEIP_WARN, 5000);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessageEx("打开运行合集安装教程时错误:", ex);
                }
            }
            else
            {
                LiveAppHub.SendMessage("程序正在初始化，请稍后尝试", OeipLogLevel.OEIP_WARN, 5000);
            }
        }

        //收取一个月内的windows系统错误日志相关
        public void GetMixed()
        {
            if (bInit == true)
            {
                EventLog eventLog1 = new EventLog();
                eventLog1.Log = "Application";
                EventLogEntryCollection collection = eventLog1.Entries;
                int Count = collection.Count;
                string info = "Windows错误日志" + Count.ToString() + "个时间节点错误。";
                string infos = "相关信息";
                foreach (EventLogEntry entry in collection)
                {
                    DateTime Str = DateTime.Now.AddDays(-30);
                    try
                    {
                        if (@"Application Hang" == entry.Source && entry.TimeGenerated > Str)
                        {
                            info += "\n\n类型：" + entry.EntryType.ToString();
                            info += "\n\n日期：" + entry.TimeGenerated;
                            info += "\n\n时间：" + entry.TimeGenerated.ToLongTimeString();
                            info += "\n\n来源：" + entry.Source;
                            info += "\n\n用户：" + entry.UserName;
                            info += "\n\n计算机：" + entry.MachineName;
                            info += "\n\n信息:" + entry.Message;
                        }
                        if (@"Application Error" == entry.Source && entry.TimeGenerated > Str)
                        {
                            infos += "\n\n类型：" + entry.EntryType.ToString();
                            infos += "\n\n日期：" + entry.TimeGenerated;
                            infos += "\n\n时间：" + entry.TimeGenerated.ToLongTimeString();
                            infos += "\n\n来源：" + entry.Source;
                            infos += "\n\n用户：" + entry.UserName;
                            infos += "\n\n计算机：" + entry.MachineName;
                            infos += "\n\n信息:" + entry.Message;
                        }
                        //info+="\n\n事件："+entry.EventID.ToString();
                    }
                    catch (Exception ex)
                    {
                        LogUtility.Log("收集Windows日志错误:" + ex, LogType.Error);
                    }
                }
                MixedLoginfo.Log(info + infos, LogTypes.Trace);
                //写入混合现实
            }
        }
        /// <summary>
        /// 删除多余文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Delete_unrelatedFileLog(string fileName)
        {
            if (bInit == true)
            {
                foreach (string d in Directory.GetFileSystemEntries(fileName))
                {
                    if (File.Exists(d))
                    {
                        string me = Path.GetFileNameWithoutExtension(d);
                        if (!me.StartsWith("v") && !me.StartsWith("b"))
                        {
                            File.Delete(d);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 将一个文件夹下的所有东西复制到另一个文件夹 (可备份文件夹)
        /// </summary>
        /// <param name="sourceDire">源文件夹全名</param>
        /// <param name="destDire">目标文件夹全名</param>
        /// <param name="backupsDire">备份文件夹全名</param>
        public void CopyDireToDire(string sourceDire, string destDire, string backupsDire = null)
        {

            if (Directory.Exists(sourceDire) && Directory.Exists(destDire))
            {
                DirectoryInfo sourceDireInfo = new DirectoryInfo(sourceDire);
                FileInfo[] fileInfos = sourceDireInfo.GetFiles();
                foreach (FileInfo fInfo in fileInfos)
                {
                    string sourceFile = fInfo.FullName;
                    string destFile = sourceFile.Replace(sourceDire, destDire);
                    if (backupsDire != null && File.Exists(destFile))
                    {
                        Directory.CreateDirectory(backupsDire);
                        string backFile = destFile.Replace(destDire, backupsDire);
                        File.Copy(destFile, backFile, true);
                    }
                    File.Copy(sourceFile, destFile, true);
                }
                DirectoryInfo[] direInfos = sourceDireInfo.GetDirectories();
                foreach (DirectoryInfo dInfo in direInfos)
                {
                    string sourceDire2 = dInfo.FullName;
                    string destDire2 = sourceDire2.Replace(sourceDire, destDire);
                    string backupsDire2 = null;
                    if (backupsDire != null)
                    {
                        backupsDire2 = sourceDire2.Replace(sourceDire, backupsDire);
                    }
                    Directory.CreateDirectory(destDire2);
                    CopyDireToDire(sourceDire2, destDire2, backupsDire2);
                }
            }
        }
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static long GetDirectoryLength(string dirPath)
        {
            //判断给定的路径是否存在,如果不存在则退出
            if (!Directory.Exists(dirPath))
                return 0;
            long len = 0;

            //定义一个DirectoryInfo对象
            DirectoryInfo di = new DirectoryInfo(dirPath);
            
            //通过GetFiles方法,获取di目录中的所有文件的大小
            foreach (FileInfo fi in di.GetFiles())
            {
                len += fi.Length;
            }
            //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }
            return len;
        }
        public static long FileSize(string filePath)
        {
            long temp = 0;

            //判断当前路径所指向的是否为文件
            if (File.Exists(filePath) == false)
            {
                string[] str1 = Directory.GetFileSystemEntries(filePath);
                foreach (string s1 in str1)
                {
                    temp += FileSize(s1);
                }
            }
            else
            {
                //定义一个FileInfo对象,使之与filePath所指向的文件向关联,

                //以获取其大小
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return temp;
        }
        /// <summary>
                /// 设置文件夹权限，处理为Everyone所有权限
                /// </summary>
                /// <param name="foldPath">文件夹路径</param>
        public static void SetFileRole(string foldPath)
        {
            DirectorySecurity fsec = new DirectorySecurity();
            fsec.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl,
              InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            System.IO.Directory.SetAccessControl(foldPath, fsec);
        }
        public void AllDelete()
        {
            Vpo po = new Vpo();
            po.AllDeleteTs();
        }
        /// <summary>
        /// 释放日志IO资源
        /// </summary>
        private void DisponFile()
        {
            lock (obj)
            {
                try
                {
                    string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string selfLog = Path.Combine(currentDir, "logs/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                    if (File.Exists(selfLog))
                    {
                        System.Drawing.Image Files = System.Drawing.Image.FromFile(selfLog);
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Files);
                        Files.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.Log("释放Logs文件日志IO资源失败:" + ex);
                }
            }
        }
        /// <summary>
        /// 删除  Zmf  相关文件    
        /// </summary>
        /// <param name="filePathByForeach">查找文件地址</param>
        public void ForeachFile(string filePathByForeach)
        {
            try { 
            DirectoryInfo theFolder = new DirectoryInfo(filePathByForeach);
            FileInfo[] file = theFolder.GetFiles();//获取所在目录的文件
            List<string> FilePath = new List<string>();
            foreach (FileInfo fileItem in file) //遍历文件
            {
                string extension = Path.GetExtension(fileItem.ToString());
                if (fileItem.FullName.Contains("Zmf") || fileItem.FullName.Contains("zmf") && extension.Equals(".dll") || extension.Equals(".pdb"))
                {
                    FilePath.Add(fileItem.ToString());
                }
            }
            if (FilePath.Count() > 0)
            {
                for (var i = 0; i < FilePath.Count(); i++)
                {
                    if (FilePath[i].Contains("Zmf") || FilePath[i].Contains("zmf"))
                    {
                        LogHelper.LogMessage("删除多余Zmf文件 : " + FilePath[i]);
                        LogUtility.Log("删除多余Zmf文件 : " + FilePath[i]);
                        File.Delete(FilePath[i]);
                    }
                }
            }
            }
            catch (Exception ex) {
                LogHelper.LogMessage("Delete Zmf File Error : "+ex);
            }
        }
        /// <summary>
        ///  复制本地日志 并压缩  上传服务器     
        /// </summary>
        /// <param name="lesson">上传信息</param>
        public void UploadDirectory(LessonInfo lesson)
        {
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logDir = Path.Combine(currentDir, "UploadLog");
            bool bDmp = false;
            try
            {
                LiveAppHub.SendMessage("开始上传日志", OeipLogLevel.OEIP_INFO);

                //如果此文件夹已经有了,删除目录所有文件后重新组织对应最新
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                //复制zego文件
                string zegoLog = Path.Combine(currentDir, "ZegoLog");
                if (Directory.Exists(zegoLog))
                {
                    string destZegoLog = Path.Combine(logDir, "ZegoLog");
                    copyDirectory(zegoLog, destZegoLog);
                }
                //复制agora文件
                string agoraLog = Path.Combine(currentDir, "AgoraLog");
                if (Directory.Exists(agoraLog))
                {
                    string destAgoraLog = Path.Combine(logDir, "AgoraLog");
                    copyDirectory(agoraLog, destAgoraLog);
                }
                string tempAgoraLog = Path.Combine(Path.GetTempPath(), "../Agora/WinLiveManage");
                string destTempAgoraLog = Path.Combine(logDir, "AgoraUserLog");
                copyDirectory(tempAgoraLog, destTempAgoraLog);
                //复制DMP文件
                var dmpFiles = Directory.GetFiles(Application.StartupPath, "*.dmp", SearchOption.TopDirectoryOnly);
                if (dmpFiles.Length > 0)
                {
                    string destDmpDir = Path.Combine(logDir, "DMP");
                    copyFiles(dmpFiles, destDmpDir);
                }
                //复制UE4日志

                if (checkPid(lesson.appId, out LiveProgram runProgram))
                {
                    var gamePath = Path.GetDirectoryName(runProgram.ProcessPath);
                    if (Directory.Exists(gamePath))
                    {
                        string ueLogsDict = Path.Combine(gamePath, "../../Saved/Logs");
                        string ueDestDict = Path.Combine(logDir, "UE4Logs");
                        LogHelper.LogMessage($"开始复制UE4日志{ueLogsDict}");
                        copyDirectory(ueLogsDict, ueDestDict, true);
                        //UE4下面的zmf dmp文件 
                        var ueDmpFiles = Directory.GetFiles(gamePath, "*.dmp", SearchOption.AllDirectories);
                        if (ueDmpFiles.Length > 0)
                        {
                            LogHelper.LogMessage($"查找到UE4下有ZMF崩溃日志{gamePath}");
                            string destUE4DmpDir = Path.Combine(ueDestDict, "zmfdmp");
                            copyFiles(ueDmpFiles, destUE4DmpDir, true);
                            bDmp = true;
                        }
                        //UE4下面的dmp文件
                        string ueDumpDict = Path.Combine(gamePath, "../../Saved/Crashes");
                        if (Directory.Exists(ueDumpDict))
                        {
                            ueDmpFiles = Directory.GetFiles(ueDumpDict, "*.dmp", SearchOption.AllDirectories);
                            if (ueDmpFiles.Length > 0)
                            {
                                LogHelper.LogMessage($"查找到UE4下有崩溃日志{ueDumpDict}");
                                string destUE4DmpDir = Path.Combine(ueDestDict, "ue4dmp");
                                copyFiles(ueDmpFiles, destUE4DmpDir, true);
                                bDmp = true;
                            }
                        }
                        //复制UE4下agora日志                        
                        string ue4agoraLog = Path.Combine(gamePath, "../../Plugins/iVRealKit/ThirdParty/ZmfUE4Dll/bin/AgoraLog");
                        string ue4destAgoraLog = Path.Combine(ueDestDict, "AgoraLog");
                        copyDirectory(ue4agoraLog, ue4destAgoraLog);
                        string ue4tempAgoraLog = Path.Combine(Path.GetTempPath(), "../Agora", runProgram.ProcessName);
                        string ue4destTempAgoraLog = Path.Combine(ueDestDict, "AgoraUserLog");
                        copyDirectory(ue4tempAgoraLog, ue4destTempAgoraLog);
                    }
                }
                //复制后台自身日志文档currentDirst

                string selflogs = Path.Combine(currentDir, "NewLog/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                FileIOZy(selflogs);
                string selflogsr = Path.Combine(currentDir, "SteamVR_log");
                File.Copy(selflogs, Path.Combine(logDir, "Hardware" + DateTime.Now.ToString("yyyy-MM-dd") + ".log"), true);
                string uss = SettingManager.Instance.UserInfo.role;
                if (uss != "teacher")
                {
                    try
                    {  //执行获取 Windows日志
                        GetMixed();

                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogMessage("获取Windows日志失败：" + ex);
                    }
                    try
                    {
                        D_FilePath_();
                        C_FilePath_();
                        F_FilePath_();
                        E_FilePath_();
                        Dprog_Filepath();
                    }
                    catch (Exception ex)
                    {
                        LogUtility.Log("全盘搜索SteamVR日志错误" + ex, LogType.Error);
                    }
                    if (FileSize(selflogsr) <= 0)
                    {
                        OpenDocx();
                    }
                }
                else
                {
                    LogUtility.Log("老师端正常跳过接收SteamVR，混合现实门户日志", LogType.Trace);
                }
                DisponFile();
                string selfLog = Path.Combine(currentDir, "logs/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                FileIOZy(selfLog);
                File.Copy(selfLog, Path.Combine(logDir, DateTime.Now.ToString("yyyy-MM-dd") + ".log"), true);
            }
            catch (Exception ex)
            {
                Lsinfo.Log("上传本身日志时出现错误:" + ex);
                string selflogs = Path.Combine(currentDir, "logs/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                if (File.Exists(selflogs))
                {
                    string Lsinfos = Path.Combine(currentDir, "Lsinfo");
                    if (Directory.Exists(Lsinfos))
                    {
                        File.Copy(selflogs, Path.Combine(Lsinfos, DateTime.Now.ToString("yyyy-MM-dd") + ".log"), true);
                        string setInfos = Path.Combine(currentDir, "Lsinfo/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                        File.Copy(setInfos, Path.Combine(logDir, DateTime.Now.ToString("yyyy-MM-dd") + ".log"), true);
                    }
                }
                LogUtility.Log("upload log error:" + ex, LogType.Error);
            }
            finally
            {
                string logDirZip = Path.Combine(currentDir, "UploadLog.zip");
                if (File.Exists(logDirZip))
                {
                    File.Delete(logDirZip);
                }
                LogHelper.LogMessage($"开始压缩文件夹{logDir}");
                ZipFile.CreateFromDirectory(logDir, logDirZip, CompressionLevel.Fastest, true);
                LogHelper.LogMessage($"开始上传压缩文件{logDirZip}");
                //通过http上传日志到服务器
                if (UploadLogFile(logDirZip, lesson, bDmp))
                {
                    LiveAppHub.SendMessage("上传日志成功", OeipLogLevel.OEIP_INFO);
                    LogHelper.LogMessage($"开始删除文件夹{logDir}");
                    Directory.Delete(logDir, true);
                    string AgoraLogs = Path.Combine(currentDir, "AgoraLog");//ZegoLog
                    string ZegoLogs = Path.Combine(currentDir, "ZegoLog");
                    string SteamVRlogs = Path.Combine(currentDir, "SteamVR_log");
                    string Mixedlogs = Path.Combine(currentDir, "Mixed_Reality_Portal");
                    string Logsinfo = Path.Combine(currentDir, "Lsinfo/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                    try
                    {
                        if (Directory.Exists(AgoraLogs))
                        {
                            Directory.Delete(AgoraLogs, true);
                        }
                    }
                    catch { }
                    try
                    {
                        if (Directory.Exists(ZegoLogs))
                        {
                            Directory.Delete(ZegoLogs, true);
                        }
                    }
                    catch { }
                   /* try
                    {
                        if (Directory.Exists(Mixedlogs))
                        {
                            Directory.Delete(Mixedlogs, true);
                        }
                    }
                    catch { }
                    try
                    {
                        if (Directory.Exists(SteamVRlogs))
                        {
                            Directory.Delete(SteamVRlogs, true);
                        }
                    }
                    catch { }*/
                    try
                    {
                        if (File.Exists(Logsinfo))
                        {
                            File.Delete(Logsinfo);
                        }
                    }
                    catch { }
                    try
                    {
                        if (File.Exists(logDirZip))
                        {
                            File.Delete(logDirZip);
                        }
                    }
                    catch { }
                }
                else
                {
                    LiveAppHub.SendMessage("上传日志失败", OeipLogLevel.OEIP_WARN);
                }
            }
        }
        private void FileIOZy(string Path) {
            var logFilePath = Path;
            var now = DateTime.Now;
            var logContent = string.Format("Tid: {0}{1} {2}.{3}\r\n", Thread.CurrentThread.ManagedThreadId.ToString().PadRight(4), now.ToLongDateString(), now.ToLongTimeString(), now.Millisecond.ToString());

            var logContentBytes = Encoding.Default.GetBytes(logContent);
            //由于设置了文件共享模式为允许随后写入，所以即使多个线程同时写入文件，也会等待之前的线程写入结束之后再执行，而不会出现错误
            using (FileStream logFile = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                logFile.Seek(0, SeekOrigin.End);
                logFile.Write(logContentBytes, 0, logContentBytes.Length);
            }
        }
        //上传日志
        public bool UploadLogFile(string path, LessonInfo lesson, bool bdmp = false)
        {
            bool bUpload = false;
            string baseAddr = SettingManager.Instance.UserInfo.bTestEnv ? "testapi.talkdoo.com:9010" : "api.talkdoo.com:9020";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string boundary = "Upload log----" + DateTime.Now.ToString(CultureInfo.InvariantCulture);
                    using (var content = new MultipartFormDataContent(boundary))
                    {
                        var filestream = new FileStream(path, FileMode.Open);
                        content.Add(new StreamContent(filestream), "log_file", path);
                        var tempPath = path;
                        if (bdmp)
                        {
                            tempPath += "_dmp";
                        }
                        //content.Add(null, "statck_file");
                        bool bTeacher = SettingManager.Instance.UserInfo.IsTeacher();
                        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(bTeacher ? "2" : "1"))), "log_type");
                        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(lesson.uid))), "device_ids");
                        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(lesson.dspId))), "dspids");//"3494822609b44cb6a95a2019f5789e18" oldId
                        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(lesson.startTimeStamp))), "start_time");
                        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(lesson.endTimeStamp))), "end_time");
                        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("0"))), "error_count");
                        content.Add(new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(tempPath))), "app_path");
                        string uri = "http://" + baseAddr + "/1.0/publicapi/runtimelog/add/";
                        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        string timestamp = Convert.ToInt64(ts.TotalMilliseconds).ToString();
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT heade
                        client.DefaultRequestHeaders.Add("uid", lesson.uid);
                        client.DefaultRequestHeaders.Add("timestamp", timestamp);
                        client.DefaultRequestHeaders.Add("signature", lesson.uid);
                        var response = client.PostAsync(uri, content);
                        LogUtility.Log("当前传入UID："+ lesson.uid);
                        var ret = response.Result;
                        if (ret.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string result = ret.Content.ReadAsStringAsync().Result;
                            if (ret.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                CommonInfo t = JsonConvert.DeserializeObject<CommonInfo>(result);
                                if (t == null || t.code != 200)
                                {
                                    LogHelper.LogMessage($"UploadLogFile fail:{result}");
                                    return false;
                                }
                            }
                        }
                        ret.Dispose();
                        bUpload = true;
                    }
                }
            }
            catch (Exception exc)
            {
                LogHelper.LogMessageEx("UploadLogFile error", exc);
                bUpload = false;
            }
            return bUpload;
        }

    }

}

