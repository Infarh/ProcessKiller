using System;
using System.Globalization;
using ProcessKiller.Converters.Base;

namespace ProcessKiller.Converters
{
    public class MemorySize : Converter
    {
        public override object? Convert(object? v, Type t, object? p, CultureInfo c)
        {
            if (v is not long size) return null;
            var index = size <= 0 ? 0 : (int)Math.Log(size, 1024);
            return index == 0 ? size : size / (1 << (index * 10));
        }
    }
}
