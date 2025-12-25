using libplctag;
using System;

namespace PlcTagApi.Core.Models.Domain
{
    /// <summary>
    /// Domain model: Represents a PLC connection
    /// No external dependencies
    /// </summary>
    public class PLCConnectionConfig
    {
        public string PLCName { get; set; }
        public string Gateway { get; set; }
        public string Path { get; set; }
        public PlcType PlcType { get; set; } = PlcType.ControlLogix;
        public Protocol Protocol { get; set; } = Protocol.ab_eip;
        public int TimeoutSeconds { get; set; } = 5;
        public string Status { get; set; } = "DISCONNECTED";
        public string LastErrorMessage { get; set; }
        public DateTime? LastErrorTime { get; set; }
    }
}
