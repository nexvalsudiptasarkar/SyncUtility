using System;
using System.Configuration;
using System.IO;

namespace Nexval.NexAEI.Utility
{
    public class ConfigurationInfo
    {
        public string ConnectionString { get; set; }
        public string ScriptFilePath { get; set; }
        public string SqlScript { get; set; }
        public string OrganizationId { get; set; }
        public string NexAEIApiUrl { get; set; }
        public string LogFilePath { get; set; }
        public int ServiceInterval { get; set; }
        public ConfigurationInfo()
        {
            ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
            OrganizationId = ConfigurationManager.AppSettings["OrganizationId"];
            NexAEIApiUrl = ConfigurationManager.AppSettings["NexAEIApiUrl"];
            int serviceInterval = 30000;
            int.TryParse(ConfigurationManager.AppSettings["ServiceInterval"], out serviceInterval);
            ServiceInterval = serviceInterval<=0? 30000: serviceInterval;
            ScriptFilePath = GetScriptFilePath();
            SqlScript = GetSqlScripts();
            LogFilePath = GetLogFilePath();
        }
        private string GetScriptFilePath()
        {
            string assemblyFileName = Uri.UnescapeDataString((new UriBuilder((new ProcessManager()).GetType().Assembly.CodeBase)).Path);
            string sqlFile = assemblyFileName + ".sql";

            return sqlFile;
        }

        private string GetLogFilePath()
        {
            string assemblyFileName = Uri.UnescapeDataString((new UriBuilder((new ProcessManager()).GetType().Assembly.CodeBase)).Path);
            string sqlFile = assemblyFileName + ".log";

            return sqlFile;
        }

        private string GetSqlScripts()
        {
            string assemblyFileName = Uri.UnescapeDataString((new UriBuilder((new ProcessManager()).GetType().Assembly.CodeBase)).Path);
            string sqlFile = assemblyFileName + ".sql";
            string script = string.Empty;
            if (File.Exists(sqlFile))
            {
                script = File.ReadAllText(sqlFile);
            }
            return script;
        }
        /*
          // System.Configuration.ExeConfigurationFileMap configFileMap = new System.Configuration.ExeConfigurationFileMap();
           // configFileMap.ExeConfigFilename = assemblyFileName + ".config"; ;
           // System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
           // ConnectionStringsSection section = (ConnectionStringsSection)configuration.GetSection("connectionStrings");
           //string sconnstring = section.ConnectionStrings["UserDBConnectionString"].ConnectionString;

        */
    }
}
