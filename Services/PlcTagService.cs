// Services/PlcTagService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libplctag;
using PlcTagApi.Models;

namespace PlcTagApi.Services
{
    public interface IPlcTagService
    {
        TagValue ReadTag(string tagName, string tagType, int arraySize = 0);
        TagResponse GetAllMonitoredTags();
        bool TestConnection();

        Task<TagValue> ReadTagFromPLC(string plcName, string tagName, string tagType, int arraySize = 0);
        Task<List<TagValue>> ReadMultipleTagsFromPLC(string plcName, string[] tagNames, string[] tagTypes, int[] arraySizes);
        Task<bool> TestPLCConnection(string plcName);
        Task<List<TagValue>> MonitorTagsWithConditions(string plcName, TagCondition[] conditions);
    }

    public class PlcTagService : IPlcTagService
    {
        private const string GATEWAY = "10.44.189.226";
        private const string PATH = "1,0";
        private const int TIMEOUT_SECONDS = 5;

        private readonly Dictionary<string, TagValue> _tagCache = new();
        private readonly IPLCConnectionPool _connectionPool;

        public PlcTagService(IPLCConnectionPool connectionPool)
        {
            _connectionPool = connectionPool;
            InitializeMonitoredTags();
        }

        private void InitializeMonitoredTags()
        {
            _tagCache["yyy"] = new TagValue { TagName = "yyy", TagType = "BOOL", Status = "INITIALIZING" };
            _tagCache["x"] = new TagValue { TagName = "x", TagType = "DINT", Status = "INITIALIZING" };
            _tagCache["array_dint"] = new TagValue { TagName = "array_dint", TagType = "ARRAY", Status = "INITIALIZING" };
        }

        #region Original Methods (Default PLC)

        public bool TestConnection()
        {
            var tag = CreateTag("x");
            try
            {
                tag.Initialize();
                tag.Read();
                tag.Dispose();
                return true;
            }
            catch
            {
                tag.Dispose();
                return false;
            }
        }

