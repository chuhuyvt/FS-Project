using System;

namespace PlcTagApi.Core.Models.Responses
{
    public class TagValue
    {
        public string TagName { get; set; }
        public string TagType { get; set; }
        public object CurrentValue { get; set; }
        public object PreviousValue { get; set; }
        public DateTime LastChanged { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
