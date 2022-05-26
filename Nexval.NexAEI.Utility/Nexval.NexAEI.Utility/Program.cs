using System;
using System.IO;
using System.ServiceProcess;

namespace Nexval.NexAEI.Utility
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var configInfo = new ConfigurationInfo();
            //try
            //{
            //    Utility.LogMessage(configInfo,$"Started at {DateTime.Now.ToString()}");
            //    ProcessManager manager = new ProcessManager();
            //    var res = manager.ExportDataToAei(configInfo);
            //    Console.Write(res.Result);
            //    Console.ReadLine();
            //    Utility.LogMessage(configInfo, $"Completed at {DateTime.Now.ToString()}");
            //}
            //catch (Exception ex)
            //{

            //    Utility.LogMessage(configInfo, ex.Message);
            //    Console.Write(ex.Message);
            //    Console.ReadLine();
            //}
#if !DEBUG
   
            ServiceBase[] serviceToRun;
            serviceToRun = new ServiceBase[]
            {
                new NexAEIExportUtilityService()

            };
            ServiceBase.Run(serviceToRun);
#else

            var configInfo = new ConfigurationInfo();
            try
            {
                //System.Timers.Timer timer = new System.Timers.Timer();
                //timer.Start();
                //timer.Enabled = true;
                //timer = new System.Timers.Timer(configInfo.ServiceInterval);  // 30000 milliseconds = 30 seconds
                //timer.AutoReset = true;
                //  Utility.LogMessage(configInfo, $"Started at {DateTime.Now.ToString()}");
                ProcessManager manager = new ProcessManager();
                var res = manager.ExportDataToAei(configInfo);
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

#endif
        }
    }
}
