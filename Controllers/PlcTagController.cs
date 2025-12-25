using Microsoft.AspNetCore.Mvc;
using PlcTagApi.Core.Interfaces;
using PlcTagApi.Core.Models.Requests;
using PlcTagApi.Core.Models.Responses;
using System;
using System.Threading.Tasks;

namespace PlcTagApi.Controllers
{
    /// <summary>
    /// REST API for tag reading and monitoring
    /// Single Responsibility: HTTP handling only
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PlcTagController(IPlcTagService tagService) : ControllerBase
    {
        private readonly IPlcTagService _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));

        [HttpPost("read")]
        public async Task<ActionResult<ApiResponse>> ReadTag([FromBody] ReadTagRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse.ErrorResponse("Request body is required"));

                var tagValue = await _tagService.ReadTagAsync(
                    request.PLCName,
                    request.TagName,
                    request.TagType,
                    request.ArraySize
                );

                return Ok(ApiResponse.SuccessResponse("Tag read successfully", tagValue));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error reading tag: {ex.Message}"));
            }
        }

        [HttpPost("read-multiple")]
        public async Task<ActionResult<ApiResponse>> ReadMultipleTags([FromBody] MonitorTagsRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse.ErrorResponse("Request body is required"));

                var results = await _tagService.ReadMultipleTagsAsync(
                    request.PLCName,
                    request.TagNames,
                    request.TagTypes,
                    request.ArraySizes
                );

                return Ok(ApiResponse.SuccessResponse($"Read {results.Count} tags successfully", results));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error reading tags: {ex.Message}"));
            }
        }

        [HttpPost("monitor")]
        public async Task<ActionResult<ApiResponse>> MonitorWithConditions([FromBody] MonitorTagsRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(ApiResponse.ErrorResponse("Request body is required"));

                var results = await _tagService.MonitorWithConditionsAsync(
                    request.PLCName,
                    request.Conditions
                );

                var alertedCount = results.FindAll(t => t.ErrorMessage?.Contains("✓") == true).Count;

                return Ok(ApiResponse.SuccessResponse(
                    $"Monitored {results.Count} tags, {alertedCount} conditions met",
                    new { totalTags = results.Count, alertedTags = alertedCount, tags = results }
                ));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse($"Error monitoring tags: {ex.Message}"));
            }
        }
    }
}
