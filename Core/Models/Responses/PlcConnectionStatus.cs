using System;

namespace PlcTagApi.Core.Models.Responses
{
    public class PLCConnectionStatus
    {
        public string PLCName { get; set; }
        public string Gateway { get; set; }
        public string Path { get; set; }
        public string Status { get; set; }
        public DateTime? LastSuccessfulConnection { get; set; }
        public string LastErrorMessage { get; set; }
        public DateTime? LastErrorTime { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}
