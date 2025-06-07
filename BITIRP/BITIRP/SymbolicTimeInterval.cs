using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFS_Intersection
{
    internal class SymbolicTimeInterval
    {
        public int Index { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public int Symbol { get; set; }

        public SymbolicTimeInterval(int Index, int StartTime, int EndTime, int Symbol)
        {
            this.Index = Index;
            this.StartTime = StartTime;
            this.EndTime = EndTime;
            this.Symbol = Symbol;
        }

        public override string ToString()
        {
            return $"[{StartTime}-{EndTime}]";
        }
    }
}
