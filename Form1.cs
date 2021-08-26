using AocePackage;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using static WinLiveManage.NewLoginfo;

namespace WinLiveManage
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);
        const uint WM_APPCOMMAND = 0x319;
        const uint APPCOMMAND_VOLUME_UP = 0x0a; const uint APPCOMMAND_VOLUME_DOWN = 0x09;
        DispatcherTimer timer;
        public  Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Volume_control();
            timeo();
        }
        public void Volume_control()
        {
            CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
            Debug.WriteLine("Current Volume:" + defaultPlaybackDevice.Volume);
            SendMessage(this.Handle, WM_APPCOMMAND, 0x30292, APPCOMMAND_VOLUME_UP * 0x10000);
            defaultPlaybackDevice.Volume = 80;
        }
        public void timeo()
        {
            int Hour = DateTime.Now.Hour;
            int Minute = DateTime.Now.Minute;
            int Second = DateTime.Now.Second;
            string ms = Hour + "" + Minute + "" + Second.ToString();
            //textBox1.Text = ms;
            if (ms!=""||ms!=null)
            {
                WinLiveManager w = new WinLiveManager();
               /* w.OAllUpdate();*/
            }
            else {
                Ts.Text = "时间W到";
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
          
        }
        //1秒执行一次
        private void Window_Loaded()
        {
            timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer1_Tick;
            timer.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //定时执行的内容
            timeo();
        }
       

    }
}
