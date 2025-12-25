// Services/PLCConnectionPool.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using libplctag;
using PlcTagApi.Models;

namespace PlcTagApi.Services
{
    public interface IPLCConnectionPool
    {
        Task<bool> AddConnection(PLCConnectionConfig config);
        Task<bool> UpdateConnection(string plcName, UpdatePLCConnectionRequest request);
        Task<bool> RemoveConnection(string plcName);
        Task<bool> TestConnection(string plcName);
        Task<List<PLCConnectionStatus>> GetAllConnectionStatus();
        Task<PLCConnectionStatus> GetConnectionStatus(string plcName);
        Tag CreateTag(string plcName, string tagName);
        bool IsConnectionActive(string plcName);
    }

    public class PLCConnectionPool : IPLCConnectionPool
    {
        private readonly Dictionary<string, PLCConnectionConfig> _connections = new();
        private readonly Dictionary<string, DateTime> _lastSuccessfulConnection = new();
        private readonly object _lockObject = new object();

        public PLCConnectionPool()
        {
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

        public Task<bool> AddConnection(PLCConnectionConfig config)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(config.PLCName))
                    throw new ArgumentException("PLCName không được để trống");

                if (string.IsNullOrWhiteSpace(config.Gateway))
                    throw new ArgumentException("Gateway (IP) không được để trống");

                lock (_lockObject)
                {
                    if (_connections.ContainsKey(config.PLCName))
                        throw new InvalidOperationException($"PLC '{config.PLCName}' đã tồn tại");

                    config.Status = "DISCONNECTED";
                    _connections[config.PLCName] = config;
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thêm kết nối: {ex.Message}");
            }
        }

        public Task<bool> UpdateConnection(string plcName, UpdatePLCConnectionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plcName))
                    throw new ArgumentException("PLCName không được để trống");

                lock (_lockObject)
                {
                    if (!_connections.ContainsKey(plcName))
                        throw new KeyNotFoundException($"PLC '{plcName}' không tồn tại");

                    var current = _connections[plcName];

                    // Update từng field nếu có giá trị
                    if (!string.IsNullOrWhiteSpace(request.Gateway))
                    {
                        if (!IsValidIPAddress(request.Gateway))
                            throw new ArgumentException($"Gateway '{request.Gateway}' không phải IP address hợp lệ");
                        current.Gateway = request.Gateway;
                    }

                    if (!string.IsNullOrWhiteSpace(request.Path))
                    {
                        if (!IsValidPath(request.Path))
                            throw new ArgumentException($"Path '{request.Path}' không hợp lệ (format: x,y)");
                        current.Path = request.Path;
                    }

                    if (request.TimeoutSeconds > 0)
                        current.TimeoutSeconds = request.TimeoutSeconds;

                    // Reset status khi update
                    current.Status = "DISCONNECTED";
                    current.LastErrorMessage = null;
                    current.LastErrorTime = null;
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi update kết nối: {ex.Message}");
            }
        }

        public Task<bool> RemoveConnection(string plcName)
        {
            try
            {
                lock (_lockObject)
                {
                    if (!_connections.ContainsKey(plcName))
                        throw new KeyNotFoundException($"PLC '{plcName}' không tồn tại");

                    if (plcName == "PLC-Default")
                        throw new InvalidOperationException("Không thể xóa PLC mặc định");

                    _connections.Remove(plcName);
                    _lastSuccessfulConnection.Remove(plcName);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xóa kết nối: {ex.Message}");
            }
        }

        public Task<bool> TestConnection(string plcName)
        {
            Tag tag = null;
            try
            {
                lock (_lockObject)
                {
                    if (!_connections.ContainsKey(plcName))
                        throw new KeyNotFoundException($"PLC '{plcName}' không tồn tại");

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

                return Task.FromResult(true);
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

                return Task.FromResult(false);
            }
            finally
            {
                tag?.Dispose();
            }
        }

        public Task<List<PLCConnectionStatus>> GetAllConnectionStatus()
        {
            var statusList = new List<PLCConnectionStatus>();

            lock (_lockObject)
            {
                foreach (var connection in _connections.Values)
                {
                    statusList.Add(new PLCConnectionStatus
                    {
                        PLCName = connection.PLCName,
                        Gateway = connection.Gateway,
                        Path = connection.Path,
                        Status = connection.Status,
                        LastSuccessfulConnection = _lastSuccessfulConnection.ContainsKey(connection.PLCName)
                            ? _lastSuccessfulConnection[connection.PLCName]
                            : null,
                        LastErrorMessage = connection.LastErrorMessage,
                        LastErrorTime = connection.LastErrorTime,
                        TimeoutSeconds = connection.TimeoutSeconds
                    });
                }
            }

            return Task.FromResult(statusList);
        }

        public Task<PLCConnectionStatus> GetConnectionStatus(string plcName)
        {
            lock (_lockObject)
            {
                if (!_connections.ContainsKey(plcName))
                    throw new KeyNotFoundException($"PLC '{plcName}' không tồn tại");

                var connection = _connections[plcName];
                return Task.FromResult(new PLCConnectionStatus
                {
                    PLCName = connection.PLCName,
                    Gateway = connection.Gateway,
                    Path = connection.Path,
                    Status = connection.Status,
                    LastSuccessfulConnection = _lastSuccessfulConnection.ContainsKey(plcName)
                        ? _lastSuccessfulConnection[plcName]
                        : null,
                    LastErrorMessage = connection.LastErrorMessage,
                    LastErrorTime = connection.LastErrorTime,
                    TimeoutSeconds = connection.TimeoutSeconds
                });
            }
        }

        public Tag CreateTag(string plcName, string tagName)
        {
            lock (_lockObject)
            {
                if (!_connections.ContainsKey(plcName))
                    throw new KeyNotFoundException($"PLC '{plcName}' không tồn tại");

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

        public List<string> GetAllPLCNames()
        {
            lock (_lockObject)
            {
                return [.. _connections.Keys];
            }
        }
    }
}
