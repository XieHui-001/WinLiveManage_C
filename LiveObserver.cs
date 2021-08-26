using OeipCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinLiveManage;
using static WinLiveManage.NewLoginfo;

namespace AocePackage
{
    class LiveObserver : ILiveObserver
    {
        public override void onEvent(int operater, int code, LogLevel level, string msg)
        {
            // 推流中发生事件,记录日志
            LogHelper.LogMessage("推流时:"+operater + ":" + code +":"+ level +":"+ msg);
        }
        // 当前推流登录是否成功
        public override void onLoginRoom(bool bReConnect)
        {
            AoceManager.Instance.setPushSetting();
            WinLiveManager.Instance.BOpenLive = true;
            LogHelper.LogMessage("登录成功", OeipLogLevel.OEIP_INFO);
           
        }

        public override void onUserChange(int userId, bool bAdd)
        {

        }
        [Serializable]
        public class GetImagedata_
        {
            public int fps { get; set; }
            public int kbs { get; set; }
            public bool ifWind { get; set; }
            public string windowTitle { get; set; }
            public bool BOpenLives { get; set; }
        }

        private GetImagedata_ womdpwLives = new GetImagedata_();
        public override void onPushQuality(int index, int quality, float fps, float kbs)
        {
            womdpwLives.fps = Convert.ToInt32(fps);
            womdpwLives.kbs = Convert.ToInt32(kbs);
            womdpwLives.windowTitle = "PlaySceneWindow";
            womdpwLives.ifWind = AoceManager.Instance.LiveWindow != null;
            LiveAppHub.Hub.Clients.All.PlayQuality(womdpwLives);
            LogHelper.LogMessage("推流中------FPS:"+ fps+"码率:"+ kbs);
            LogUtility.Log("推流中------FPS:" + fps + "码率:" + kbs, LogType.Trace);
        }
    }
}
