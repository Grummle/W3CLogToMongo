using System;

namespace IILogReader
{
    public class Conversion
    {
        public string ElementName { get; set; }
        public Type DataType { get; set; }

        public Conversion ToType(Type type)
        {
            DataType = type;
            return this;
        }
    }
}