using System;

namespace PlcTagApi.Models
{
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
}
