using libplctag;
using PlcTagApi.Core.Interfaces;
using PlcTagApi.Core.Models.Domain;
using PlcTagApi.Core.Models.Responses;
using PlcTagApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PlcTagApi.Infrastructure.Implementations
{
    /// <summary>
    /// Manages PLC connection pool
    /// Single Responsibility: Connection lifecycle management
    /// Dependency Inversion: Depends on IPlcConnectionValidator
    /// </summary>
    public class PLCConnectionPool : IPlcConnectionPool
    {
        private readonly Dictionary<string, PLCConnectionConfig> _connections = new();
        private readonly Dictionary<string, DateTime> _lastSuccessfulConnection = new();
        private readonly IPlcConnectionValidator _validator;
        private readonly object _lockObject = new object();

        public PLCConnectionPool(IPlcConnectionValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            InitializeDefaultConnection();
        }

        private void InitializeDefaultConnection()
        {
            lock (_lockObject)
            {
                _connections["PLC-Default"] = new PLCConnectionConfig
                {
                    PLCName = "PLC-Default",
                    Gateway = "10.44.189.226",
                    Path = "1,0",
                    PlcType = PlcType.ControlLogix,
                    Protocol = Protocol.ab_eip,
                    TimeoutSeconds = 5,
                    Status = "IDLE"
                };
            }
        }

        public async Task<bool> AddConnectionAsync(PLCConnectionConfig config)
        {
            try
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                _validator.ValidateConnection(config.PLCName, config.Gateway, config.Path, config.TimeoutSeconds);

                lock (_lockObject)
                {
                    if (_connections.ContainsKey(config.PLCName))
                        throw new PlcConnectionException($"PLC '{config.PLCName}' already exists");

                    config.Status = "DISCONNECTED";
                    _connections[config.PLCName] = config;
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new PlcConnectionException($"Error adding connection: {ex.Message}");
            }
        }

        public async Task<bool> UpdateConnectionAsync(string plcName, string gateway, string path, int timeoutSeconds)
        {
            try
            {
                _validator.ValidateConnection(plcName, gateway, path, timeoutSeconds);

                lock (_lockObject)
                {
                    if (!_connections.ContainsKey(plcName))
                        throw new PlcConnectionException($"PLC '{plcName}' not found");

                    var config = _connections[plcName];
                    config.Gateway = gateway;
                    config.Path = path;
                    config.TimeoutSeconds = timeoutSeconds;
                    config.Status = "DISCONNECTED";
                    config.LastErrorMessage = null;
                    config.LastErrorTime = null;
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new PlcConnectionException($"Error updating connection: {ex.Message}");
            }
        }

        public async Task<bool> RemoveConnectionAsync(string plcName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plcName))
                    throw new ArgumentException("PLCName cannot be empty");

                lock (_lockObject)
                {
                    if (!_connections.ContainsKey(plcName))
                        throw new PlcConnectionException($"PLC '{plcName}' not found");

                    if (plcName == "PLC-Default")
                        throw new InvalidOperationException("Cannot remove default PLC");

                    _connections.Remove(plcName);
                    _lastSuccessfulConnection.Remove(plcName);
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new PlcConnectionException($"Error removing connection: {ex.Message}");
            }
        }

        public async Task<bool> TestConnectionAsync(string plcName)
        {
            Tag tag = null;
            try
            {
                lock (_lockObject)
                {
                    if (!_connections.ContainsKey(plcName))
                        throw new PlcConnectionException($"PLC '{plcName}' not found");

                    _connections[plcName].Status = "TESTING";
                }

                tag = CreateTag(plcName, "x");
                tag.Initialize();
                tag.Read();

                lock (_lockObject)
                {
                    _connections[plcName].Status = "CONNECTED";
                    _lastSuccessfulConnection[plcName] = DateTime.Now;
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                lock (_lockObject)
                {
                    if (_connections.ContainsKey(plcName))
                    {
                        _connections[plcName].Status = "DISCONNECTED";
                        _connections[plcName].LastErrorMessage = ex.Message;
                        _connections[plcName].LastErrorTime = DateTime.Now;
                    }
                }

                return await Task.FromResult(false);
            }
            finally
            {
                tag?.Dispose();
            }
        }

        public async Task<PLCConnectionStatus> GetConnectionStatusAsync(string plcName)
        {
            lock (_lockObject)
            {
                if (!_connections.TryGetValue(plcName, out PLCConnectionConfig? value))
                    throw new PlcConnectionException($"PLC '{plcName}' not found");

                return Task.FromResult(MapToStatus(value)).Result;
            }
        }

        public async Task<List<PLCConnectionStatus>> GetAllConnectionStatusAsync()
        {
            lock (_lockObject)
            {
                var statuses = _connections.Values
                    .Select(MapToStatus)
                    .ToList();

                return Task.FromResult(statuses).Result;
            }
        }

        public Tag CreateTag(string plcName, string tagName)
        {
            lock (_lockObject)
            {
                if (!_connections.ContainsKey(plcName))
                    throw new PlcConnectionException($"PLC '{plcName}' not found");

                var config = _connections[plcName];
                return new Tag()
                {
                    Name = tagName,
                    Gateway = config.Gateway,
                    Path = config.Path,
                    PlcType = config.PlcType,
                    Protocol = config.Protocol,
                    Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
                };
            }
        }

        public bool IsConnectionActive(string plcName)
        {
            lock (_lockObject)
            {
                if (!_connections.ContainsKey(plcName))
                    return false;

                return _connections[plcName].Status == "CONNECTED";
            }
        }

        private PLCConnectionStatus MapToStatus(PLCConnectionConfig config)
        {
            return new PLCConnectionStatus
            {
                PLCName = config.PLCName,
                Gateway = config.Gateway,
                Path = config.Path,
                Status = config.Status,
                LastSuccessfulConnection = _lastSuccessfulConnection.ContainsKey(config.PLCName)
                    ? _lastSuccessfulConnection[config.PLCName]
                    : (DateTime?)null,
                LastErrorMessage = config.LastErrorMessage,
                LastErrorTime = config.LastErrorTime,
                TimeoutSeconds = config.TimeoutSeconds
            };
        }
    }
}
