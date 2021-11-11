using System;

namespace beeEmailing
{
    public class mEmailLog
    {
        public string LoggedUser { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public bool IsSend { get; set; }
        public DateTime SendTime { get; set; }

    }
}
