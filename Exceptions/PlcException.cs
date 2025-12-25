using System;

namespace PlcTagApi.Exceptions
{
    /// <summary>
    /// Base exception for PLC operations
    /// </summary>
    public class PlcException : Exception
    {
        public string ErrorCode { get; set; }

        public PlcException(string message, string errorCode = null) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
