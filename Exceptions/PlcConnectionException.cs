namespace PlcTagApi.Exceptions
{
    public class PlcConnectionException : PlcException
    {
        public PlcConnectionException(string message) 
            : base(message, "PLC_CONNECTION_ERROR") { }
    }
}