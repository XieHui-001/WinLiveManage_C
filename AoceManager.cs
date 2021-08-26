
using OeipCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinLiveManage;

namespace AocePackage
{
    public class AoceManager : MSingleton<AoceManager>
    {
        private List<IWindow> windows = new List<IWindow>();
        private ILiveRoom room = null;
        private LiveObserver liveObserver = new LiveObserver();
        private IPipeGraph pipeGraph = null;
        private IInputLayer inputLayer = null;
        private IOutputLayer outputLayer = null;
        private IReSizeLayer resizeLayer = null;
        private IYUVLayer yuvLayer = null;
        private GpuType gpuType = GpuType.other;
        private CaptureType captureType = CaptureType.other;
        private LogObserver logObserver = new LogObserver();
        private CaptureObserver captureObserver = new CaptureObserver();
        private OutLayerObserver observer = new OutLayerObserver();
        private IWindow liveWindow = null;
        private VideoFormat videoFormat = new VideoFormat();
        private VideoFrame videoFrame = new VideoFrame();
        private object obj = new object();
        private GpuType selectGPU()
        {
            GpuType gpuType = GpuType.other;
            bool bLoad = AoceWrapper.checkLoadModel("aoce_cuda");
            if (!bLoad)
            {
                bLoad = AoceWrapper.checkLoadModel("aoce_vulkan");
                if (bLoad)
                {
                    gpuType = GpuType.vulkan;
                }
            }
            else
            {
                gpuType = GpuType.cuda;
            }
            return gpuType;
        }

        private CaptureType selectCapture()
        {
            CaptureType captureType = CaptureType.other;
            bool bLoad = AoceWrapper.checkLoadModel("aoce_winrt");
            if (!bLoad)
            {
                bLoad = AoceWrapper.checkLoadModel("aoce_win");
                if (bLoad)
                {
                    captureType = CaptureType.win_bitblt;
                }
            }
            else
            {
                captureType = CaptureType.win_rt;
            }
            return captureType;
        }

        public ICaptureWindow CaptureWindow { get; private set; } = null;
        public bool BPush { get => bPush; set => bPush = value; }
        public IWindow LiveWindow { get => liveWindow; }

        /// <summary>
        /// 提供给前面初始化
        /// </summary>
        public void Getinit()
        {
            lock (obj)
            {
                Init();
            }
        }
        protected override void Init()
        {
            lock (obj)
            {
                AoceWrapper.setLogObserver(logObserver);
                AoceWrapper.loadAoce();

                // 设置窗口输出格式 
                videoFormat.width = 1280;
                videoFormat.height = 720;
                videoFormat.videoType = VideoType.yuv420P;
                gpuType = selectGPU();
                if (gpuType == GpuType.other)
                {
                    //日志记录
                    return;
                }
                pipeGraph = AoceWrapper.getPipeGraphFactory(gpuType).createGraph();
                var layerFactory = AoceWrapper.getLayerFactory(gpuType);
                inputLayer = layerFactory.createInput();
                outputLayer = layerFactory.createOutput();
                resizeLayer = layerFactory.createSize();
                yuvLayer = layerFactory.createRGBA2YUV();

                outputLayer.setObserver(observer);

                InputParamet inputParamet = new InputParamet();
                inputParamet.bCpu = 0;
                inputParamet.bGpu = 1;
                inputLayer.updateParamet(inputParamet);

                OutputParamet outputParamet = new OutputParamet();
                outputParamet.bCpu = 1;
                outputParamet.bGpu = 0;

                ReSizeParamet reSize = new ReSizeParamet();
                reSize.bLinear = 1;
                reSize.newWidth = videoFormat.width;
                reSize.newHeight = videoFormat.height;
                resizeLayer.updateParamet(reSize);

                YUVParamet yuvParamet = new YUVParamet();
                yuvParamet.type = videoFormat.videoType;
                yuvLayer.updateParamet(yuvParamet);

                pipeGraph.addNode(inputLayer).addNode(resizeLayer).addNode(yuvLayer).addNode(outputLayer);

                captureType = selectCapture();
                if (captureType == CaptureType.other)
                {
                    return;
                }
                CaptureWindow = AoceWrapper.getWindowCapture(captureType);
                CaptureWindow.setObserver(captureObserver);
                InitLive();
            }
        }

