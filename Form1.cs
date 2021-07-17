using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using System.Management;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        //PerformanceCounter theRAMCounter = new PerformanceCounter("Memory", "Available MBytes");
        int theRAMCounter;
        Computer thisComputer;
        string temp;

        public Form1()
        {
            InitializeComponent();
            theRAMCounter = DisplayTotalRam();
            cpuubox.Text = string.Format("{0} %", trackBar1.Value.ToString());
            timebox.Text = "1";
            rambox.Text = theRAMCounter.ToString();
            thisComputer = new Computer()
            {
                CPUEnabled = true,
                RAMEnabled = true,
                HDDEnabled = false,
                GPUEnabled = false,
                MainboardEnabled = false,
                FanControllerEnabled = false
            };
            thisComputer.Open();
            timer1.Start();
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }

        private int DisplayTotalRam()
        {
            string Query = "SELECT MaxCapacity FROM Win32_PhysicalMemoryArray";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
            int SizeinKB;
            int SizeinMB = 0;
            foreach (ManagementObject WniPART in searcher.Get())
            {
                SizeinKB = Convert.ToInt32(WniPART.Properties["MaxCapacity"].Value);
                SizeinMB = SizeinKB / 2048;
                //UInt32 SizeinGB = SizeinMB / 1024;
                //Console.WriteLine("Size in KB: {0}, Size in MB: {1}, Size in GB: {2}", SizeinKB, SizeinMB, SizeinGB);
                
            }
            return SizeinMB;
        }


        private void GetSystemInfo()
        {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.Accept(updateVisitor);
            textBox1.Text = "";
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    textBox1.Text = computer.Hardware[i].Name;
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        string sent = "";
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            textBox1.Text += Environment.NewLine;
                            sent = string.Format(computer.Hardware[i].Sensors[j].Name + ": {0:F2}  \u00B0C", (float)computer.Hardware[i].Sensors[j].Value);
                            //Console.WriteLine(computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                            textBox1.Text += sent;

                        }
                        //textBox1.AppendText(sent);
                        //textBox1.AppendText(Environment.NewLine);
                    }
                }
            }
            computer.Close();
        }

        private void Thread1()
        {
            string argss = " ";
            ScriptRunner.RunFromCmd(@"monitor.py", argss);

        }
        private void Thread2(string args)
        {
            
            ScriptRunner.RunFromCmd(@"stres.py", args);

        }
        private void button1_Click(object sender, EventArgs e)
        {

            //string ss = string.Format("{0} {1}", trackBar1.Value, rambox.Text);
            //string args = string.Format("{0} {1} {2}", trackBar1.Value, timebox.Text, rambox.Text);
            //ScriptRunner.RunFromCmd(@"stres.py", args);

            long timeinter = Convert.ToInt32(timebox.Text);
            string args = string.Format("{0} {1} {2}", trackBar1.Value, timeinter*60, rambox.Text);
            //Thread t2 = new Thread(new ParameterizedThreadStart(Thread2));
            Thread thread = new Thread(() => Thread2(args));
            thread.Start();


            //Thread t1 = new Thread(new ThreadStart(Thread1));
            //t1.Start();

            //ScriptRunner.RunFromCmd(@"syss.py", ss);

            

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            cpuubox.Text = string.Format("{0} %", trackBar1.Value.ToString());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GetSystemInfo();
            //textBox1.AppendText("hello");
            //textBox1.AppendText(Environment.NewLine);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }


    }
    public class ScriptRunner
    {
        //args separated by spaces
        public static string RunFromCmd(string rCodeFilePath, string args)
        {
            string file = rCodeFilePath;
            string result = string.Empty;

            try
            {

                var info = new ProcessStartInfo(System.IO.Path.GetDirectoryName(Application.ExecutablePath)+@"\Python\Python37\python.exe");
                //var info = new ProcessStartInfo(@"C:\Users\savef\AppData\Local\Programs\Python\Python37\python.exe");
                info.Arguments = rCodeFilePath + " " + args;

                info.RedirectStandardInput = false;//false
                info.RedirectStandardOutput = false;//true
                info.UseShellExecute = false;//false
                info.CreateNoWindow = true;//true

                using (var proc = new Process())
                {
                    proc.StartInfo = info;
                    proc.Start();
                    proc.WaitForExit();
                    if (proc.ExitCode == 0)
                    {
                        result = proc.StandardOutput.ReadToEnd();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("R Script failed: " + result, ex);
            }
        }
    }
}
