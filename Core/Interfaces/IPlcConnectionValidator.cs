namespace PlcTagApi.Core.Interfaces
{
    /// <summary>
    /// Validates PLC connection configurations
    /// Single Responsibility: Only validation logic
    /// </summary>
    public interface IPlcConnectionValidator
    {
        /// <summary>
        /// Validates IP address format
        /// </summary>
        bool IsValidIPAddress(string ip);

        /// <summary>
        /// Validates path format (e.g., "1,0")
        /// </summary>
        bool IsValidPath(string path);

        /// <summary>
        /// Validates timeout value
        /// </summary>
        bool IsValidTimeout(int timeoutSeconds);

        /// <summary>
        /// Validates PLC name
        /// </summary>
        bool IsValidPlcName(string plcName);

        /// <summary>
        /// Throws exception if connection config is invalid
        /// </summary>
        void ValidateConnection(string plcName, string gateway, string path, int timeoutSeconds);
    }
}