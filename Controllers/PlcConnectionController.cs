using Microsoft.AspNetCore.Mvc;
using PlcTagApi.Core.Interfaces;
using PlcTagApi.Core.Models.Domain;
using PlcTagApi.Core.Models.Requests;
using PlcTagApi.Core.Models.Responses;
using PlcTagApi.Exceptions;
using System;
using System.Threading.Tasks;

namespace PlcTagApi.Controllers
{
    /// <summary>
    /// REST API for PLC connection management
    /// Single Responsibility: HTTP handling only
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PlcConnectionController(IPlcConnectionPool connectionPool) : ControllerBase
    {
        private readonly IPlcConnectionPool _connectionPool = connectionPool ?? throw new ArgumentNullException(nameof(connectionPool));

        [HttpPost("add-connection")]
        public async Task<ActionResult<ApiResponse>> AddConnection([FromBody] AddPLCConnectionRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse.ErrorResponse("Request body is required"));

                var config = new PLCConnectionConfig
                {
                    PLCName = request.PLCName,
                    Gateway = request.Gateway,
                    Path = request.Path,
                    TimeoutSeconds = request.TimeoutSeconds
                };

                await _connectionPool.AddConnectionAsync(config);

                return Ok(ApiResponse.SuccessResponse(
                    $"PLC '{request.PLCName}' added successfully",
                    new { plcName = request.PLCName, gateway = request.Gateway }
                ));
            }
            catch (InvalidConfigurationException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error adding connection: {ex.Message}"));
            }
        }

        [HttpPut("update-connection/{plcName}")]
        public async Task<ActionResult<ApiResponse>> UpdateConnection(
            string plcName,
            [FromBody] UpdatePLCConnectionRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse.ErrorResponse("Request body is required"));

                await _connectionPool.UpdateConnectionAsync(
                    plcName,
                    request.Gateway,
                    request.Path,
                    request.TimeoutSeconds
                );

                var status = await _connectionPool.GetConnectionStatusAsync(plcName);
                return Ok(ApiResponse.SuccessResponse(
                    $"PLC '{plcName}' updated successfully",
                    status
                ));
            }
            catch (InvalidConfigurationException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message, ex.ErrorCode));
            }
            catch (PlcConnectionException ex)
            {
                return NotFound(ApiResponse.ErrorResponse(ex.Message, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error updating connection: {ex.Message}"));
            }
        }

        [HttpDelete("remove-connection/{plcName}")]
        public async Task<ActionResult<ApiResponse>> RemoveConnection(string plcName)
        {
            try
            {
                await _connectionPool.RemoveConnectionAsync(plcName);
                return Ok(ApiResponse.SuccessResponse($"PLC '{plcName}' removed successfully"));
            }
            catch (PlcConnectionException ex)
            {
                return NotFound(ApiResponse.ErrorResponse(ex.Message, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error removing connection: {ex.Message}"));
            }
        }

        [HttpGet("test-connection/{plcName}")]
        public async Task<ActionResult<ApiResponse>> TestConnection(string plcName)
        {
            try
            {
                bool isConnected = await _connectionPool.TestConnectionAsync(plcName);
                return Ok(ApiResponse.SuccessResponse(
                    isConnected ? $"Connection to '{plcName}' successful" : $"Connection to '{plcName}' failed"
                ));
            }
            catch (PlcConnectionException ex)
            {
                return NotFound(ApiResponse.ErrorResponse(ex.Message, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error testing connection: {ex.Message}"));
            }
        }

        [HttpGet("status/{plcName}")]
        public async Task<ActionResult<ApiResponse>> GetConnectionStatus(string plcName)
        {
            try
            {
                var status = await _connectionPool.GetConnectionStatusAsync(plcName);
                return Ok(ApiResponse.SuccessResponse("Connection status retrieved", status));
            }
            catch (PlcConnectionException ex)
            {
                return NotFound(ApiResponse.ErrorResponse(ex.Message, ex.ErrorCode));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error retrieving status: {ex.Message}"));
            }
        }

        [HttpGet("all-status")]
        public async Task<ActionResult<ApiResponse>> GetAllConnectionStatus()
        {
            try
            {
                var statuses = await _connectionPool.GetAllConnectionStatusAsync();
                return Ok(ApiResponse.SuccessResponse("All connection statuses retrieved", statuses));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error retrieving statuses: {ex.Message}"));
            }
        }
    }
}
