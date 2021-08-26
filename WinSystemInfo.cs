using OeipCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test2.WinRing0;
using static WinLiveManage.NewLoginfo;

namespace WinLiveManage
{
    [Serializable]
    public class CPUInfo
    {
        public string name = string.Empty;
        public string temp = string.Empty;
        public string cpuload = string.Empty;
        public float usageRate = 0.0f;
    }

    [Serializable]
    public class MemoryInfo
    {
        public string memoryload = string.Empty;
        public float total = 0.0f;
        public float usage = 0.0f;
    }

    [Serializable]
    public class VideoCardInfo
    {
        public int count = 0;
        public string displayfh = string.Empty;
        public string displaywd = string.Empty;
        public string Nvadwd = string.Empty;
        public string Nvadfh = string.Empty;
        public List<string> current = new List<string>();
    }

    [Serializable]
    public class DiskInfo
    {
        public string name = string.Empty;
        public string disktemp = string.Empty;
        public string diskload = string.Empty;
        public float total = 0.0f;
        public float usage = 0.0f;
    }

    [Serializable]
    public class HardwareInfo
    {
        public CPUInfo cpuInfo = new CPUInfo();
        public MemoryInfo memoryInfo = new MemoryInfo();
        public VideoCardInfo videoCardInfo = new VideoCardInfo();
        public DiskInfo diskInfo = new DiskInfo();
    }

    public class WinSystemInfo : MSingleton<WinSystemInfo>
    {
        //private PerformanceCounter cpuCounter = null;
        //private PerformanceCounter ramCounter = null;
        //private PerformanceCounter diskCounter = null;
        private ManagementObjectSearcher cpuQuery = null;
        private ManagementObjectSearcher ramQuery = null;

        public HardwareInfo hardwareInfo = new HardwareInfo();

