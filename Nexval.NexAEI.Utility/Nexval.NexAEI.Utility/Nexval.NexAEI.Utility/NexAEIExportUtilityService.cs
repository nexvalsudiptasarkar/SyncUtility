using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Nexval.NexAEI.Utility
{
    partial class NexAEIExportUtilityService : ServiceBase
    {
        private System.Timers.Timer timer;

        public NexAEIExportUtilityService()
        {
            InitializeComponent();
            this.ServiceName = "NexAEI.Utility.Service";

        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
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
                //Utility.LogMessage(configInfo, $"Started at {DateTime.Now.ToString()}");
                ProcessManager manager = new ProcessManager();               
                var res = manager.ExportDataToNexAei(configInfo);
                //Console.Write(res.Result);
                //Console.ReadLine();
                // Utility.LogMessage(configInfo, $"Completed at {DateTime.Now.ToString()}");
            }
            catch (Exception ex)
            {

                Utility.LogMessage(configInfo, ex.Message);
                //Console.Write(ex.Message);
                //Console.ReadLine();
            }
           
            this.timer.Start();
            this.timer.Enabled = true;
            this.timer = new System.Timers.Timer(configInfo.ServiceInterval);  // 30000 milliseconds = 30 seconds
            this.timer.AutoReset = true;
        }


        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service. _oTimer.Stop();  // no harm in calling it
            timer.Stop();
            timer.Dispose();

        }
    }
}
