// Models/TagValue.cs
using System;
using libplctag;

namespace PlcTagApi.Models
{
    // Existing models
    public class TagValue
    {
        public string TagName { get; set; }
        public string TagType { get; set; } // BOOL, DINT, REAL, STRING, ARRAY
        public object CurrentValue { get; set; }
        public object PreviousValue { get; set; }
        public DateTime LastChanged { get; set; }
        public string Status { get; set; } // OK, ERROR
        public string ErrorMessage { get; set; }
    }

    public class TagReadRequest
    {
        public string TagName { get; set; }
        public string TagType { get; set; }
        public int ArraySize { get; set; } = 0;
    }

    public class TagResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    // ====== PLC CONNECTION MODELS ======

    public class PLCConnectionConfig
    {
        public string PLCName { get; set; }
        public string Gateway { get; set; } // IP Address
        public string Path { get; set; } // e.g., "1,0"
        public PlcType PlcType { get; set; } = PlcType.ControlLogix;
        public Protocol Protocol { get; set; } = Protocol.ab_eip;
        public int TimeoutSeconds { get; set; } = 5;
        public string Status { get; set; } = "DISCONNECTED"; // DISCONNECTED, CONNECTED, TESTING, ERROR
        public string LastErrorMessage { get; set; }
        public DateTime? LastErrorTime { get; set; }
    }

    public class PLCConnectionStatus
    {
        public string PLCName { get; set; }
        public string Gateway { get; set; }
        public string Path { get; set; }
        public string Status { get; set; } // CONNECTED, DISCONNECTED, TESTING, ERROR
        public DateTime? LastSuccessfulConnection { get; set; }
        public string LastErrorMessage { get; set; }
        public DateTime? LastErrorTime { get; set; }
        public int TimeoutSeconds { get; set; }
    }

    public class AddPLCConnectionRequest
    {
        public string PLCName { get; set; }
        public string Gateway { get; set; }
        public string Path { get; set; } = "1,0";
        public int TimeoutSeconds { get; set; } = 5;
    }
    public class UpdatePLCConnectionRequest
    {
        public string Gateway { get; set; }
        public string Path { get; set; }
        public int TimeoutSeconds { get; set; }
    }

    public class ReadTagWithPLCRequest
    {
        public string PLCName { get; set; }
        public string TagName { get; set; }
        public string TagType { get; set; }
        public int ArraySize { get; set; } = 0;
    }

    public class MonitorTagsRequest
    {
        public string PLCName { get; set; }
        public string[] TagNames { get; set; }
        public string[] TagTypes { get; set; }
        public int[] ArraySizes { get; set; }
    }

    public class TagCondition
    {
        public string TagName { get; set; }
        public string ConditionType { get; set; } // GreaterThan, LessThan, Equal, Between
        public object ThresholdValue { get; set; }
        public object MinValue { get; set; } // For Between condition
        public object MaxValue { get; set; } // For Between condition
    }

    public class MonitorTagsWithConditionRequest
    {
        public string PLCName { get; set; }
        public TagCondition[] Conditions { get; set; }
    }
}
