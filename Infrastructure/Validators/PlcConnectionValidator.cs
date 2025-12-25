using PlcTagApi.Core.Interfaces;
using PlcTagApi.Exceptions;
using PlcTagApi.Infrastructure.Helpers;

namespace PlcTagApi.Infrastructure.Validators
{
    /// <summary>
    /// Implements connection validation
    /// Single Responsibility: Validation logic
    /// Dependency Inversion: Implements IPlcConnectionValidator
    /// </summary>
    public class PlcConnectionValidator : IPlcConnectionValidator
    {
        public bool IsValidIPAddress(string ip)
        {
            return NetworkHelper.IsValidIPAddress(ip);
        }

        public bool IsValidPath(string path)
        {
            return NetworkHelper.IsValidPath(path);
        }

        public bool IsValidTimeout(int timeoutSeconds)
        {
            return timeoutSeconds > 0;
        }

        public bool IsValidPlcName(string plcName)
        {
            return !string.IsNullOrWhiteSpace(plcName) && plcName.Length <= 100;
        }

        public void ValidateConnection(string plcName, string gateway, string path, int timeoutSeconds)
        {
            if (!IsValidPlcName(plcName))
                throw new InvalidConfigurationException("PLCName is invalid or too long");

            if (!IsValidIPAddress(gateway))
                throw new InvalidConfigurationException($"Gateway '{gateway}' is not a valid IP address");

            if (!IsValidPath(path))
                throw new InvalidConfigurationException($"Path '{path}' is invalid. Format: x,y");

            if (!IsValidTimeout(timeoutSeconds))
                throw new InvalidConfigurationException("TimeoutSeconds must be greater than 0");
        }
    }
}
