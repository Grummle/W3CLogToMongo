using System.Collections.Generic;
using IILogReader.Binders;

namespace IILogReader
{
    public static class PrimitiveBinders
    {
        private static IEnumerable<IBinder> _binders;
        public static IEnumerable<IBinder> Binders
        {
            get
            {
                if (_binders == null)
                    _binders = new List<IBinder> { new DatTimeBinder(), new IntBinder() };

                return _binders;
            }
        }
    }
}