        protected override void Init()
        {
            try
            {
                //cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                //ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                //diskCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");

                ramQuery = new ManagementObjectSearcher("select AvailableBytes from Win32_PerfFormattedData_PerfOS_Memory");
                cpuQuery = new ManagementObjectSearcher("select PercentProcessorTime from Win32_PerfFormattedData_PerfOS_Processor WHERE Name=\"_Total\"");
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("初始化硬件统计对象出错", ex);
            }
        }
        //获取显卡信息
        public void GetVideoCard(bool bOnce = false)
        {
            if (bOnce)
            {
                hardwareInfo.videoCardInfo.count = 0;
                hardwareInfo.videoCardInfo.current.Clear();
                using (var mc = new ManagementClass("Win32_VideoController"))
                {
                    using (var moc = mc.GetInstances())
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            string name = mo.Properties["Name"]?.Value?.ToString();
                            string status = mo.Properties["Status"]?.Value?.ToString();
                            string scanMode = mo.Properties["CurrentScanMode"]?.Value?.ToString();
                            if (status == "OK")
                            {
                                hardwareInfo.videoCardInfo.count++;
                                if (string.IsNullOrEmpty(scanMode))
                                {
                                    LogHelper.LogMessage($"显卡 {name}:连接输出设备");
                                    hardwareInfo.videoCardInfo.current.Add(name);
                                }
                            }
                        }
                    }
                }
            }
        }
        //CPU温度
        public void GetNewCpuTempTrue(bool bOnce = false)
        {
         
            if (bOnce)
            {
                hardwareInfo.cpuInfo.temp = "0";
                testByWinRing0 = new TestByWinRing0();
                bool initResult = testByWinRing0.Initialize();
                try
                {
                    if (!initResult)
                    {
                        LogUtility.Log("初始化硬件失败", LogType.Error);
                    }
                    else
                    {
                       
                        hardwareInfo.cpuInfo.temp = Convert.ToString(testByWinRing0.CpuTemp());
                        LogUtility.Log("初始化硬件获取成功保存Cpu温度:" + hardwareInfo.cpuInfo.temp, LogType.Trace);
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.Log("初始化硬件异常:" + ex, LogType.Error);
                }
            }
        }
        //CPU负荷
        public void GetNewCpuload(bool bOnce = false) {
          
            if (bOnce) {
                hardwareInfo.cpuInfo.cpuload = "0";
                Vpo po = new Vpo();
                hardwareInfo.cpuInfo.cpuload = Convert.ToString(po.GetCpu());
                LogUtility.Log("初始化硬件获取成功保存Cpu负荷:" + hardwareInfo.cpuInfo.cpuload, LogType.Trace);
            }
        }
        //A卡负荷
        public void NewGetVideoCard(bool bOnce = false) {
           
            if (bOnce) {
                hardwareInfo.videoCardInfo.displayfh = "0";
                Vpo po = new Vpo();
                hardwareInfo.videoCardInfo.displayfh = Convert.ToString(po.GetAGPULOAD());
                LogUtility.Log("初始化硬件获取成功保存A卡负荷:" + hardwareInfo.videoCardInfo.displayfh, LogType.Trace);
            }
        }
        //A卡温度
        public void NewGetVideoCardTemp(bool bOnce = false) {
            
            if (bOnce) {
                hardwareInfo.videoCardInfo.displaywd = "0";
                Vpo po = new Vpo();
                hardwareInfo.videoCardInfo.displaywd = Convert.ToString(po.GetAGPU());
                LogUtility.Log("初始化硬件获取成功保存A卡温度:" + hardwareInfo.videoCardInfo.displaywd, LogType.Trace);
            }
        }
        //硬盘温度
        public void NewGetHDD(bool bOnce = false) {
           
            if (bOnce) {
                hardwareInfo.diskInfo.disktemp = "0";
                Vpo po = new Vpo();
                hardwareInfo.diskInfo.disktemp = Convert.ToString(po.GetHDD());
                LogUtility.Log("初始化硬件获取成功保存硬盘温度:" + hardwareInfo.diskInfo.disktemp, LogType.Trace);
            }
        }
        //硬盘负荷
        public void NewGetloadHdd(bool bOnce = false) {
            
            if (bOnce) {
                hardwareInfo.diskInfo.diskload = "0";
                Vpo po = new Vpo();
                hardwareInfo.diskInfo.diskload = Convert.ToString(po.GetloadHdd_());
                LogUtility.Log("初始化硬件获取成功保存硬盘负荷:" + hardwareInfo.diskInfo.diskload, LogType.Trace);
            }
        }
        //内存负荷
        public void NewGetMemoryLoad(bool bOnce = false) {
           
            if (bOnce) {
                hardwareInfo.memoryInfo.memoryload = "0";
                Vpo po = new Vpo();
                hardwareInfo.memoryInfo.memoryload = Convert.ToString(po.NewgetRam());
                LogUtility.Log("初始化硬件获取成功保存内存负荷:" + hardwareInfo.memoryInfo.memoryload, LogType.Trace);
            }
        }
        //N卡温度
        public void NewGetNvdaTemp(bool bOnce = false) {
           
            if (bOnce) {
                hardwareInfo.videoCardInfo.Nvadwd = "0";
                Vpo po = new Vpo();
                hardwareInfo.videoCardInfo.Nvadwd = Convert.ToString(po.GetGPU());
                LogUtility.Log("初始化硬件获取成功保存N卡温度:" + hardwareInfo.videoCardInfo.Nvadwd, LogType.Trace);
            }
        }
        //N卡负荷 
        public void NewGetNvdaload(bool bOnce = false) {
            if (bOnce) {
                hardwareInfo.videoCardInfo.Nvadfh = "0";
                Vpo po = new Vpo();
                hardwareInfo.videoCardInfo.Nvadfh = Convert.ToString(po.PubGetloadGPU());
                LogUtility.Log("初始化硬件获取成功保存N卡负荷:" + hardwareInfo.videoCardInfo.Nvadfh, LogType.Trace);
            }
        }
        //获取本机内存条大小
        public void GetMemory(bool bOnce = false)
        {
            hardwareInfo.memoryInfo.total = 0;
            hardwareInfo.memoryInfo.usage = 0;
            using (var mc = new ManagementClass("Win32_OperatingSystem"))
            {
                using (var moc = mc.GetInstances())
                {
                    foreach (ManagementObject mo in moc)
                    {
                        string capity = mo.Properties["TotalVisibleMemorySize"]?.Value?.ToString();
                        hardwareInfo.memoryInfo.total += getCapacity(capity);
                        string availablebytes = mo.Properties["FreePhysicalMemory"]?.Value?.ToString();
                        hardwareInfo.memoryInfo.usage += getCapacity(availablebytes);
                        ShowManagementObject(mo);
                    }
                }
            }
        }
        private TestByWinRing0 testByWinRing0 = null;
        //获取CPU信息
        public void GetCpu(bool bOnce = false)
        {
            if (bOnce)
            {
                using (var mc = new ManagementClass("Win32_Processor"))
                {
                    using (var moc = mc.GetInstances())
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            hardwareInfo.cpuInfo.name = mo.Properties["Name"]?.Value?.ToString();
                            ShowManagementObject(mo);
                        }
                    }
                }
              
            }
            var cpuItem = cpuQuery.Get().Cast<ManagementObject>().Select(item => item["PercentProcessorTime"]?.ToString()).First();
            float.TryParse(cpuItem, out hardwareInfo.cpuInfo.usageRate);
        }
       
        //获取硬盘信息
        public void GetDisk(bool bOnce = false)
        {
            string rootPath = Path.GetPathRoot(SettingManager.Instance.GetPathSetting().GamePath);
            using (var mc = new ManagementClass("Win32_LogicalDisk"))
            {
                using (var moc = mc.GetInstances())
                {
                    foreach (ManagementObject mo in moc)
                    {
                        string deviceId = mo.Properties["DeviceID"]?.Value?.ToString();
                        if (rootPath.Contains(deviceId))
                        {
                            hardwareInfo.diskInfo.name = deviceId;
                            hardwareInfo.diskInfo.total = getCapacity(mo.Properties["Size"]?.Value?.ToString(), 1024.0f);
                            hardwareInfo.diskInfo.usage = getCapacity(mo.Properties["FreeSpace"]?.Value?.ToString(), 1024.0f);
                        }
                    }
                }
            }
        }

        public void ShowManagementObject(ManagementObject mo)
        {
#if DEBUG
            Console.WriteLine("-----------");
            foreach (var prop in mo.Properties)
            {
                Console.WriteLine($"{prop.Name}:{prop.Value}");
            }
#endif
        }
        public void WinReload() {
            try {
                WinSystemInfo.Instance.GetHardware(true);
                LiveAppHub.Hub.Clients.All.SendHardware(hardwareInfo);
            } catch { }
        }
        public void GetHardware(bool bOnce = false)
        {
            GetVideoCard(bOnce);
            GetMemory(bOnce);
            GetCpu(bOnce);
            GetDisk(bOnce);
            GetNewCpuTempTrue(bOnce);
            NewGetVideoCard(bOnce);
            NewGetVideoCardTemp(bOnce);
            NewGetHDD(bOnce);
            NewGetloadHdd(bOnce);
            GetNewCpuload(bOnce);
            NewGetMemoryLoad(bOnce);
            NewGetNvdaTemp(bOnce);
            NewGetNvdaload(bOnce);
            LiveAppHub.Hub.Clients.All.SendHardware(hardwareInfo);
        }

        private float getCapacity(string strValue, float baseValue = 1.0f)
        {
            //返回GB 如果是KB-baseValue=1,如果Byte-baseValue=1024
            if (Int64.TryParse(strValue, out Int64 value))
            {
                return value / 1024.0f / 1024.0f / baseValue;
            }
            return 0.0f;
        }

    }
}
