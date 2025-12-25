using PlcTagApi.Core.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlcTagApi.Core.Interfaces
{
    /// <summary>
    /// Handles tag reading operations
    /// Segregated Interface: Reading only
    /// Single Responsibility: Tag reading logic
    /// </summary>
    public interface IPLCTagReader
    {
        Task<TagValue> ReadTagAsync(string plcName, string tagName, string tagType, int arraySize = 0);
        Task<List<TagValue>> ReadMultipleTagsAsync(string plcName, string[] tagNames, string[] tagTypes, int[] arraySizes);
    }
}
