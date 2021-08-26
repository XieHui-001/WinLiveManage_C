using OeipCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinLiveManage;

namespace AocePackage
{
    class CaptureObserver : ICaptureObserver
    {
        public int SetCount;
        public override void onEvent(CaptureEventId eventId, LogLevel level, string msg)
        {
            // 记录下,窗口lost表示沉下去了
            if(eventId == CaptureEventId.lost)
            {
                LiveAppHub.SendMessage("窗口捕获出错，请手动将课件置顶", OeipLogLevel.OEIP_WARN, 3000);
                SetCount = SetCount + 1;
                if (SetCount <= 1) {
                    AoceManager.Instance.ActiveWindow();
                }
            }
        }

        public override void onResize(int width, int height)
        {
            // 日志记录下,窗口大小变化
            LogHelper.LogMessage("推流情况下窗口大小发生变化:"+ width+"---"+height);
        }
        public override void onCapture(VideoFormat videoFormat, IntPtr device, IntPtr texture)
        {
            AoceManager.Instance.captureFrame(videoFormat, device, texture);
        }
    }
}
