using System;
using System.Net;

namespace PlcTagApi.Infrastructure.Helpers
{
    /// <summary>
    /// Network validation utilities
    /// Single Responsibility: Network validation only
    /// </summary>
    public static class NetworkHelper
    {
        public static bool IsValidIPAddress(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }

        public static bool IsValidPath(string path)
{
    if (string.IsNullOrWhiteSpace(path))
        return false;

    var parts = path.Split(',');
    if (parts.Length != 2)
        return false;

    // Trim each string element explicitly
    string part0 = parts[0].Trim();      // ← WITH [0] index
    string part1 = parts[1].Trim();      // ← WITH [1] index
    return int.TryParse(part0, out _) && int.TryParse(part1, out _);
}
    }
}
