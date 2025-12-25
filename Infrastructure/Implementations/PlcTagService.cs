using libplctag;
using PlcTagApi.Core.Interfaces;
using PlcTagApi.Core.Models.Domain;
using PlcTagApi.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlcTagApi.Infrastructure.Implementations
{
    /// <summary>
    /// High-level tag service (Facade pattern)
    /// Single Responsibility: Orchestrate read/monitor operations
    /// Dependency Inversion: Depends on IPLCTagReader and IPLCTagMonitor
    /// Open/Closed: Easy to extend without modifying
    /// </summary>
    public class PlcTagService : IPlcTagService  // ← CRITICAL: must implement interface
{
    private readonly IPLCTagReader _tagReader;
    private readonly IPLCTagMonitor _tagMonitor;
    private readonly IPlcConnectionPool _connectionPool;

    public PlcTagService(
        IPLCTagReader tagReader,
        IPLCTagMonitor tagMonitor,
        IPlcConnectionPool connectionPool)
    {
        _tagReader = tagReader ?? throw new ArgumentNullException(nameof(tagReader));
        _tagMonitor = tagMonitor ?? throw new ArgumentNullException(nameof(tagMonitor));
        _connectionPool = connectionPool ?? throw new ArgumentNullException(nameof(connectionPool));
    }

    public async Task<TagValue> ReadTagAsync(string plcName, string tagName, string tagType, int arraySize = 0)
    {
        return await _tagReader.ReadTagAsync(plcName, tagName, tagType, arraySize);
    }

    public async Task<List<TagValue>> ReadMultipleTagsAsync(string plcName, string[] tagNames, string[] tagTypes, int[] arraySizes)
    {
        return await _tagReader.ReadMultipleTagsAsync(plcName, tagNames, tagTypes, arraySizes);
    }

    public async Task<List<TagValue>> MonitorWithConditionsAsync(string plcName, TagCondition[] conditions)
    {
        return await _tagMonitor.MonitorWithConditionsAsync(plcName, conditions);
    }

    public TagResponse GetAllMonitoredTagsFromDefault()
    {
        try
        {
            var response = new TagResponse 
            { 
                Success = true,
                Message = "Default PLC tags retrieved",
                Data = null
            };
            return response;
        }
        catch (Exception ex)
        {
            return new TagResponse 
            { 
                Success = false, 
                Message = ex.Message,
                ErrorCode = "DEFAULT_PLC_ERROR"
            };
        }
    }

    public bool TestDefaultConnection()
    {
        try
        {
            return _connectionPool.IsConnectionActive("PLC-Default");
        }
        catch
        {
            return false;
        }
    }
}

}
