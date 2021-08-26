using AudioSwitcher.AudioApi.CoreAudio;
using OeipCommon;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Test2.WinRing0;
using static WinLiveManage.NewLoginfo;

namespace WinLiveManage
{
  public class Vpo
    {
        private TestByWinRing0 testByWinRing0 = null;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);
        const uint WM_APPCOMMAND = 0x319;
        const uint APPCOMMAND_VOLUME_UP = 0x0a; const uint APPCOMMAND_VOLUME_DOWN = 0x09;
        const uint APPCOMMAND_VOLUME_MUTE = 0x08;
        DispatcherTimer timer;
       private static object objc = new object();
        public void Vos()
        {
            Form1 fo = new Form1();
            try
            {
                CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
                Debug.WriteLine("Current Volume:" + defaultPlaybackDevice.Volume);
                SendMessage(fo.Handle, WM_APPCOMMAND, 0x30292, APPCOMMAND_VOLUME_UP * 0x10000);
                defaultPlaybackDevice.Volume = 100;
            }
            catch (Exception ex) {
                LogUtility.Log("系统未检测到有效输出音源设备"+ex, LogType.Error);
            }
           
        }
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            // 设置结构体块容量 
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            // 捕获的时间 
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        // 获取键盘和鼠标没有操作的时间
      