        public TagValue ReadTag(string tagName, string tagType, int arraySize = 0)
        {
            var tag = CreateTag(tagName);
            var result = new TagValue { TagName = tagName, TagType = tagType };

            try
            {
                tag.Initialize();
                tag.Read();

                switch (tagType.ToUpper())
                {
                    case "BOOL":
                        result.CurrentValue = tag.GetUInt8(0) != 0;
                        break;

                    case "DINT":
                        result.CurrentValue = tag.GetInt32(0);
                        break;

                    case "REAL":
                        result.CurrentValue = tag.GetFloat32(0);
                        break;

                    case "STRING":
                        result.CurrentValue = ReadStringValue(tag, 82);
                        break;

                    case "ARRAY":
                        result.CurrentValue = ReadArrayValues(tag, arraySize);
                        break;

                    default:
                        throw new InvalidOperationException($"Loại tag không được hỗ trợ: {tagType}");
                }

                result.Status = "OK";
                result.LastChanged = DateTime.Now;

                if (_tagCache.ContainsKey(tagName))
                {
                    _tagCache[tagName].PreviousValue = _tagCache[tagName].CurrentValue;
                    _tagCache[tagName].CurrentValue = result.CurrentValue;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Status = "ERROR";
                result.ErrorMessage = ex.Message;
                return result;
            }
            finally
            {
                tag.Dispose();
            }
        }

        public TagResponse GetAllMonitoredTags()
        {
            var response = new TagResponse { Success = true };
            var tags = new List<TagValue>();

            tags.Add(ReadTag("yyy", "BOOL"));
            tags.Add(ReadTag("x", "DINT"));
            tags.Add(ReadTag("array_dint", "ARRAY", 2));

            response.Data = tags;
            return response;
        }

        #endregion

        #region New Methods (Multi-PLC Support)

        public async Task<bool> TestPLCConnection(string plcName)
        {
            return await _connectionPool.TestConnection(plcName);
        }

        public async Task<TagValue> ReadTagFromPLC(string plcName, string tagName, string tagType, int arraySize = 0)
        {
            Tag tag = null;
            var result = new TagValue { TagName = tagName, TagType = tagType };

            try
            {
                tag = _connectionPool.CreateTag(plcName, tagName);
                tag.Initialize();
                tag.Read();

                switch (tagType.ToUpper())
                {
                    case "BOOL":
                        result.CurrentValue = tag.GetUInt8(0) != 0;
                        break;

                    case "DINT":
                        result.CurrentValue = tag.GetInt32(0);
                        break;

                    case "REAL":
                        result.CurrentValue = tag.GetFloat32(0);
                        break;

                    case "STRING":
                        result.CurrentValue = ReadStringValue(tag, 82);
                        break;

                    case "ARRAY":
                        result.CurrentValue = ReadArrayValues(tag, arraySize);
                        break;

                    default:
                        throw new InvalidOperationException($"Loại tag không được hỗ trợ: {tagType}");
                }

                result.Status = "OK";
                result.LastChanged = DateTime.Now;

                return result;
            }
            catch (Exception ex)
            {
                result.Status = "ERROR";
                result.ErrorMessage = ex.Message;
                return result;
            }
            finally
            {
                tag?.Dispose();
            }
        }

        public async Task<List<TagValue>> ReadMultipleTagsFromPLC(
            string plcName, 
            string[] tagNames, 
            string[] tagTypes, 
            int[] arraySizes)
        {
            var results = new List<TagValue>();

            if (tagNames.Length != tagTypes.Length)
                throw new ArgumentException("Số lượng tagNames và tagTypes phải bằng nhau");

            for (int i = 0; i < tagNames.Length; i++)
            {
                int arraySize = (arraySizes != null && i < arraySizes.Length) ? arraySizes[i] : 0;
                var tagValue = await ReadTagFromPLC(plcName, tagNames[i], tagTypes[i], arraySize);
                results.Add(tagValue);
            }

            return results;
        }

        public async Task<List<TagValue>> MonitorTagsWithConditions(
            string plcName, 
            TagCondition[] conditions)
        {
            var results = new List<TagValue>();

            foreach (var condition in conditions)
            {
                var tagValue = await ReadTagFromPLC(plcName, condition.TagName, "DINT");

                if (tagValue.Status == "OK")
                {
                    bool conditionMet = EvaluateCondition(tagValue.CurrentValue, condition);
                    
                    tagValue.ErrorMessage = conditionMet 
                        ? $"✓ Điều kiện {condition.ConditionType} được thỏa mãn" 
                        : $"✗ Điều kiện {condition.ConditionType} không được thỏa mãn";
                }

                results.Add(tagValue);
            }

            return results;
        }

        private bool EvaluateCondition(object value, TagCondition condition)
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

        #endregion

        #region Helper Methods

        private string ReadStringValue(Tag tag, int bufferSize)
        {
            int stringLength = tag.GetInt32(0);
            if (stringLength > bufferSize - 4)
                stringLength = bufferSize - 4;

            byte[] stringBytes = new byte[stringLength];
            for (int i = 0; i < stringLength; i++)
            {
                stringBytes[i] = tag.GetUInt8(4 + i);
            }

            return Encoding.ASCII.GetString(stringBytes);
        }

        private int[] ReadArrayValues(Tag tag, int arraySize)
        {
            if (arraySize == 0)
            {
                int bufferSize = tag.GetBuffer().Length;
                arraySize = bufferSize / 4;
            }

            int[] values = new int[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                try
                {
                    values[i] = tag.GetInt32(i * 4);
                }
                catch
                {
                    values[i] = 0;
                }
            }

            return values;
        }

        private Tag CreateTag(string tagName)
        {
            return new Tag()
            {
                Name = tagName,
                Gateway = GATEWAY,
                Path = PATH,
                PlcType = PlcType.ControlLogix,
                Protocol = Protocol.ab_eip,
                Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS)
            };
        }

        #endregion
    }
}
