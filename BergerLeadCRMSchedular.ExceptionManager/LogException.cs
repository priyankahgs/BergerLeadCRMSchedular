using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace BergerLeadCRMSchedular.ExceptionManager
{
    public class LogException
    {

        public static void Log(string message)
        {
            string rootLocation = ConfigurationManager.AppSettings["ExceptionLog.RootLocation"];

            if (string.IsNullOrWhiteSpace(rootLocation))
            {
                return;
            }

            string fileName = DateTime.Now.ToString("ddMMyyyy");
            string filePath = string.Format("{0}/{1}.txt", rootLocation, fileName);

            string CurrentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string physicalPath = string.Format("{0}{1}", CurrentDomainBaseDirectory, filePath);

            string physicalRootLocation = string.Format("{0}{1}", CurrentDomainBaseDirectory, rootLocation);
            if (!Directory.Exists(physicalRootLocation))
            {
                Directory.CreateDirectory(physicalRootLocation);
            }

            File.AppendAllText(physicalPath, DecorateMessage(message));
        }


        private static string DecorateMessage(string message)
        {
            StringBuilder msg = new StringBuilder();
            string divider = new String('-', 20);

            msg.AppendFormat("Logged Date: {0}", DateTime.Now);
            msg.Append(Environment.NewLine);
            msg.Append(divider);
            msg.Append(Environment.NewLine);
            msg.Append(message);
            msg.Append(Environment.NewLine);
            msg.Append(divider);
            msg.Append(Environment.NewLine);
            msg.Append(divider);
            msg.Append(Environment.NewLine);
            msg.Append(Environment.NewLine);

            return msg.ToString();
        }
    }
}
