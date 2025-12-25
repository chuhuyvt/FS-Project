// Controllers/PlcController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlcTagApi.Models;
using PlcTagApi.Services;

namespace PlcTagApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlcController(IPlcTagService plcService, IPLCConnectionPool connectionPool) : ControllerBase
    {
        private readonly IPlcTagService _plcService = plcService;
        private readonly IPLCConnectionPool _connectionPool = connectionPool;

        #region New Multi-PLC Connection Management Endpoints

        /// <summary>
        /// Thêm PLC connection mới
        /// </summary>
        [HttpPost("add-connection")]
        public async Task<ActionResult<TagResponse>> AddConnection([FromBody] AddPLCConnectionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PLCName))
                    return BadRequest(new TagResponse { Success = false, Message = "PLCName không được để trống" });

                if (string.IsNullOrWhiteSpace(request.Gateway))
                    return BadRequest(new TagResponse { Success = false, Message = "Gateway (IP) không được để trống" });

                var config = new PLCConnectionConfig
                {
                    PLCName = request.PLCName,
                    Gateway = request.Gateway,
                    Path = request.Path ?? "1,0",
                    TimeoutSeconds = request.TimeoutSeconds
                };

                await _connectionPool.AddConnection(config);

                return Ok(new TagResponse
                {
                    Success = true,
                    Message = $"Đã thêm PLC '{request.PLCName}' thành công",
                    Data = new { plcName = request.PLCName, gateway = request.Gateway }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi thêm kết nối: {ex.Message}"
                });
            }
        }

        [HttpPut("update-connection/{plcName}")]
        public async Task<ActionResult<TagResponse>> UpdateConnection(
            string plcName,
            [FromBody] UpdatePLCConnectionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plcName))
                    return BadRequest(new TagResponse 
                    { 
                        Success = false, 
                        Message = "PLCName không được để trống" 
                    });

                if (request == null)
                    return BadRequest(new TagResponse 
                    { 
                        Success = false, 
                        Message = "Request body không được để trống" 
                    });

                await _connectionPool.UpdateConnection(plcName, request);

                var updatedStatus = await _connectionPool.GetConnectionStatus(plcName);

                return Ok(new TagResponse
                {
                    Success = true,
                    Message = $"Đã cập nhật PLC '{plcName}' thành công",
                    Data = updatedStatus
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new TagResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi update kết nối: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa PLC connection
        /// </summary>
        [HttpDelete("remove-connection/{plcName}")]
        public async Task<ActionResult<TagResponse>> RemoveConnection(string plcName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plcName))
                    return BadRequest(new TagResponse { Success = false, Message = "PLCName không được để trống" });

                await _connectionPool.RemoveConnection(plcName);

                return Ok(new TagResponse
                {
                    Success = true,
                    Message = $"Đã xóa PLC '{plcName}' thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi xóa kết nối: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Test kết nối đến một PLC cụ thể
        /// </summary>
        [HttpGet("test-plc-connection/{plcName}")]
        public async Task<ActionResult<TagResponse>> TestPLCConnection(string plcName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plcName))
                    return BadRequest(new TagResponse { Success = false, Message = "PLCName không được để trống" });

                bool isConnected = await _plcService.TestPLCConnection(plcName);

                return Ok(new TagResponse
                {
                    Success = isConnected,
                    Message = isConnected 
                        ? $"Kết nối PLC '{plcName}' thành công" 
                        : $"Kết nối PLC '{plcName}' thất bại"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi test kết nối: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy trạng thái kết nối của một PLC cụ thể
        /// </summary>
        [HttpGet("connection-status/{plcName}")]
        public async Task<ActionResult<TagResponse>> GetConnectionStatus(string plcName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plcName))
                    return BadRequest(new TagResponse { Success = false, Message = "PLCName không được để trống" });

                var status = await _connectionPool.GetConnectionStatus(plcName);

                return Ok(new TagResponse
                {
                    Success = true,
                    Message = "Lấy trạng thái kết nối thành công",
                    Data = status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi lấy trạng thái: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy trạng thái kết nối của tất cả PLC
        /// </summary>
        [HttpGet("all-connections-status")]
        public async Task<ActionResult<TagResponse>> GetAllConnectionsStatus()
        {
            try
            {
                var statuses = await _connectionPool.GetAllConnectionStatus();

                return Ok(new TagResponse
                {
                    Success = true,
                    Message = "Lấy trạng thái tất cả kết nối thành công",
                    Data = statuses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        #endregion

        #region New Multi-PLC Tag Reading Endpoints

        /// <summary>
        /// Đọc tag từ PLC cụ thể
        /// </summary>
        [HttpPost("read-from-plc")]
        public async Task<ActionResult<TagResponse>> ReadTagFromPLC([FromBody] ReadTagWithPLCRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PLCName))
                    return BadRequest(new TagResponse { Success = false, Message = "PLCName không được để trống" });

                if (string.IsNullOrWhiteSpace(request.TagName))
                    return BadRequest(new TagResponse { Success = false, Message = "TagName không được để trống" });

                var tagValue = await _plcService.ReadTagFromPLC(
                    request.PLCName, 
                    request.TagName, 
                    request.TagType, 
                    request.ArraySize
                );

                return Ok(new TagResponse
                {
                    Success = tagValue.Status == "OK",
                    Message = tagValue.ErrorMessage ?? "Đọc tag thành công",
                    Data = tagValue
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Đọc nhiều tags từ PLC cụ thể
        /// </summary>
        [HttpPost("read-multiple-tags")]
        public async Task<ActionResult<TagResponse>> ReadMultipleTags([FromBody] MonitorTagsRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PLCName))
                    return BadRequest(new TagResponse { Success = false, Message = "PLCName không được để trống" });

                if (request.TagNames == null || request.TagNames.Length == 0)
                    return BadRequest(new TagResponse { Success = false, Message = "TagNames không được để trống" });

                var results = await _plcService.ReadMultipleTagsFromPLC(
                    request.PLCName,
                    request.TagNames,
                    request.TagTypes,
                    request.ArraySizes
                );

                return Ok(new TagResponse
                {
                    Success = true,
                    Message = $"Đọc {results.Count} tags thành công",
                    Data = results
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Đọc tags với điều kiện monitoring
        /// </summary>
        [HttpPost("monitor-with-conditions")]
        public async Task<ActionResult<TagResponse>> MonitorWithConditions([FromBody] MonitorTagsWithConditionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PLCName))
                    return BadRequest(new TagResponse { Success = false, Message = "PLCName không được để trống" });

                if (request.Conditions == null || request.Conditions.Length == 0)
                    return BadRequest(new TagResponse { Success = false, Message = "Conditions không được để trống" });

                var results = await _plcService.MonitorTagsWithConditions(request.PLCName, request.Conditions);

                var alertedTags = results.FindAll(t => t.ErrorMessage?.Contains("Điều kiện") == true);

                return Ok(new TagResponse
                {
                    Success = true,
                    Message = $"Monitoring {results.Count} tags, {alertedTags.Count} tag có điều kiện được thỏa mãn",
                    Data = new { totalTags = results.Count, alertedTags = alertedTags.Count, tags = results }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        #endregion

        #region Existing Endpoints (Default PLC - giữ nguyên)

        [HttpGet("test-connection")]
        public ActionResult<TagResponse> TestConnection()
        {
            try
            {
                bool isConnected = _plcService.TestConnection();
                return Ok(new TagResponse
                {
                    Success = isConnected,
                    Message = isConnected ? "Kết nối PLC thành công" : "Kết nối PLC thất bại"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        [HttpPost("read")]
        public ActionResult<TagResponse> ReadTag([FromBody] TagReadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TagName))
                    return BadRequest(new TagResponse { Success = false, Message = "TagName không được để trống" });

                var tagValue = _plcService.ReadTag(request.TagName, request.TagType, request.ArraySize);

                return Ok(new TagResponse
                {
                    Success = tagValue.Status == "OK",
                    Message = tagValue.ErrorMessage ?? "Đọc tag thành công",
                    Data = tagValue
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        [HttpGet("read-bool/{tagName}")]
        public ActionResult<TagResponse> ReadBool(string tagName)
        {
            try
            {
                var tagValue = _plcService.ReadTag(tagName, "BOOL");
                return Ok(new TagResponse
                {
                    Success = tagValue.Status == "OK",
                    Data = tagValue
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("read-dint/{tagName}")]
        public ActionResult<TagResponse> ReadDint(string tagName)
        {
            try
            {
                var tagValue = _plcService.ReadTag(tagName, "DINT");
                return Ok(new TagResponse
                {
                    Success = tagValue.Status == "OK",
                    Data = tagValue
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("read-real/{tagName}")]
        public ActionResult<TagResponse> ReadReal(string tagName)
        {
            try
            {
                var tagValue = _plcService.ReadTag(tagName, "REAL");
                return Ok(new TagResponse
                {
                    Success = tagValue.Status == "OK",
                    Data = tagValue
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("read-string/{tagName}")]
        public ActionResult<TagResponse> ReadString(string tagName)
        {
            try
            {
                var tagValue = _plcService.ReadTag(tagName, "STRING");
                return Ok(new TagResponse
                {
                    Success = tagValue.Status == "OK",
                    Data = tagValue
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("read-array/{tagName}")]
        public ActionResult<TagResponse> ReadArray(string tagName, [FromQuery] int arraySize = 0)
        {
            try
            {
                var tagValue = _plcService.ReadTag(tagName, "ARRAY", arraySize);
                return Ok(new TagResponse
                {
                    Success = tagValue.Status == "OK",
                    Data = tagValue
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("read-all")]
        public ActionResult<TagResponse> ReadAllMonitoredTags()
        {
            try
            {
                var response = _plcService.GetAllMonitoredTags();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new TagResponse { Success = false, Message = ex.Message });
            }
        }

        #endregion
    }
}