      public  static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            // 捕获时间 
            if (!GetLastInputInfo(ref vLastInputInfo))
            {
                return 0;
            }
            else
            {
                return (Environment.TickCount - (int)vLastInputInfo.dwTime) / 1000;
            }
        }
        public string sleep() {
            string publi = GetLastInputTime().ToString();
            return publi;
        }


        //Open 
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
        //Cpu负荷率
        internal static string GetTemperatureOftheCPU()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
          
                UpdateVisitor updateVisitor = new UpdateVisitor();
                Computer computer = new Computer();
                try
                {
                    try { computer.Open(); } catch { }
                    try { computer.CPUEnabled = true; } catch { }
                    try { computer.Accept(updateVisitor); } catch { }

                    for (int i = 0; i < computer.Hardware.Length; i++)
                    {
                        if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                        {
                            for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                                {
                                    if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                                        m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.Log("" + ex, LogType.Error);
                }
                try { computer.Close(); } catch { }
            return $"{  m_cpuTemperature }";
            }
        }
        //调用方法Cpu负荷
        public string GetCpu() {
            lock (objc) { 
            string getcpu = GetTemperatureOftheCPU().ToString();

            if (getcpu.ToString() != "" && getcpu.ToString() != null && getcpu.ToString().Length > 0)
            {
                try { getcpu = getcpu.Substring(0, getcpu.IndexOf('.')); } catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }
            }
            return getcpu.ToString();
            }
        }
        //N卡温度
        internal static string GetTemperatureOftheNGPU()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            try {
                try {

                    try { computer.Open(); } catch { }
                    try { computer.GPUEnabled = true; } catch { }
                    try { computer.Accept(updateVisitor); } catch { } 
                } catch (Exception ex) {
                    LogUtility.Log("ERROR:初始化英伟达GPU失败" + ex, LogType.Error);
                }
              
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                                    m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogUtility.Log("" + ex, LogType.Error);
            }
            try { computer.Close(); } catch { }  
            return $"{  m_cpuTemperature }";
            }
        }
        //调用方法N卡温度
        public string GetGPU() {
            lock (objc) { 
            string getgpu = GetTemperatureOftheNGPU().ToString();
            return getgpu;
            }
        }
        //N卡负荷
        internal static string GetloadNGPU()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            try {
                try {
                    try { computer.Open(); } catch { }
                    try { computer.GPUEnabled = true; } catch { }
                    try { computer.Accept(updateVisitor); } catch { } 

                } catch (Exception ex) {
                    LogUtility.Log("ERROR:初始化英伟达GPU失败" + ex, LogType.Error);
                }
               
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                                    m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                            }
                        }
                    }
                }

            } catch (Exception ex) {
                LogUtility.Log("" + ex, LogType.Error);
            }

            try { computer.Close(); } catch { } 
            return $"{  m_cpuTemperature }";
            }
        }
        //N卡负荷调用方法
        public string GetLoad_NGPU() {
            lock (objc) { 
            string getloadngpu = GetloadNGPU().ToString();
            if (getloadngpu.ToString() != "" && getloadngpu.ToString() != null && getloadngpu.ToString().Length > 0)
            {
                try { getloadngpu = getloadngpu.Substring(0, getloadngpu.IndexOf('.')); } catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }
               
            }
            return getloadngpu;
            }
        }
        //A卡温度
        internal static string GetTemperatureOftheAGPU()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            try
            {
                try {
                    try { computer.Open(); } catch { }
                    try { computer.GPUEnabled = true; } catch { }
                    try { computer.Accept(updateVisitor); } catch { }
                } catch (Exception ex) {
                    LogUtility.Log("ERROR:初始化AMD GPU失败" + ex, LogType.Error);
                }
             
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.GpuAti)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                                    m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                LogUtility.Log("" + ex, LogType.Error);
            }

            try { computer.Close(); } catch { } 
            return $"{  m_cpuTemperature }";
            }
        }
        //A卡温度调用方法
        public string GetAGPU() {
            lock (objc) { 
            string getagpu = GetTemperatureOftheAGPU().ToString();
            return getagpu;
            }
        }
        //A卡负荷
        internal static string GetLoadAGPU()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            try
            {
                try
                {
                    try { computer.Open(); } catch { }
                    try { computer.GPUEnabled = true; } catch { }
                    try { computer.Accept(updateVisitor); } catch { } 
                } catch (Exception ex) {
                    LogUtility.Log("ERROR:初始化AMD GPU失败" + ex, LogType.Error);
                }
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.GpuAti)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                                    m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                LogUtility.Log("0" + ex, LogType.Error);
            };
            try { computer.Close(); } catch { }
          
            return $"{  m_cpuTemperature }";
            }
        }
        //A卡负荷调用
        public string GetAGPULOAD() {
            lock (objc) { 
            string getloadgpu = GetLoadAGPU().ToString();
            if (getloadgpu.ToString() != "" && getloadgpu.ToString() != null && getloadgpu.ToString().Length > 1)
            {
                try {
                    getloadgpu = getloadgpu.Substring(0, getloadgpu.IndexOf('.'));
                } catch (Exception ex) {
                    LogUtility.Log("" + ex, LogType.Error);
                }
            }
            return getloadgpu;
            }
        }
        //GPU负荷
        internal static string GetLoadGPU()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            try
            {
                try {
                    try { computer.Open(); } catch { }
                    try { computer.GPUEnabled = true; } catch { }
                    try { computer.Accept(updateVisitor); } catch { }  
                } catch (Exception ex) {
                    LogUtility.Log("" + ex, LogType.Error);
                }
               
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                                    m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                LogUtility.Log("" + ex, LogType.Error);
            }
            try { computer.Close(); } catch { }
            return $"{  m_cpuTemperature }";
            }
        }
        //GPU负荷调用方法
        public string PubGetloadGPU() {
            lock (objc) { 
            string pubgetloadgpu = GetLoadGPU().ToString();
            if (pubgetloadgpu.ToString() != "" && pubgetloadgpu.ToString() != null && pubgetloadgpu.ToString().Length > 0)
            {
                try { pubgetloadgpu = pubgetloadgpu.Substring(0, pubgetloadgpu.IndexOf('.')); } catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }
               
            }
            return pubgetloadgpu;
            }
        }
        //硬盘温度
        internal static string GetTemperatureOftheHDD()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            try
            {
                try {
                    try { computer.Open(); } catch { }
                    try { computer.HDDEnabled = true; } catch { }
                    try { computer.Accept(updateVisitor); } catch { }
                } catch (Exception ex) {
                    LogUtility.Log("ERROR:初始化硬盘失败" + ex, LogType.Error);
                }
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.HDD)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                                    m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                LogUtility.Log("" + ex, LogType.Error);
            }
            try { computer.Close(); } catch { }  
            return $"{  m_cpuTemperature }";
            }
        }
        //硬盘温度方法调用
        public string GetHDD() {
            lock (objc) { 
            string gethdd = GetTemperatureOftheHDD().ToString();
            return gethdd.ToString();
            }
        }
        //硬盘负荷
        internal static string GetTLoadHDD()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            try
            {
                try
                {
                    try { computer.Open(); } catch { }
                    try { computer.HDDEnabled = true; } catch { } 
                    try { computer.Accept(updateVisitor); } catch { }
                   
                }
                catch (Exception ex) {
                    LogUtility.Log("ERROR:初始化硬盘失败" + ex, LogType.Error);
                }
                
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.HDD)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                                    m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                            }
                        }
                    }
                }
            }
            catch(Exception ex) {
                LogUtility.Log("" + ex, LogType.Error);
            }
            try { computer.Close(); } catch { } 
            return $"{  m_cpuTemperature }";
            }
        }
        //硬盘负荷方法调用
        public string GetloadHdd_() {
            lock (objc) { 
            string getloadhdd = GetTLoadHDD().ToString();
            if (getloadhdd.ToString() != "" && getloadhdd.ToString() != null && getloadhdd.ToString().Length > 0)
            {
                try { getloadhdd = getloadhdd.Substring(0, getloadhdd.IndexOf('.')); } catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }
              
            }
            return getloadhdd.ToString();
            }
        }
        // try { } catch (Exception ex) { }
        //Ram内存负荷   负荷等于百分比
        internal static string GetTemperatureOftheRAM()
        {
            lock (objc) { 
            string m_cpuTemperature = "";
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            try
            {
                try {

                    try { computer.Open(); } catch { }
                    try { computer.RAMEnabled = true; } catch { }
                    try { computer.Accept(updateVisitor); } catch { }
                } catch (Exception ex) {
                    LogUtility.Log("ERROR:初始化内存失败" + ex, LogType.Error);
                }
              
                for (int i = 0; i < computer.Hardware.Length; i++)
                {
                    if (computer.Hardware[i].HardwareType == HardwareType.RAM)
                    {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                        {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                            {
                                if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load)
                                    m_cpuTemperature = computer.Hardware[i].Sensors[j].Value.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                LogUtility.Log("" + ex, LogType.Error);
            }
            try { computer.Close(); } catch { } 
            return $"{  m_cpuTemperature }";
            }
        }
        public string NewgetRam() {
            lock (objc) { 
            string strRam = GetTemperatureOftheRAM().ToString();
            if (strRam.ToString() != "" && strRam.ToString() != null && strRam.ToString().Length > 0)
            {
                try { strRam = strRam.Substring(0, strRam.IndexOf('.')); } catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }
                
            }
            return strRam;
            }
        }

        public void oTime()
        {
            Form1 fo = new Form1();
            fo.timeo();
           
        }
        public void Window_Loaded()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer1_Tick;
            timer.Start();
        }

        public void timer1_Tick(object sender, EventArgs e)
        {
            //定时执行的内容
            oTime();
        }
        //获取Cpu实时温度
        public string OpenGetTemptrue() {
            lock (objc) { 
            string getcpu = "";
            testByWinRing0 = new TestByWinRing0();
            bool initResult = testByWinRing0.Initialize();
            /*testByWinRing0.InitSuperIO();
            testByWinRing0.InitEc();*/
            try {
                if (!initResult)
                {
                    LogUtility.Log("初始化硬件失败", LogType.Error);
                }
                else
                {
                    LogUtility.Log("初始化硬件获取成功", LogType.Trace);
                    getcpu = testByWinRing0.CpuTemp().ToString();
                }
            }
            catch (Exception ex) {
                LogUtility.Log("初始化硬件异常:" + ex, LogType.Error);
            }
            return getcpu;
            }
        }
      
        public void Delete_1()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR_Test\Let_Us_Begin_One2Eight";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void Delete_2()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR_Test\Let_us_begin3_U5U6";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void Delete_3()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR_Test\Let_us_begin4_U7U8";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void Delete_4()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR_Test\Let_Us_Go_U1U2";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void Delete_5()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR_Test\Let_Us_GO1_U3U4";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }
        }
        public void Delete_6()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR_Test\Let_Us_GO1_U5U6";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void Delete_7()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR_Test\Let_Us_Go2_U1U2";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void Delete_8()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR_Test\Let_Us_Begin_Nine2Sixteen";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void TDelete_1()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR\Let_Us_Begin_U1U2";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void TDelete_2()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR\Let_Us_Begin_U3U4";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void TDelete_3()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR\Let_Us_Go1_U5U6";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void TDelete_4()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR\Let_Us_Begin_U7U8";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void TDelete_5()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR\Let_Us_Go1_U1U2";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }
        }
        public void TDelete_6()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MRt\Let_Us_Go1_U5U6";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void TDelete_7()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR\Let_Us_Go1_U7U8";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }
        public void TDelete_8()
        {
            try
            {
                string pathfile = @"D:\IVREAL_MR\Let_Us_Go2_One2Eight";
                if (Directory.Exists(pathfile))
                {
                    Directory.Delete(pathfile, true);
                }
            }
            catch (Exception ex) { LogUtility.Log("" + ex, LogType.Error); }

        }

        public void AllDeleteTs()
        {
            Delete_1();
            Delete_2();
            Delete_3();
            Delete_4();
            Delete_5();
            Delete_6();
            Delete_7();
            Delete_8();
            TDelete_1();
            TDelete_2();
            TDelete_3();
            TDelete_4();
            TDelete_5();
            TDelete_6();
            TDelete_7();
            TDelete_8();
        }
    }
   
}
