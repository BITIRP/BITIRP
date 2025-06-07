using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFS_Intersection
{
    internal class Entity
    {
        public int Index { get; set; }
        public List<SymbolicTimeInterval> SymbolicTimeIntervals { get; set; }

        public Entity(int index, List<SymbolicTimeInterval> symbolicTimeIntervals)
        {
            Index = index;
            SymbolicTimeIntervals = symbolicTimeIntervals;
        }
    }
}
