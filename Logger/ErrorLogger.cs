using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace beeEmailing
{
    public class ErrorLogger
    {
        private StringBuilder oLogFilePath = new StringBuilder();
        private StringBuilder str = new StringBuilder();
        public void Log(object info)
        {
            try
            {

                str.Remove(0, str.Length);
                oLogFilePath.Remove(0, oLogFilePath.Length);
                oLogFilePath.Append(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Logs\\" + "Error_" + DateTime.Now.ToString("ddMMyyyy", new CultureInfo("en-IN")) + ".log");

                if (info is string)
                {
                    string ex = info as string;
                    str.Append(DateTime.UtcNow.ToString("ddMMyyyy", new CultureInfo("en-IN")) + " : Info : " + info);
                }
                else if (info is Exception)
                {
                    Exception ex = info as Exception;
                    str.Append(DateTime.UtcNow.ToString("ddMMyyyy", new CultureInfo("en-IN")) + " : Error : " + ex.Message + "-" + ex.StackTrace);
                }

                TextFileInfo.WriteorApendText(oLogFilePath.ToString(), str.ToString().Replace("\n", String.Empty).Replace("\r", String.Empty).Replace("\t", String.Empty));

            }
            catch
            { }
        }

    }
}
