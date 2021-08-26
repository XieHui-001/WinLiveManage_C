using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using OeipCommon;
using OeipCommon.FileTransfer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ILiveObserver;
using static WinLiveManage.NewLoginfo;
using static WinLiveManage.WinLiveManager;

namespace WinLiveManage
{
    [HubName("liveapphub")]
    public class LiveAppHub : Hub
    {
        private static List<string> connectList = new List<string>();
        public static IHubContext Hub
        {
            get
            {
                return GlobalHost.ConnectionManager.GetHubContext<LiveAppHub>();
            }
        }

        public static void SendMessage(string message, OeipLogLevel logLevel, int timems = 5000)
        {
            LogHelper.LogMessage(message, logLevel);
            Hub.Clients.All.SendMessage(message, logLevel, timems);
        }

        public static void SendBackMessage(int code, string msg, OeipLogLevel level = OeipLogLevel.OEIP_INFO)
        {
            LogHelper.LogMessage($"code:{code} msg:{msg}", level);
            Hub.Clients.All.SendBackMessage(code, msg, level);
        }

        //初始连接后给于相应状态,在链接时可能会多次调用
        public async Task GetSetting(User user)
        {
            await Task.Run(() =>
            {
                if (user == null)
                {
                    LogHelper.LogMessage("有空的用户信息登陆,请排查.", OeipLogLevel.OEIP_ERROR);
                    return;
                }
                if (SettingManager.Instance.UserInfo != null)
                {
                    //已经有人登陆的情况下
                    if (connectList.Count > 1)
                    {
                        
                        Clients.Caller?.SendUserCount(connectList.Count);
                        return;
                    }
                    if (SettingManager.Instance.UserInfo.bTestEnv != user.bTestEnv)
                    {
                        string evnStr = SettingManager.Instance.UserInfo.bTestEnv ? "测试环境" : "正式环境";
                        LogHelper.LogMessage("用户改变" + evnStr + ",准备重启");
                        string errStr = $"请注意,后台还是{evnStr},程序2秒后自动重启.";
                        Clients.Caller?.SendEnvError(errStr);
                        Thread.Sleep(500);
                        AppManage.Instance.CloseApp(true);
                        return;
                    }
                }
                //没人登录或是重登录情况
                SettingManager.Instance.UserInfo = user;
                LogHelper.LogMessage("用户登陆:" + JsonConvert.SerializeObject(user));
                LogUtility.Log("用户登陆" + JsonConvert.SerializeObject(user), LogType.Trace);

                 var setting = SettingManager.Instance.Setting;
                LogHelper.LogMessage("用户配置:" + JsonConvert.SerializeObject(setting));
                LogUtility.Log("用户配置:" + JsonConvert.SerializeObject(setting), LogType.Trace);
                //var settingStr = JsonConvert.SerializeObject(setting);
                Clients.Caller?.SendSetting(setting);
                //版本信息
                string versionPath = Path.Combine(SettingManager.Instance.Setting.launchpadSetting.GetUpdateAddress(), "launcher/LauncherVersion.txt");
                RequestItem requestItem = RequestItem.GetRequestItem(versionPath);
                requestItem.SetPath(versionPath, string.Empty);
                string remoteVersion = requestItem.ReadRemote();
                LogHelper.LogMessage("服务器版本:" + remoteVersion);
                LogUtility.Log("服务器版本:" + remoteVersion,LogType.Trace);
                string localVersion = GetType().Assembly.GetName().Version.ToString();
                HostVersion hostVersion = new HostVersion();
                hostVersion.localVersion = localVersion;
                hostVersion.remoteVersion = remoteVersion;
                LiveAppHub.Hub.Clients.All.UpdateHost(hostVersion);
                WinLiveManager.Instance.SendProcess();
                //电脑硬件信息
                WinSystemInfo.Instance.GetHardware(true);
            });
        }
        private object obj = new object();
        //获取所有课件
        public async Task<AppProgram[]> GetProgramList(ProgramInfo[] programInfos)
        {
            return await Task.Run(async () =>
            {
                ConcurrentBag<AppProgram> appPrograms = new ConcurrentBag<AppProgram>();
                try {
                LogHelper.LogMessage("开始请求填充课件列表本地信息.");
                LogUtility.Log("开始请求填充可见列表本地信息");
                if (connectList.Count < 1)
                { 
                return null;
                }
                if (SettingManager.Instance.UserInfo == null) {
                    return null;
                }
                var rootPathSetting = SettingManager.Instance.GetPathSetting();
                string root = Path.GetPathRoot(rootPathSetting.GamePath);
                if (!Directory.Exists(root))
                {
                    string rootPath = Path.GetPathRoot(Assembly.GetExecutingAssembly().Location);
                    string dirPath = "IVREAL_MR";
                    if (SettingManager.Instance.UserInfo.bTestEnv)
                    {
                        dirPath += "_Test";
                    }
                    rootPathSetting.GamePath = Path.Combine(rootPath, dirPath);
                    rootPathSetting.SettingPath = Path.Combine(rootPath, dirPath, "Setting");
                    SettingManager.Instance.SaveSetting();
                }
                var livePrograms = new List<LiveProgram>();
                foreach (var pInfo in programInfos)
                {
                    LiveProgram liveProgram = await GetLiveProgram(pInfo);
                    if (liveProgram == null)
                        continue;
                    livePrograms.Add(liveProgram);
                }
              
                //List <AppProgram> appPrograms = new List<AppProgram>();
                foreach (var pLive in livePrograms)
                //Parallel.ForEach(WinLiveManager.Instance.LivePrograms, (pLive) =>
                {
                    pLive.RemoteVersion = pLive.Request.GetRemoteVersion();
                    try
                    {
                        string fullPath = Path.Combine(pLive.ProgramPath.Path, "Win64/GameVersion.txt");
                        if (File.Exists(fullPath))
                        {
                            var localGameVersion = File.ReadAllText(fullPath);
                            if (Version.TryParse(localGameVersion, out Version gameVersion))
                            {
                                pLive.LocalVersion = gameVersion;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogMessage("GetProgramList is error :"+ex);
                        LogUtility.Log("GetProgramList is error" + ex, LogType.Error);
                    }
                    AppProgram appProgram = new AppProgram();
                    appProgram.id = pLive.Info.app_id;
                    appProgram.remoteVersion = pLive.RemoteVersion?.ToString();
                    appProgram.localVersion = pLive.LocalVersion?.ToString();
                    appProgram.launcherMode = pLive.LauncherMode;
                    appPrograms.Add(appProgram);
                };//);
                WinLiveManager.Instance.SetLivePorgrams(livePrograms);
                LogHelper.LogMessage("课件列表本地信息填充完成.");
                LogUtility.Log("课件列表本地信息填充完成");
                }
                catch (Exception ex) {
                    WinLiveManager.Instance.strinfo.Add("课表填充出错::::" + ex);
                    LiveAppHub.SendMessage("请求课件列表时发生错误", OeipLogLevel.OEIP_WARN, 5000);
                    LogHelper.LogMessage("请求课件列表时错误:" + ex);
                }
                return appPrograms.ToArray();
                //Clients.Caller?.SendProgramList(appPrograms.ToArray());
            });
            
        }

        public async Task<LiveProgram> GetLiveProgram(ProgramInfo programInfo)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var launchpad = SettingManager.Instance.Setting.launchpadSetting;
                    var uri = programInfo.GetUpdateAddress();
                    // uri可能是非正常地址
                    if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri rUri))
                    {
                        LogHelper.LogMessage($"课件:{programInfo.app_name} 下载地址:{uri} not uri.");
                        LogUtility.Log($"课件:{programInfo.app_name} 下载地址:{uri} not uri.",LogType.Trace);
                        return null;
                    }
                    var rootPathSetting = SettingManager.Instance.GetPathSetting();
                    string uriPath = rUri.LocalPath;
                    var paths = uriPath.TrimEnd('/').Split('/');
                   
