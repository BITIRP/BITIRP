using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BFS_Intersection
{
    internal class EntitiesDb
    {
        public List<Entity> Entities { get; set; }

        public EntitiesDb(List<Entity> entities)
        {
            Entities = entities;
        }

        public List<int> GetSymbols()
        {
            var symbols = new HashSet<int>();

            foreach (var entity in this.Entities)
            {
                foreach (var symbolicTimeInterval in entity.SymbolicTimeIntervals)
                {
                    symbols.Add(symbolicTimeInterval.Symbol);
                }
            }

            return symbols.ToList();
        }
    }

    internal static class EntitiesDbLoader
    {
        private const string StartToncepts = "startToncepts";
        private const string NumberOfEntitiesPattern = @"numberOfEntities\,(\d+)";
        private static readonly Regex SymbolicTimeIntervalRegex = new Regex(@"(\d+),(\d+),([a-zA-Z\d]+)"); // Improved symbol regex
        private static readonly Regex EntityRegex = new Regex(@"(\d+)\,(\d+)\;");
        private const int headerSize = 3;

        public static EntitiesDb Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Entities file not found: {filePath}");
            }

            List<Entity> entities = new List<Entity>();
            int symbolicTimeIntervalIndex = 0;

            List<string> lines = File.ReadAllLines(filePath).ToList();

            ValidateHeader(lines[1]);
            int numberOfEntities = ExtractNumberOfEntities(lines[2]);
            ValidateNumberOfEntities(lines.Skip(headerSize).Count() / 2, numberOfEntities);

            for (int i = 0; i < numberOfEntities; i++)
            {
                int entityIndex = ExtractEntityIndex(lines[headerSize + i * 2]);
                List<SymbolicTimeInterval> symbolicTimeIntervals = ExtractSymbolicTimeIntervals(lines[headerSize + i * 2 + 1]);

                entities.Add(new Entity(entityIndex, symbolicTimeIntervals));
                symbolicTimeIntervalIndex += symbolicTimeIntervals.Count;
            }

            return new EntitiesDb(entities);
        }

        private static void ValidateHeader(string header)
        {
            if (header != StartToncepts)
            {
                throw new InvalidDataException($"Invalid header: Expected '{StartToncepts}', found '{header}'");
            }
        }

        private static int ExtractNumberOfEntities(string line)
        {
            Match match = Regex.Match(line, NumberOfEntitiesPattern);
            if (!match.Success)
            {
                throw new InvalidDataException($"Invalid number of entities format: {line}");
            }

            return int.Parse(match.Groups[1].Value);
        }

        private static void ValidateNumberOfEntities(int actualCount, int expectedCount)
        {
            if (actualCount != expectedCount)
            {
                throw new InvalidDataException($"Number of entities mismatch. Expected: {expectedCount}, Actual: {actualCount}");
            }
        }

        private static int ExtractEntityIndex(string line)
        {
            Match match = EntityRegex.Match(line);
            if (!match.Success)
            {
                throw new InvalidDataException($"Invalid entity format: {line}");
            }

            return int.Parse(match.Groups[1].Value);
        }

        private static List<SymbolicTimeInterval> ExtractSymbolicTimeIntervals(string line)
        {
            List<SymbolicTimeInterval> symbolicTimeIntervals = new List<SymbolicTimeInterval>();
            int symbolicTimeIntervalIndex = 0;
            foreach (Match match in SymbolicTimeIntervalRegex.Matches(line))
            {
                symbolicTimeIntervals.Add(new SymbolicTimeInterval(
                    symbolicTimeIntervalIndex++,
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value)
                ));
            }

            return symbolicTimeIntervals;
        }
    }
}
