using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFS_Intersection
{
    internal enum Relation
    {
        NOT_DEFINED = -1,
        BEFORE = 0,
        MEET = 1,
        OVERLAP = 2,
        FINISHBY = 3,
        CONTAIN = 4,
        STARTS = 5,
        EQUAL = 6,
    }

    internal static class RelationHelper
    {
        public static string AsString(Relation relation)
        {
            return relation switch
            {
                Relation.BEFORE => "<",
                Relation.MEET => "m",
                Relation.OVERLAP => "o",
                Relation.FINISHBY => "f",
                Relation.CONTAIN => "c",
                Relation.STARTS => "S",
                Relation.EQUAL => "=",
                _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null),
            };
        }

        public static bool IsLexicographicallyValid(SymbolicTimeInterval first, SymbolicTimeInterval second)
        {
            if (first.StartTime < second.StartTime)
            {
                return true;
            }

            if (first.StartTime == second.StartTime && first.EndTime < second.EndTime)
            {
                return true;
            }

            if (first.StartTime == second.StartTime && first.EndTime == second.EndTime && first.Symbol < second.Symbol)
            {
                return true;
            }

            return false;
        }

        public static Relation CheckRelation(SymbolicTimeInterval first, SymbolicTimeInterval second, int maxGap)
        {
            Debug.Assert(IsLexicographicallyValid(first, second)); // Use Debug.Assert for assertions in development

            if (!(second.StartTime - first.EndTime < maxGap))
            {
                return Relation.NOT_DEFINED;
            }

            if (first.EndTime < second.StartTime)
            {
                return Relation.BEFORE;
            }

            if (first.EndTime == second.StartTime)
            {
                return Relation.MEET;
            }

            if (first.StartTime < second.StartTime && first.EndTime < second.EndTime)
            {
                return Relation.OVERLAP;
            }

            if (first.StartTime < second.StartTime && first.EndTime == second.EndTime)
            {
                return Relation.FINISHBY;
            }

            if (first.StartTime < second.StartTime && first.EndTime > second.EndTime)
            {
                return Relation.CONTAIN;
            }

            if (first.StartTime == second.StartTime && first.EndTime < second.EndTime)
            {
                return Relation.STARTS;
            }

            if (first.StartTime == second.StartTime && first.EndTime == second.EndTime)
            {
                return Relation.EQUAL;
            }

            throw new InvalidOperationException("Should not reach here"); // Throw an exception for unexpected cases
        }
    }
}
