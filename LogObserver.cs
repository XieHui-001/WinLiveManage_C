using OeipCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OeipCommon.FileTransfer.All_LiveInfo;

namespace AocePackage
{
    class LogObserver : ILogObserver
    {
        public override void onLogEvent(int level, string message)
        {
            LogHelper.LogMessage(level + "::::" + message);
            Console.WriteLine(message);
        }
    }
}
