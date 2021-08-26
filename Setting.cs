using OeipCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace WinLiveManage
{
    [Serializable]
    public class HostVersion
    {
        public string localVersion = string.Empty;
        public string remoteVersion = string.Empty;
    }

    [Serializable]
    public class LessonInfo
    {
        public string uid = string.Empty;
        public string appId = string.Empty;
        public string startTimeStamp = string.Empty;
        public string endTimeStamp = string.Empty;
        public string dspId = string.Empty;
    }

    [Serializable]
    public class CommonInfo
    {
        public int code = 0;
        public string msg = string.Empty;
        public string deviceId = string.Empty;
        public string token = string.Empty;
    }

    [Serializable]
    public class User
    {
        public string name = string.Empty;
        public string password = string.Empty;
        public string role = string.Empty;
        public bool bTestEnv = false;
        public int UserStar = 0;

        public bool IsTeacher()
        {
            return this.role == "teacher";
        }
    }

    [Serializable]
    public class LiveSetting
    {
        public int Rate { get; set; } = 2000000;
        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;
        public bool IsAutoStream { get; set; } = true;
        public bool IsAgora { get; set; } = true;
    }

    [Serializable]
    public class ProgramSetting
    {
        public string Id { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    [Serializable]
    public class PathSetting
    {
        /// <summary>
        /// 
        /// 路径路径
        /// </summary>
        public string GamePath { get; set; } = string.Empty;
        /// <summary>
        /// 所有游戏相关的配置路径
        /// </summary>
        public string SettingPath { get; set; } = string.Empty;
    }

    [Serializable]
    public class LaunchpadSetting
    {
        public bool IsAutoStart { get; set; } = true;
        public bool IsInternal { get; set; } = false;
        public PathSetting DefaultPath { get; set; } = new PathSetting();
        public PathSetting DefaultPathTest { get; set; } = new PathSetting();
        //内网自身上传与下载地址
        public string SelfUpdateAddressInternal { get; set; } = @"ftp://192.168.0.158/LauncherIvreal";
        //外网自身的下载地址
        public string SelfUpdateAddress { get; set; } = @"http://download.talkdoo.com/LauncherIvreal";

        public string GetUpdateAddress()
        {
            if (IsInternal && AppManage.Instance.InnerNet)
                return SelfUpdateAddressInternal;
            return SelfUpdateAddress;
        }
    }

    [Serializable]
    public class AudioSetting
    {
        public bool Check = false;
        public bool IsAutoSetting = false;
        public string InputId = string.Empty;
        public string OutputId = string.Empty;
        public int InputVolume = -1;
        public int OutputVolume = -1;
    }

    public class Setting : IXmlSerializable
    {
        public LiveSetting liveSetting = new LiveSetting();
        public LaunchpadSetting launchpadSetting = new LaunchpadSetting();
        public AudioSetting audioSetting = new AudioSetting();
        public Setting()
        {
            //默认路径
            launchpadSetting.DefaultPath.GamePath = @"D:\IVREAL_MR";
            launchpadSetting.DefaultPath.SettingPath = @"D:\IVREAL_MR\Setting";
            launchpadSetting.DefaultPathTest.GamePath = @"D:\IVREAL_MR_Test";
            launchpadSetting.DefaultPathTest.SettingPath = @"D:\IVREAL_MR_Test\Setting";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            try
            {
                reader.ReadStartElement("Setting");
                reader.ReadStartElement("XmlSerializable");

                SettingHelper.ReadElement(reader, "LiveSetting", ref liveSetting);
                SettingHelper.ReadElement(reader, "ProgramList", ref launchpadSetting);
                SettingHelper.ReadElement(reader, "AudioSetting", ref audioSetting);

                reader.ReadEndElement();
                reader.ReadEndElement();
            }
            catch (Exception ex)
            {
                LogHelper.LogMessageEx("read xml", ex);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("XmlSerializable");

            SettingHelper.WriteElement(writer, "LiveSetting", liveSetting);
            SettingHelper.WriteElement(writer, "ProgramList", launchpadSetting);
            SettingHelper.WriteElement(writer, "AudioSetting", audioSetting);

            writer.WriteEndElement();
        }
    }

    public class SettingManager : MSingleton<SettingManager>
    {
        public User UserInfo { get; set; } = null;

        public Setting Setting { get; set; } = null;

        private string path = "AppSetting.xml";// Application.dataPath + "/Resources/Xml/" + "Setting.xml";

        protected override void Init()
        {
            if (!File.Exists(path))
            {
                SettingHelper.SaveSetting(Setting, path);
            }
            Setting = SettingHelper.ReadSetting<Setting>(path);
        }

        public void SaveSetting()
        {
            SettingHelper.SaveSetting(Setting, path);
        }

        public PathSetting GetPathSetting()
        {
            if (UserInfo == null)
            {
                return Setting.launchpadSetting.DefaultPath;
               
            }
            return UserInfo.bTestEnv ? Setting.launchpadSetting.DefaultPathTest : Setting.launchpadSetting.DefaultPath;        }

    }
}
