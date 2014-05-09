using System;
using Raven.Client.Converters;

namespace NPSharp.CommandLine.Server
{
    public class UInt32Converter : ITypeConverter
    {
        public bool CanConvertFrom(Type sourceType)
        {
            return sourceType == typeof(uint);
        }
        public string ConvertFrom(string tag, object value, bool allowNull)
        {
            var val = (uint)value;
            if (val == 0 && allowNull)
                return null;
            return tag + value;
        }
        public object ConvertTo(string value)
        {
            return uint.Parse(value);
        }
    }
}
