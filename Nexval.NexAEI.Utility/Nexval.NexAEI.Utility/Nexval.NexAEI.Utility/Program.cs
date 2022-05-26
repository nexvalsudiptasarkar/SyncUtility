using System;
using System.IO;
using System.ServiceProcess;

namespace Nexval.NexAEI.Utility
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            
            var configInfo = new ConfigurationInfo();
            try
            {

                ProcessManager manager = new ProcessManager();
                configInfo = manager.GetTrackerValues(configInfo);               
                var res = manager.ExportDataToNexAei(configInfo);
                DataAccess.UpdateTracker(configInfo);
            }
            catch (Exception ex)
            {

                Utility.LogMessage(configInfo, ex.Message, "Main");

            }


        }
    }
}
