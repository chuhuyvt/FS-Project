using PlcTagApi.Core.Interfaces;
using PlcTagApi.Core.Models.Domain;
using PlcTagApi.Core.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlcTagApi.Infrastructure.Implementations
{
    /// <summary>
    /// Implements tag monitoring with conditions
    /// Single Responsibility: Condition evaluation and monitoring
    /// Dependency Inversion: Depends on IPLCTagReader abstraction
    /// </summary>
    public class PlcTagMonitor(IPLCTagReader tagReader) : IPLCTagMonitor
    {
        private readonly IPLCTagReader _tagReader = tagReader ?? throw new ArgumentNullException(nameof(tagReader));

        public async Task<List<TagValue>> MonitorWithConditionsAsync(string plcName, TagCondition[] conditions)
        {
            if (conditions == null || conditions.Length == 0)
                throw new ArgumentException("Conditions cannot be empty");

            var results = new List<TagValue>();

            foreach (var condition in conditions)
            {
                var tagValue = await _tagReader.ReadTagAsync(plcName, condition.TagName, "DINT");

                if (tagValue.Status == "OK")
                {
                    bool conditionMet = EvaluateCondition(tagValue.CurrentValue, condition);
                    tagValue.ErrorMessage = conditionMet
                        ? $"✓ Condition {condition.ConditionType} met"
                        : $"✗ Condition {condition.ConditionType} not met";
                }

                results.Add(tagValue);
            }

            return results;
        }

        public bool EvaluateCondition(object value, TagCondition condition)
        {
            try
            {
                double numValue = Convert.ToDouble(value);

                return condition.ConditionType.ToLower() switch
                {
                    "greaterthan" => numValue > Convert.ToDouble(condition.ThresholdValue),
                    "lessthan" => numValue < Convert.ToDouble(condition.ThresholdValue),
                    "equal" => Math.Abs(numValue - Convert.ToDouble(condition.ThresholdValue)) < 0.0001,
                    "between" => numValue >= Convert.ToDouble(condition.MinValue) &&
                                 numValue <= Convert.ToDouble(condition.MaxValue),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }
    }
}
