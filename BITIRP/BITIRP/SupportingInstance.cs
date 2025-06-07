using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFS_Intersection
{
    internal class SupportingInstance
    {
        public List<SymbolicTimeInterval> SymbolicTimeIntervals { get; set; }

        public SupportingInstance(List<SymbolicTimeInterval> symbolicTimeIntervals)
        {
            SymbolicTimeIntervals = symbolicTimeIntervals;
            ExtendWithKey = string.Join("^", SymbolicTimeIntervals.Take(SymbolicTimeIntervals.Count - 1).Select(sti => sti.Index.ToString()));
            ExtendByKey = string.Join("^", SymbolicTimeIntervals.Skip(1).Select(sti => sti.Index.ToString()));
        }

        public string ExtendWithKey { get; }

        public string ExtendByKey { get; }

        public override string ToString()
        {
            return string.Join("", SymbolicTimeIntervals.Select(sti => sti.ToString()));
        }
    }
}
