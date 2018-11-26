using Thorium.Core.DataIntegration.Attributes;
using Thorium.Core.DataIntegration.Constants;
using Thorium.Core.DataIntegration.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Thorium.Core.Serializers.FixedWidthSerializer
{
    /// <summary>
    /// Serializer for fixed width files.
    /// <remarks>This code can probably be optimized by using string builder. We don't worry about that until we need to.</remarks>
    /// </summary>
    public class FixedWidthSerializer : IIntegrationDataSerializer
    {
        public bool TrimFields { get; set; } = true;
        public string DefaultDateFormat { get; set; } = null;

        public string Delimiter { get; set; } = null;

        public IEnumerable<TData> GetData<TData>(MemoryStream rawData) where TData : new()
        {
            rawData.Position = 0;
            var dataList = new List<TData>();
            var properties = typeof(TData).GetProperties();
            using (var sr = new StreamReader(rawData))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    dataList.Add(GetRecord<TData>(line, properties));
                }
            }

            return dataList;
        }

        public MemoryStream GenerateRawData<TData>(IEnumerable<TData> data)
        {
            var result = new MemoryStream();
            var streamWriter = new StreamWriter(result);

            var enumerable = data as IList<TData> ?? data.ToList();

            foreach (var item in enumerable)
            {
                var line = string.Empty;

                foreach (var propertyInfo in typeof(TData).GetProperties())
                {
                    line = ProcessFileLine(propertyInfo, item, line);
                }

                streamWriter.WriteLineAsync(line);
            }

            streamWriter.Flush();

            return result;
        }

        private string ProcessFileLine<TData>(PropertyInfo p, TData item, string line)
        {
            var fieldFormats = p.GetCustomAttributes<FixedWidthFieldAttribute>();
            var fixedWidthFieldAttributes = fieldFormats.ToList();
            var dateFormat = p.GetCustomAttribute<DateTimeFormatAttribute>();
            var value = string.Empty;

            if (!fixedWidthFieldAttributes.Any())
            {
                return line;
            }

            var numberOfFixedWidthAttribs = fixedWidthFieldAttributes.Count();
            if (numberOfFixedWidthAttribs > 2)
            {
                throw new Exception($"More than two FixedWidthFieldAttributes on {typeof(TData)}.{p.Name}");
            }

            //If there is one FixedWidthFieldAttribute then innerfieldFormat is the first element. If there is two the innerfieldFormat is the second element.
            var fieldFormat = fixedWidthFieldAttributes.OrderByDescending(_ => _.Width)
                .ElementAt(numberOfFixedWidthAttribs - 1);

            //var name = p.Name; //Used for debugging
            var propValue = p.GetValue(item);
            var width = fieldFormat.Width;

            if (propValue != null)
            {
                value = SetPropertyValue<TData>(p, dateFormat, propValue, width);
            }

            //inner padding
            var paddedValue = string.Empty;
            if (width > value.Length)
            {
                paddedValue += ApplyPadding(value, fieldFormat.PaddingDirection, fieldFormat.PaddingCharacter, width);
            }
            else
            {
                paddedValue += value;
            }

            if (!String.IsNullOrEmpty(Delimiter))
            {
                paddedValue += Delimiter;
            }

            if (numberOfFixedWidthAttribs != 2)
            {
                line += paddedValue;
                return line;
            }

            //outer padding
            fieldFormat = fixedWidthFieldAttributes.OrderByDescending(_ => _.Width).ElementAt(0);
            width = fieldFormat.Width;

            if (width > paddedValue.Length)
            {
                line += ApplyPadding(paddedValue, fieldFormat.PaddingDirection, fieldFormat.PaddingCharacter, width);
            }

            return line;
        }

        private string ApplyPadding(string paddedValue, PaddingDirection direction, char character, int width)
        {
            return direction == PaddingDirection.Left
                ? paddedValue.PadLeft(width, character)
                : paddedValue.PadRight(width, character);
        }

        private string SetPropertyValue<TData>(PropertyInfo p, DateTimeFormatAttribute dateFormat, object propValue,
            int width)
        {
            string value;
            var dateVal = GetDateStringValue(p, dateFormat, propValue);

            if (dateVal.isDate)
            {
                value = dateVal.dateTimeVal;
            }
            else
            {
                value = propValue.ToString();
                value = width < value.Length
                    ? value.Substring(0, width)
                    : value; //truncate field when value is larger then the width attribute.
            }

            return value;
        }

        /// <summary>
        /// Gets a date input property as a string.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="dateTimeFormatAttribute"></param>
        /// <param name="propValue"></param>
        /// <returns></returns>
        private (bool isDate, string dateTimeVal) GetDateStringValue(PropertyInfo propertyInfo,
            DateTimeFormatAttribute dateTimeFormatAttribute, object propValue)
        {
            var dateVal = "";

            var isDateTimeProperty = propertyInfo.PropertyType == typeof(DateTime)
                                     ||
                                     propertyInfo.PropertyType == typeof(DateTime?);
            var hasDateFormat = DefaultDateFormat != null
                                ||
                                dateTimeFormatAttribute != null;
            var isDate = isDateTimeProperty && hasDateFormat;
                
            if (isDate)
            {
                dateVal = ((DateTime) propValue).ToString(dateTimeFormatAttribute?.DateTimeFormat ?? DefaultDateFormat);
            }

            return (isDate, dateVal);
        }

        public void ReadSingle<TData, TDiscriminator>(Action<TData> assignAction,
            Func<TDiscriminator, bool> discriminator, MemoryStream rawData)
            where TData : new() where TDiscriminator : new()
        {
            try
            {
                rawData.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(rawData);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var discriminatorValue = GetRecord<TDiscriminator>(line);
                    if (discriminator(discriminatorValue))
                    {
                        assignAction(GetRecord<TData>(line));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ReadMany<TData, TDiscriminator>(IList<TData> destination, Func<TDiscriminator, bool> discriminator,
            MemoryStream stream) where TData : new() where TDiscriminator : new()
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(stream);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var discriminatorValue = GetRecord<TDiscriminator>(line);
                    if (discriminator(discriminatorValue))
                    {
                        destination.Add(GetRecord<TData>(line));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetDataFromSegment(string segment, object dataObject, PropertyInfo property)
        {
            var dateTimeFormatAttrib = property.GetCustomAttribute<DateTimeFormatAttribute>();
            if (dateTimeFormatAttrib != null)
            {
                var dateTimeFormat = dateTimeFormatAttrib.DateTimeFormat;

                var dt = DateTime.ParseExact(segment.Trim(), dateTimeFormat, CultureInfo.InvariantCulture);
                property.SetValue(dataObject, dt);
                return;
            }

            if (property.PropertyType == typeof(string) && TrimFields)
            {
                property.SetValue(dataObject, segment.Trim());

                return;
            }

            property.SetValue(dataObject, segment);
        }

        private TData GetRecord<TData>(string line, PropertyInfo[] properties = null) where TData : new()
        {
            var dataObject = new TData();

            foreach (var property in properties ?? typeof(TData).GetProperties())
            {
                var formattingAttribute = property.GetCustomAttribute<FixedWidthFieldAttribute>();

                if (formattingAttribute == null)
                {
                    // we have no idea how to parse this field because we don't know how long it is
                    continue;
                }

                if (line.Length < formattingAttribute.Width)
                {
                    // the line is shorter than the padding. we can't work with that!
                    continue;
                }

                if (!String.IsNullOrEmpty(Delimiter))
                {
                    line = line.Replace(Delimiter, "");
                }

                var segment = line.Substring(0, formattingAttribute.Width);
                SetDataFromSegment(segment, dataObject, property);
                // cut the rest of the string for the next field
                line = line.Substring(formattingAttribute.Width);
            }

            return dataObject;
        }
    }
}
