namespace PlcTagApi.Core.Models.Requests
{
    public class AddPLCConnectionRequest
    {
        public string PLCName { get; set; }
        public string Gateway { get; set; }
        public string Path { get; set; } = "1,0";
        public int TimeoutSeconds { get; set; } = 5;
    }
}
