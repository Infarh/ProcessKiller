using System;
using System.Globalization;
using ProcessKiller.Converters.Base;

namespace ProcessKiller.Converters
{
    public class MemorySizeUnit : Converter
    {
        private static readonly string[] __Unit = { "", "k", "M", "G", "T", "P" };

        public override object? Convert(object? v, Type t, object? p, CultureInfo c)
        {
            if (v is not long size) return null;
            var index = size <= 0 ? 0 : (int)Math.Log(size, 1024);
            return __Unit[Math.Min(index, __Unit.Length - 1)];
        }
    }
}