                    if (paths.Length > 0)
                    {
                        uriPath = paths[paths.Length - 1];
                    }
                    string programName = uriPath.Replace("/", "");

                    ProgramSetting programPath = new ProgramSetting();
                    programPath.Id = programInfo.app_id;
                    programPath.Path = Path.Combine(rootPathSetting.GamePath, programName, "Game");
                    RequestList requestList = new RequestList();
                    requestList.SetDirectory(uri, Path.Combine(programPath.Path, "Win64"));
                    requestList.LocalDownloadList = "GameManifest.txt";
                    requestList.LocalListCheck = "GameManifest.checksum";
                    requestList.RemoteListParent = "game/Win64/bin";
                    requestList.RemoteDownloadList = "game/Win64/GameManifest.txt";
                    requestList.RemoteListCheck = "game/Win64/GameManifest.checksum";
                    requestList.RemoteVersion = "game/Win64/bin/GameVersion.txt";
                    requestList.EXEpathStr = "game/Win64/iVRealEdc.exe";
                    LiveProgram liveProgram = new LiveProgram();
                    liveProgram.Info = programInfo;
                    liveProgram.ProgramPath = programPath;
                    liveProgram.Request = requestList;
                    return liveProgram;
                }
                catch (Exception ex)
                {
                    LogHelper.LogMessageEx("GetLiveProgram is error", ex, OeipLogLevel.OEIP_WARN);
                    LogUtility.Log("GetLiveProgram is error"+ex, LogType.Error);
                    return null;
                }
            });
        }

        /// <summary>
        /// 花费太长时间，阻塞了别的消息，所以使用异步方式
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="launcherMode"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task RunProgram(string pid, LauncherMode launcherMode, string args)
        {
            await Task.Run(() => WinLiveManager.Instance.RunProgram(pid, launcherMode, args));

        }
        //打开目录课件
        public async Task OpenProgramPath(string pid)
        {
            await Task.Run(() => { WinLiveManager.Instance.OpenProgramPath(pid);});
        }
        public async Task DleteFile_Sur(string pid) {
            await Task.Run(() => { WinLiveManager.Instance.DleteFile_Sur(pid); });
        }
        public async Task<DeleteHin> Delete_hint()
        {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.Delete_hint();
            });
        }
        //验证文件完整性
        public async Task VerifyProgram(string pid)
        {
            await Task.Run(() => WinLiveManager.Instance.VerifyProgram(pid));
        }

        //传入当前时间
        public async Task<Gettime> Newtime()
        {
           return await Task.Run(() =>
            {
               return WinLiveManager.Instance.Newtime();
            });
        }
        //测试手动上传日志
        public async Task Delete_qy() {
            await Task.Run(() =>
            {
                 WinLiveManager.Instance.Delete_qy();
            });
        }
        //传入当前时间
        public async Task<Gettime_Minute> Newtime_Minute() {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.Newtime_Minute();
            });
        }
        /// <summary>
        /// 当前静止时间    休眠时间
        /// </summary>
        /// <returns></returns>
        public async Task<GetSleep> Sleep()
        {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.Sleep();
            });
        }
        /// <summary>
        /// 运行合集安装包
        /// </summary>
        /// <returns></returns>
        public async Task InstallRunTime() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.InstallRunTime();
            });
        }
        //课程中途关闭时执行上传日志
        public async Task GetinfoStr(string GetInfo_Tes) {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.GetinfoStr(GetInfo_Tes);
            });
        }
        //收集Windows日志
        public async Task GetMixed() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.GetMixed();
            });
        }
        //删除多余日志
        public async Task CleanMixed() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.CleanMixed();
            });
        }
        /// <summary>
        /// 检测包体完整性并处理
        /// </summary>
        /// <returns></returns>
        public async Task NewTest_() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.NewTest_();
            });
        }
        /// <summary>
        /// 传入前端错误信息
        /// </summary>
        /// <returns></returns>
        /// 
        public async Task<GetInfo> Getinfos() {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.Getinfos();
            });
        }
        /// <summary>
        /// PC 系统 版本信息   
        /// </summary>
        /// <returns></returns>
        public async Task<GetSyem> GetWinSyesm()
        {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.GetWinSyesm();
            });
        }
        /// <summary>
        /// DX版本信息 传入前端显示
        /// </summary>
        /// <returns></returns>
        public async Task<GetDxVersion> GetDxVer() {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.GetDxVer();
            });
        }
        //获取运行库版本
        public async Task GetRuntime() {
            await Task.Run(() => WinLiveManager.Instance.GetRuntime());
        }
        //硬件状态刷新
        public async Task WinReload() {
            await Task.Run(() => WinSystemInfo.Instance.WinReload());
        }
        //开机自启
        public async Task OpenTask() {
            await Task.Run(() => WinLiveManager.Instance.OpenTask());
        }
        //关闭自启
        public async Task CloseTask() {
            await Task.Run(() => WinLiveManager.Instance.CloseTask());
        }
        //USB监测
        public async Task GetAudio() {
            await Task.Run(() => WinLiveManager.Instance.GetAudio());
        }
        //定时删除 Newlog 日志文件
        public async Task CleanFile() {
            await Task.Run(() => WinLiveManager.Instance.CleanFile());
        }
        //定时删除 Logs 日志文件
        public async Task CleanFile_logs() {
            await Task.Run(() => WinLiveManager.Instance.CleanFile_logs());
        }
        //传入网络流量
        public async Task<NetWork_wsll> NetWord_sw()
        {
           return await Task.Run(() =>
            {
              return  WinLiveManager.Instance.NetWord_sw();
            });
        }

        public async Task<Getssinfo_s> GetSSInfo() {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.GetSSInfo();
            });
        }
        public async Task Delete_infoStr() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.Delete_infoStr();
            });
        }
        public async Task<NetWork_to> Network_to() {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.Network_to();
            });
        }
        public async Task AutoProgram(bool bOpen, string lessonId, string pid, string liveMode)
        {
            await Task.Run(() => WinLiveManager.Instance.AutoProgram(bOpen, lessonId, pid, liveMode));
        }

       /* public async Task AutoPushStream(bool bOpen, string lessonId)
        {
          *//*  await Task.Run(() => WinLiveManager.Instance.AutoPushStream(bOpen, lessonId));*//*
        }*/

        //这个方法保持同步
        public void UpdateSetting(Setting setting)
        {
            bool bReset = SettingManager.Instance.Setting.liveSetting.IsAgora != setting.liveSetting.IsAgora;
            SettingManager.Instance.Setting = setting;
            SettingManager.Instance.SaveSetting();
            LogHelper.LogMessage("用户更新配置:" + JsonConvert.SerializeObject(setting));
            LogUtility.Log("用户更新配置:" + JsonConvert.SerializeObject(setting));
            if (bReset)
            {
                string errStr = "直播供应商改变,后台正在重启.";
                Clients.Caller?.SendEnvError(errStr);
                Thread.Sleep(500);
                AppManage.Instance.CloseApp(true);
            }
        }
        public async Task<bool> UploadGame(string pid, string gamePath, string newVersion)
        {
            return await Task.Run(() =>
            {
                return WinLiveManager.Instance.UploadGame(pid, gamePath, newVersion);
            });
        }

        public async Task<HostVersion> UploadSelfPath(string gamePath)
        {
            return await Task.Run(() =>
            {
                return AppManage.Instance.UploadSelfPath(gamePath);
            });
        }

       
        public async Task<bool> UploadSelf(string gamePath, string localVersion)
        {
            return await Task.Run(() =>
            {
                return AppManage.Instance.UploadSelf(gamePath, localVersion);
            });
        }

        public async Task UpdateHost()
        {
            await Task.Run(() =>
            {
                AppManage.Instance.UpdateHost();
            });
        }

        /*  public async Task<bool> CloseLive()
          {
              return await Task.Run(() =>
              {
                  return WinLiveManager.Instance.CloseLive();
              });
          }*/
        /// <summary>
        /// 结束推流
        /// </summary>
        /// <returns></returns>
        public async Task CloseLiveS() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.CloseLiveS();
            });
        }

        public async Task ColseProgram()
        {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.ForceCloseProgram();
            });
        }

        public void CloseApp(string msg)
        {
#if DEBUG
            return;
#endif
            if (SettingManager.Instance.UserInfo == null)
                return;
            if (connectList.Contains(Context.ConnectionId))
                connectList.Remove(Context.ConnectionId);
            LogHelper.LogMessage($"当前用户数{connectList.Count}");
            LogUtility.Log($"当前用户数{connectList.Count}");
            if (connectList.Count > 0)
                return;
            //置顶时，第六个变量为MessageBoxOptions.ServiceNotification或MessageBoxOptions.DefaultDesktopOnly。
            var digResult = MessageBox.Show(msg, "前端提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            if (digResult == DialogResult.Yes)
            {
                LogHelper.LogMessage("用户重新打开前端");
                LogUtility.Log("用户重新打开前端",LogType.Trace);
                string appUri = Program.AppUri();
                Process.Start($"http://{appUri}?reload=1");
            }
            else
            {
                LogHelper.LogMessage("用户确认关闭后端,如果在推流中,自动关闭");
                LogUtility.Log("用户确认关闭后端,如果在推流中,自动关闭", LogType.Trace);
               /* WinLiveManager.Instance.CloseLive();*/
                AppManage.Instance.CloseApp();
            }
        }

        public void CloseServe()
        {
        #if DEBUG
            return;
        #endif
            LogHelper.LogMessage("用户确认关闭后端服务,如果在推流中,自动关闭");
            LogUtility.Log("用户确认关闭后端服务,如果在推流中,自动关闭", LogType.Trace);
           /* WinLiveManager.Instance.CloseLive();*/
            AppManage.Instance.CloseApp();
        }

        public async Task LogMessage(string meg, OeipLogLevel logLevel)
        {
            await Task.Run(() => LogHelper.LogMessage("(app) " + meg, logLevel));
        }

        public async Task AllUpdate(int type)
        {
            await Task.Run(() => WinLiveManager.Instance.AllUpdate(type));
        }
        //异步日志
        public async Task UploadLog(LessonInfo lesson)
        {
            await Task.Run(() =>
            {
                LogHelper.LogMessage($"upload log paramet:{JsonConvert.SerializeObject(lesson)}");
                WinLiveManager.Instance.UploadDirectory(lesson);
            });
        }
       
        //测试回调Lessoninfo对象参数
        public async Task TestDX(string uid_t, string appId_t, string startTimeStamp_t, string endTimeStamp_t, string dspId_t) {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.TestDX(uid_t, appId_t, startTimeStamp_t, endTimeStamp_t, dspId_t);
            });
        }
        /// <summary>
        /// 实时获取前端返回开课时间，和结束时间
        /// </summary>
        /// <param name="Minute_"></param>
        /// <param name="Houre_"></param>
        /// <param name="EndTime_"></param>
        /// <returns></returns>
        public async Task Up_GetMinute(int Minute_, int Houre_, int EndTime_, int StartTime_) {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.Up_GetMinute(Minute_, Houre_, EndTime_, StartTime_);
            });
        }
        /// <summary>
        /// 传入前段打开SteamVR日志配置手册方法
        /// </summary>
        /// <returns></returns>
        public async Task OpenDocx() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.OpenDocx();
            });
        }
        /// <summary>
        /// 传入前端可能会遇到的错误HTml
        /// </summary>
        /// <returns></returns>
        public async Task OpenErrorHtml() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.OpenErrorHtml();
            });
        }
      
        /// <summary>
        /// 运行库合集安装
        /// </summary>
        /// <returns></returns>
        /// 
        public async Task InstallRunTime_JC() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.InstallRunTime_JC();
            });
        }
        //删除多余包体
        public async Task AllDelete()
        {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.AllDelete();
            });
        }
        //传入异步刷新网络接收
        public async Task NetWrok_sub() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.NetWrok_sub();
            });
        }
        //区别Nlog日志返回
        public async Task NewUploadinfo(string logstring) {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.NewUploadinfo(logstring);
            });
        }
        /// <summary>
        /// 获取麦克风设备
        /// </summary>
        /// <returns></returns>
        public async Task GetAuDio_Sup() {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.GetAuDio_Sup();
            });
        }
      
        public override Task OnConnected()
        {
            LogHelper.LogMessage($"[{DateTime.Now.ToString("HH:mm:ss")}] {Context.ConnectionId} joined");
            if (!connectList.Contains(Context.ConnectionId))
                connectList.Add(Context.ConnectionId);
            return base.OnConnected();
        }


        public override Task OnReconnected()
        {
            LogHelper.LogMessage($"[{DateTime.Now.ToString("HH:mm:ss")}] {Context.ConnectionId} reconnect");
            if (!connectList.Contains(Context.ConnectionId))
                connectList.Add(Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            LogHelper.LogMessage($"[{DateTime.Now.ToString("HH:mm:ss")}] {Context.ConnectionId} left");
            if (connectList.Contains(Context.ConnectionId))
                connectList.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
        public async Task LoadWindows(bool bOpen, string lessonId) {
            await Task.Run(() =>
            {
                WinLiveManager.Instance.loadWindows(bOpen, lessonId);
            });
        }
    }
}
