using System;

namespace IILogReader.Binders
{
    public class IntBinder : IBinder
    {
        public bool Matches(Type type)
        {
            return typeof(int) == type;
        }

        public object Bind(Type type, string value)
        {
            return int.Parse(value);
        }
    }
}