using System;

namespace IILogReader.Binders
{
    public class DatTimeBinder : IBinder
    {
        public bool Matches(Type type)
        {
            return typeof(DateTime) == type;
        }

        public object Bind(Type type, string value)
        {
            return DateTime.Parse(value+"Z");
        }
    }
}