        public override void Close()
        {
            AoceWrapper.unloadAoce();
        }

        private List<IWindow> getWindows(bool bUpdate = false)
        {
            if (windows.Count == 0 || bUpdate)
            {
                windows.Clear();
                var winManager = AoceWrapper.getWindowManager(WindowType.win);
                int count = winManager.getWindowCount(bUpdate);
                for (int i = 0; i < count; i++)
                {
                    var window = winManager.getWindow(i);
                    windows.Add(window);
                }
            }
            return windows;
        }

        public void InitLive()
        {
            AgoraContext agoraContext = new AgoraContext();
            agoraContext.bLoopback = 1;

            room = AoceWrapper.getLiveRoom(LiveType.agora);
            room.initRoom(AgoraContext.getCPtr(agoraContext).Handle, liveObserver);
        }
        private IWindow findWindow(string title)
        {
            var cwindows = getWindows(true);
            foreach (var win in cwindows)
            {
                if (win.getTitle() == title || win.getTitle().Contains(title))
                {
                    return win;
                }
            }
            return null;

        }
        private string LessonIds = string.Empty;
        public bool LoginLive(string roomName, bool bTestEnv)
        {
            bool bResult = room.loginRoom(roomName + (bTestEnv ? "-test_wx" : "_wx"), 800, 1);
            LessonIds = roomName;
            return bResult;
        }

        public void setPushSetting()
        {
            // 设定推流格式
            PushSetting pushSetting = new PushSetting();
            pushSetting.videoStream.width = videoFormat.width;
            pushSetting.videoStream.height = videoFormat.height;
            pushSetting.videoStream.videoType = videoFormat.videoType;
            pushSetting.videoStream.fps = 30;
            room.pushStream(0, pushSetting);
            LiveAppHub.Hub.Clients.All.StartLive(LessonIds, 1);
        }
        private bool bPush = false;
        public bool Tick()
        {
            lock (obj)
            {
                if (BPush && LiveWindow != null)
                {
                    if (!CaptureWindow.renderCapture())
                    {
                        liveWindow = null;
                    }
                }
            }
            return false;
        }
        public void StartCapture(string title)
        {
            lock (obj)
            {
                BPush = false;
                liveWindow = findWindow(title);
                if (LiveWindow == null)
                {
                    return;
                }
                ColseCapture();
                BPush = CaptureWindow.startCapture(LiveWindow, false);
            }
        }
        /// <summary>
        /// 结束推流
        /// </summary>
        public void ColseCapture()
        {
            if (CaptureWindow == null && !CaptureWindow.bCapturing())
            {
                return;
            }
            CaptureWindow.stopCapture();
        }

        internal void captureFrame(VideoFormat videoFormat, IntPtr device, IntPtr texture)
        {
            inputLayer.setImage(videoFormat);
            inputLayer.inputGpuData(device, texture);
            pipeGraph.run();
        }

        internal void pushVideoFrame(IntPtr data, ImageFormat imageFormat)
        {
            AoceWrapper.createVideoFrame(videoFrame, data, imageFormat.width, imageFormat.height, videoFormat.videoType);
            room.pushVideoFrame(0, videoFrame);
        }
        /// <summary>
        /// 结束推流
        /// </summary>
        public void LogoutLive()
        {
            lock (obj) {
                try { 
                ColseCapture();
                room.logoutRoom();
                LiveAppHub.Hub.Clients.All.StopLive(LessonIds, 1);
                BPush = false;
                }
                catch (Exception e) {
                    LogHelper.LogMessage("Aoce 推流关闭时出错：" + e);
                }

            }
        }
        /// <summary>
        /// 激活窗口
        /// </summary>
        public void ActiveWindow()
        {
            try
            {
                if (LiveWindow != null)
                {
                    // && !liveWindow.bValid()
                    AoceWrapper.getWindowManager(WindowType.win).setForeground(LiveWindow);
                    LiveAppHub.SendMessage("正在尝试激活窗口", OeipLogLevel.OEIP_WARN, 3000);
                    LogHelper.LogMessage("ActiveWindow:::正在尝试激活窗口");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage("尝试激活窗口时失败" + ex);
            }
        }
    }
}
