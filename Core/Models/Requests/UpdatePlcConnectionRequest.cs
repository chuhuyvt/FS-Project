namespace PlcTagApi.Core.Models.Requests
{
    public class UpdatePLCConnectionRequest
    {
        public string Gateway { get; set; }
        public string Path { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}
