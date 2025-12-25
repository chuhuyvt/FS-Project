using PlcTagApi.Core.Models.Domain;

namespace PlcTagApi.Core.Models.Requests
{
    public class MonitorTagsRequest
    {
        public string PLCName { get; set; }
        public TagCondition[] Conditions { get; set; }
        public string[] TagNames { get; set; }      // ← THÊM
        public string[] TagTypes { get; set; }      // ← THÊM
        public int[] ArraySizes { get; set; }       // ← THÊM
    }
}