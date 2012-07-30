using System.Collections.Generic;
using System.Linq;

namespace IILogReader
{
    public class CompositeElement
    {
        public string Name { get; set; }
        public string[] Components { get; set; }
        public string Glue { get; set; }

        public CompositeElement() { Glue = " "; }

        public CompositeElement Named(string name)
        {
            Name = name;
            return this;
        }

        public CompositeElement FromElements(params string[] elements)
        {
            Components = elements;
            return this;
        }

        public CompositeElement JoinedWith(string glue)
        {
            Glue = glue;
            return this;
        }

        public object BuildComposite(IDictionary<string, object> entry)
        {
            return Components.Select(x => entry[x]).Join(Glue);
        }
    }
}