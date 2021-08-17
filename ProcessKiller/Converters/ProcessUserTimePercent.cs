using System;
using System.Diagnostics;
using System.Globalization;
using ProcessKiller.Converters.Base;

namespace ProcessKiller.Converters
{
    public class ProcessUserTimePercent : Converter
    {
        public override object? Convert(object? v, Type t, object? p, CultureInfo c)
        {
            if (v is not Process process) return null;
            var time = process.UserProcessorTime.TotalSeconds;
            var total_time = (DateTime.Now - process.StartTime).TotalSeconds;
            return time / total_time;
        }
    }

    public class ProcessTimePercent : Converter
    {
        public override object? Convert(object? v, Type t, object? p, CultureInfo c)
        {
            if (v is not Process process) return null;
            var time = process.TotalProcessorTime.TotalSeconds;
            var total_time = (DateTime.Now - process.StartTime).TotalSeconds;
            return time / total_time;
        }
    }
}
