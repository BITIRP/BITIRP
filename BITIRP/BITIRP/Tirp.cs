using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFS_Intersection
{
    internal static class TirpHelpers
    {
        public static string CalculateKey(List<int> symbols, List<Relation> relations)
        {
            if (symbols.Count * (symbols.Count - 1) / 2 != relations.Count)
            {
                Console.WriteLine(string.Join(", ", symbols));
                Console.WriteLine(string.Join(", ", relations));
                throw new ArgumentException("Invalid number of relations for the given symbols.");
            }

            return $"{string.Join("&", symbols)}^" +
                   $"{string.Join("&", relations)}";

        }
    }
    internal class Tirp
    {
        public List<int> Symbols { get; private set; }
        public List<Relation> Relations { get; private set; }
        public Dictionary<int, SupportingEntity> SupportingEntities { get; private set; }

        public string ExtendByKey { get; private set; }
        public string ExtendWithKey { get; private set; }

        public Tirp(
            List<int> symbols,
            List<Relation> relations,
            Dictionary<int, SupportingEntity> supportingEntities = null
        )
        {
            if (symbols.Count * (symbols.Count - 1) / 2 != relations.Count)
            {
                Console.WriteLine(string.Join(", ", symbols));
                Console.WriteLine(string.Join(", ", relations));
                throw new ArgumentException("Invalid number of relations for the given symbols.");
            }

            Symbols = symbols;
            Relations = relations;
            SupportingEntities = supportingEntities ?? new Dictionary<int, SupportingEntity>();

            // Calculate ExtendByKey and ExtendWithKey
            ExtendByKey = TirpHelpers.CalculateKey(Symbols.Skip(1).ToList(), GetExtendByKeyRelations());
            ExtendWithKey = TirpHelpers.CalculateKey(Symbols.Take(Symbols.Count - 1).ToList(), GetExtendWithRelations());
        }

        private List<Relation> GetExtendByKeyRelations()
        {
            List<Relation> extendByKeyRelations = new List<Relation>();
            List<int> skipIndices = new List<int>();

            int skipIndex = 0;
            for (int i = 0; i < Symbols.Count; i++)
            {
                skipIndex += i;
                skipIndices.Add(skipIndex);
            }

            for (int i = 0; i < Relations.Count; i++)
            {
                if (!skipIndices.Contains(i))
                {
                    extendByKeyRelations.Add(Relations[i]);
                }
            }

            return extendByKeyRelations;
        }

        private List<Relation> GetExtendWithRelations()
        {
            return Relations.Take(Relations.Count - (Symbols.Count - 1)).ToList();
        }

        public void AddSupportingEntity(SupportingEntity supportingEntity)
        {
            if (SupportingEntities.ContainsKey(supportingEntity.Index))
            {
                throw new ArgumentException($"Supporting entity with index {supportingEntity.Index} already exists.");
            }

            SupportingEntities[supportingEntity.Index] = supportingEntity;
        }

        public string shortString()
        {
            string symbolsString = string.Join("-", Symbols);
            string relationsString = string.Join(".", Relations.Select(r => RelationHelper.AsString(r)));
            string supportString = SupportingEntities.Count.ToString();
            return $"{symbolsString} {relationsString} {supportString}";
        }

        public override string ToString()
        {
            string symbolsString = string.Join("-", Symbols);
            string relationsString = string.Join(".", Relations.Select(r => RelationHelper.AsString(r)));
            string supportString = SupportingEntities.Count.ToString();
            string supportingEntitiesString = string.Join(" ", SupportingEntities.Values.Select(se => se.ToString()));

            return $"{Symbols.Count} {symbolsString} {relationsString} {supportString} {supportingEntitiesString}";
        }
    }
    }
