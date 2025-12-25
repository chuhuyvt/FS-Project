using PlcTagApi.Core.Models.Domain;
using PlcTagApi.Core.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlcTagApi.Core.Interfaces
{
    /// <summary>
    /// Manages PLC connection lifecycle
    /// Segregated Interface: Connection management only
    /// </summary>
    public interface IPlcConnectionPool
    {
        Task<bool> AddConnectionAsync(PLCConnectionConfig config);
        Task<bool> UpdateConnectionAsync(string plcName, string gateway, string path, int timeoutSeconds);
        Task<bool> RemoveConnectionAsync(string plcName);
        Task<bool> TestConnectionAsync(string plcName);
        Task<PLCConnectionStatus> GetConnectionStatusAsync(string plcName);
        Task<List<PLCConnectionStatus>> GetAllConnectionStatusAsync();
        libplctag.Tag CreateTag(string plcName, string tagName);
        bool IsConnectionActive(string plcName);
    }
}
