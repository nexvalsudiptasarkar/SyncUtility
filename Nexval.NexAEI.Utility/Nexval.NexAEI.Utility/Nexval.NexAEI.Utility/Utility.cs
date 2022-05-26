using System;
using System.IO;

namespace Nexval.NexAEI.Utility
{
    public static class Utility
    {
        public static void LogMessage(ConfigurationInfo configInfo, string logMessage,string methodNme)
        {
            try
            {
                DataAccess.ClearLog(configInfo);
                DataAccess.InsertLog(configInfo, methodNme, logMessage);
                LogMessage(configInfo, logMessage);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                LogMessage(configInfo, ex.Message);
                Console.ReadLine();
            }


        }
        public static void LogMessage(ConfigurationInfo configInfo, string logMessage)
        {
            try
            {
                PrepareLogFile(configInfo.LogFilePath, 1000000, 10);
                if (File.Exists(configInfo.LogFilePath))
                {
                    using (StreamWriter w = File.AppendText(configInfo.LogFilePath))
                    {
                        Log(logMessage, w);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Console.ReadLine();
            }


        }
        private static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }

        // If the file exceeds max_size bytes, move it to a new file
        // with .1 appended to the name and bump down older versions.
        // (E.g. log.txt.1, log.txt.2, etc.)
        // Then write the text into the main log file. 
        private static void PrepareLogFile(string file_name,long max_size, int num_backups)
        {
            try
            {
                // See if the file is too big.
                FileInfo file_info = new FileInfo(file_name);
                if (file_info.Exists && file_info.Length > max_size)
                {
                    // Remove the oldest version if it exists.
                    if (File.Exists(file_name + "." + num_backups.ToString()))
                    {
                        File.Delete(file_name + "." + num_backups.ToString());
                    }

                    // Bump down earlier backups.
                    for (int i = num_backups - 1; i > 0; i--)
                    {
                        if (File.Exists(file_name + "." + i.ToString()))
                        {
                            // Move file i to file i + 1.
                            File.Move(file_name + "." + i.ToString(), file_name + "." + (i + 1).ToString());
                        }
                    }

                    // Move the main log file.
                    File.Move(file_name, file_name + ".1");
                }

                // Write the text.
                //File.AppendAllText(file_name, new_text + '\n');
            }
            catch(Exception ex)
            {
                //To Do
            }
        }

        /// <summary>
        /// ConvertFromUnixTimestamp
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }
        /// <summary>
        /// ConvertToUnixTimestamp
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
        /// <summary>
        /// ConvertToUnixTime
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(datetime - sTime).TotalSeconds;
        }
    }
}
