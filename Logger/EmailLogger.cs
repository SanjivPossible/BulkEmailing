using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace beeEmailing
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
        public DataTable ReadLog(DateTime? selecteddate)
        {
            DataTable dtLogs = new DataTable();
            dtLogs.Columns.Add("Send", typeof(DateTime));
            dtLogs.Columns.Add("UserId", typeof(string));
            dtLogs.Columns.Add("From", typeof(string));
            dtLogs.Columns.Add("To", typeof(string));
            dtLogs.Columns.Add("SendStatus", typeof(bool));
            dtLogs.Columns.Add("Subject", typeof(string));
            string fileName = string.Empty;

            try
            {

                DateTime logDate = selecteddate != null ? selecteddate.Value : DateTime.Now;
                fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Logs\\" + "Email_" + logDate.ToString("ddMMyyyy", new CultureInfo("en-IN")) + ".log";

                if (File.Exists(fileName))
                {

                    string[] strLines = File.ReadAllLines(fileName);
                    foreach (string strLine in strLines)
                    {
                        string[] lineschema = strLine.Split(';');
                        if (lineschema.Length > 0)
                        {
                            dtLogs.Rows.Add(new object[] { DateTime.ParseExact(lineschema[0], "ddMMyyyy HHmmssffff", System.Globalization.CultureInfo.InvariantCulture), lineschema[1], lineschema[2], lineschema[3], lineschema[4], lineschema[5] });
                        }
                    }
                }
            }
            catch
            {
                
            }

            return dtLogs;
        }

    }
}
