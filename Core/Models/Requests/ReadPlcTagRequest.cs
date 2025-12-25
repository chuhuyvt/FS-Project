namespace PlcTagApi.Core.Models.Requests
{
    public class ReadTagRequest
    {
        public string PLCName { get; set; }
        public string TagName { get; set; }
        public string TagType { get; set; }
        public int ArraySize { get; set; } = 0;
    }
}
