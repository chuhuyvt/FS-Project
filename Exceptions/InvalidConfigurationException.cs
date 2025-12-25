namespace PlcTagApi.Exceptions
{
    public class InvalidConfigurationException : PlcException
    {
        public InvalidConfigurationException(string message) 
            : base(message, "INVALID_CONFIGURATION") { }
    }
}
