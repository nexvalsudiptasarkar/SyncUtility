using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Nexval.NexAEI.Utility.WinService
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer timer;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.timer = new System.Timers.Timer(30000D);  // 30000 milliseconds = 30 seconds
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            this.timer.Enabled = true;
            this.timer.Start();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            var configInfo = new ConfigurationInfo();
            try
            {
                ProcessManager manager = new ProcessManager();
                var res = manager.ExportDataToNexAei(configInfo);

            }
            catch (Exception ex)
            {
                Utility.LogMessage(configInfo, ex.Message);
            }

            this.timer.Start();
            this.timer.Enabled = true;
            this.timer = new System.Timers.Timer(configInfo.ServiceInterval);  // 30000 milliseconds = 30 seconds
            this.timer.AutoReset = true;
        }
        protected override void OnStop()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
