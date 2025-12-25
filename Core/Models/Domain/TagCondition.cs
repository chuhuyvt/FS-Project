namespace PlcTagApi.Core.Models.Domain
{
    /// <summary>
    /// Represents a monitoring condition for tags
    /// </summary>
    public class TagCondition
    {
        public string TagName { get; set; }
        public string ConditionType { get; set; } // GreaterThan, LessThan, Equal, Between
        public object ThresholdValue { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
    }
}
