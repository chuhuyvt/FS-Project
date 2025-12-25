using PlcTagApi.Core.Models.Domain;
using PlcTagApi.Core.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlcTagApi.Core.Interfaces
{
    public interface IPlcTagService
    {
        Task<TagValue> ReadTagAsync(string plcName, string tagName, string tagType, int arraySize = 0);
        Task<List<TagValue>> ReadMultipleTagsAsync(string plcName, string[] tagNames, string[] tagTypes, int[] arraySizes);
        Task<List<TagValue>> MonitorWithConditionsAsync(string plcName, TagCondition[] conditions);
        TagResponse GetAllMonitoredTagsFromDefault();
        bool TestDefaultConnection();
    }
}
