using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace bEmailing
{
    public class EmailLogger
    {
        private StringBuilder oLogFilePath = new StringBuilder();
        private StringBuilder str = new StringBuilder();
        public void Log(mEmailLog info)
        {
            try
            {

                str.Remove(0, str.Length);
                oLogFilePath.Remove(0, oLogFilePath.Length);
                oLogFilePath.Append(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Logs\\" + "Email_" + DateTime.Now.ToString("ddMMyyyy", new CultureInfo("en-IN")) + ".log");
                str.Append(info.SendTime.ToString("ddMMyyyy HHmmssffff") + ";" + info.LoggedUser + ";" + info.From + ";" + info.To + ";" + info.IsSend.ToString() + ";" + info.Subject);
                TextFileInfo.WriteorApendText(oLogFilePath.ToString(), str.ToString().Replace("\n", String.Empty).Replace("\r", String.Empty).Replace("\t", String.Empty));

            }
            catch
            { }
        }

    }
}
