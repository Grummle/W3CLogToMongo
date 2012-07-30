using System;

namespace IILogReader.Binders
{
    public interface IBinder
    {
        bool Matches(Type type);
        object Bind(Type type, string context);
    }
}