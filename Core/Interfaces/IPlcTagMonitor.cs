using PlcTagApi.Core.Models.Domain;
using PlcTagApi.Core.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlcTagApi.Core.Interfaces
{
    /// <summary>
    /// Handles tag monitoring with conditions
    /// Segregated Interface: Monitoring only
    /// Single Responsibility: Condition-based monitoring
    /// </summary>
    public interface IPLCTagMonitor
    {
        Task<List<TagValue>> MonitorWithConditionsAsync(string plcName, TagCondition[] conditions);
        bool EvaluateCondition(object value, TagCondition condition);
    }
}
