using System.IO;

namespace beeEmailing
{
    public static class TextFileInfo
    {
        public static string ReadFile(string fileName)
        {
            string str = string.Empty;
            using (StreamReader sr = File.OpenText(fileName))
            {
                str = sr.ReadToEnd();
            }
            return str;
        }

        public static void WriteText(string fileName, string info)
        {
            try
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine(info);
                }
            }
            catch
            { }
        }

        public static void WriteorApendText(string fileName, string info)
        {
            try
            {
                if (!File.Exists(fileName))
                {

                    using (StreamWriter sw = File.CreateText(fileName))
                    {
                        sw.WriteLine(info);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(fileName))
                    {
                        sw.WriteLine(info);
                    }
                }
            }
            catch
            { }
        }
    }

}
