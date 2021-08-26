using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinLiveManage
{
    public static class ToolHelper
    {
        public static bool FilterFile(string filePath, bool bSelf = false)
        {
            bool bDropout =
                filePath.EndsWith(".install") ||
                filePath.EndsWith(".update") ||
                filePath.EndsWith(".dmp") ||
                filePath.EndsWith("GameManifest.txt") ||
                filePath.EndsWith("GameManifest.checksum") ||
                filePath.EndsWith("Setting.xml") ||
                filePath.EndsWith("x360ce.ini") ||
                filePath.EndsWith("LiveWindowLog.txt");
            //检查是否需要包含PDB文件
            if (!bDropout)
            {
                bDropout = filePath.EndsWith(".pdb") || filePath.Contains("ZegoLog") || filePath.Contains("logs") || filePath.Contains("AgoraLog");
                //上传自身屏了这些
                if (!bDropout)
                {
                    if (bSelf)
                    {
                        bDropout = filePath.EndsWith(".iobj") ||
                          filePath.EndsWith(".bsc") ||
                          filePath.EndsWith(".ipdb") ||
                          filePath.EndsWith(".exp") ||
                          filePath.EndsWith(".cso") ||
                          filePath.Contains("yolov3-tiny") ||
                          filePath.Contains("cuda");
                    }
                    else
                    {
                        bDropout = filePath.Contains("SaveXml") ||
                                   filePath.Contains("CameraData") ||
                                   filePath.Contains("Logs") ||
                                   filePath.Contains("SaveGames") ||
                                   filePath.Contains("Crashes") ||
                                   filePath.Contains("CrashReportClient");
                    }
                }
            }
            return bDropout;
        }
    }
}
