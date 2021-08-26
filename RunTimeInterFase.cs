using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace WinLiveManage
{
  public interface RunTimeInterFase
    {
        object Runtim_();
        public class Runtime : RunTimeInterFase {
            object RunTimeInterFase.Runtim_() {
                string currentDirs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string RunTime = Path.Combine(currentDirs, "RunTimeHJ.exe");
                object? obj="";
                if (File.Exists(RunTime)) { 
                    obj= System.Diagnostics.Process.Start(RunTime);
                }
                return obj;
            }
        }
    }
}
