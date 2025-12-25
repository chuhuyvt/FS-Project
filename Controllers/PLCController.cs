using System;
using Microsoft.AspNetCore.Mvc;
using PlcTagApi.Models;
using PlcTagApi.Services;

namespace PlcTagApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlcController : ControllerBase
    {
        private readonly IPlcTagService _plcService;

        public PlcController(IPlcTagService plcService)
        {
            _plcService = plcService;
        }

        /// <summary>
        /// Test kết nối đến PLC
        /// </summary>
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

        /// <summary>
        /// Đọc một tag cụ thể
        /// </summary>
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

        /// <summary>
        /// Đọc tag BOOL
        /// </summary>
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

        /// <summary>
        /// Đọc tag DINT
        /// </summary>
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

        /// <summary>
        /// Đọc tag REAL
        /// </summary>
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

        /// <summary>
        /// Đọc tag STRING
        /// </summary>
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

        /// <summary>
        /// Đọc tag Array
        /// </summary>
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

        /// <summary>
        /// Đọc tất cả tag được theo dõi
        /// </summary>
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
    }
}
