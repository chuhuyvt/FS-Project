using libplctag;
using PlcTagApi.Core.Interfaces;
using PlcTagApi.Core.Models.Responses;
using PlcTagApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PlcTagApi.Infrastructure.Implementations
{
    /// <summary>
    /// Implements tag reading operations
    /// Single Responsibility: Tag reading only
    /// Dependency Inversion: Depends on IPlcConnectionPool abstraction
    /// </summary>
    public class PlcTagReader(IPlcConnectionPool connectionPool) : IPLCTagReader
    {
        private readonly IPlcConnectionPool _connectionPool = connectionPool ?? throw new ArgumentNullException(nameof(connectionPool));

        public async Task<TagValue> ReadTagAsync(string plcName, string tagName, string tagType, int arraySize = 0)
        {
            Tag tag = null;
            var result = new TagValue { TagName = tagName, TagType = tagType };

            try
            {
                ValidateInputs(plcName, tagName);

                tag = _connectionPool.CreateTag(plcName, tagName);
                tag.Initialize();
                tag.Read();

                result.CurrentValue = ExtractValue(tag, tagType, arraySize);
                result.Status = "OK";
                result.LastChanged = DateTime.Now;

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                result.Status = "ERROR";
                result.ErrorMessage = ex.Message;
                return await Task.FromResult(result);
            }
            finally
            {
                tag?.Dispose();
            }
        }

        public async Task<List<TagValue>> ReadMultipleTagsAsync(string plcName, string[] tagNames, string[] tagTypes, int[] arraySizes)
        {
            var results = new List<TagValue>();

            if (tagNames.Length != tagTypes.Length)
                throw new ArgumentException("tagNames and tagTypes arrays must have same length");

            for (int i = 0; i < tagNames.Length; i++)
            {
                int arraySize = (arraySizes != null && i < arraySizes.Length) ? arraySizes[i] : 0;
                var tagValue = await ReadTagAsync(plcName, tagNames[i], tagTypes[i], arraySize);
                results.Add(tagValue);
            }

            return results;
        }

        private object ExtractValue(Tag tag, string tagType, int arraySize)
        {
            return tagType.ToUpper() switch
            {
                "BOOL" => tag.GetUInt8(0) != 0,
                "DINT" => tag.GetInt32(0),
                "REAL" => tag.GetFloat32(0),
                "STRING" => ReadStringValue(tag),
                "ARRAY" => ReadArrayValues(tag, arraySize),
                _ => throw new InvalidOperationException($"Tag type '{tagType}' not supported")
            };
        }

        private string ReadStringValue(Tag tag)
        {
            int stringLength = tag.GetInt32(0);
            int bufferSize = tag.GetBuffer().Length;

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

        private void ValidateInputs(string plcName, string tagName)
        {
            if (string.IsNullOrWhiteSpace(plcName))
                throw new ArgumentException("PLCName cannot be empty");

            if (string.IsNullOrWhiteSpace(tagName))
                throw new ArgumentException("TagName cannot be empty");
        }
    }
}
