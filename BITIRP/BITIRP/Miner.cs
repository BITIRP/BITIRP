using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFS_Intersection
{
    internal class Miner
    {
        private const int INITIAL_DEPTH = 2;
        private const int INITIAL_JOIN_DEPTH = INITIAL_DEPTH + 1;
        private readonly EntitiesDb _entitiesDb;
        private readonly List<int> _symbols;
        private readonly int _maxDepth;
        private readonly int _minSupport;
        private readonly int _maxGap;
        private int _currentDepth;
        private int _frequentOneSizedCount; //Patch

        private Dictionary<int, List<Tirp>> _tirpsByDepth;
        private Dictionary<string, Tirp> _tirpsByKey;
        private Dictionary<string, List<Tirp>> _tirpsByExtendWithKey;
        private Dictionary<Tuple<int, int>, Relation> _symbolicTimeIntervalsRelations;

        public Miner(EntitiesDb entitiesDb, List<int> symbols, int maxDepth, int minSupport, int maxGap)
        {
            this._entitiesDb = entitiesDb;
            this._symbols = symbols;
            this._maxDepth = maxDepth;
            this._minSupport = minSupport;
            this._maxGap = maxGap;

            this._currentDepth = INITIAL_DEPTH;

            this._tirpsByDepth = new Dictionary<int, List<Tirp>>();
            this._tirpsByKey = new Dictionary<string, Tirp>();
            this._tirpsByExtendWithKey = new Dictionary<string, List<Tirp>>();
            this._symbolicTimeIntervalsRelations = new Dictionary<Tuple<int, int>, Relation>();

            //Patch
            this._frequentOneSizedCount = 0;
        }

        public void Run()
        {
            pruneInfrequentSymbols();
            MinePairs();
            this._currentDepth = INITIAL_JOIN_DEPTH;
            MineDepth();
            Console.WriteLine("Total TIRPs: {0}", _tirpsByDepth.Values.Sum(list => list.Count) + this._frequentOneSizedCount);
        }

        private void pruneInfrequentSymbols()
        {
            Dictionary<int, HashSet<int>> symbolsToEntities = new Dictionary<int, HashSet<int>>();
            foreach (Entity entity in _entitiesDb.Entities)
            {
                foreach (SymbolicTimeInterval symbolicTimeInterval in entity.SymbolicTimeIntervals)
                {
                    if (!symbolsToEntities.ContainsKey(symbolicTimeInterval.Symbol))
                    {
                        symbolsToEntities[symbolicTimeInterval.Symbol] = new HashSet<int>();
                    }

                    HashSet<int> symbolEntities = symbolsToEntities[symbolicTimeInterval.Symbol];
                    symbolEntities.Add(entity.Index); // Fast add, no need to check Contains, as it is handled by HashSet
                }
            }

            HashSet<int> frequentSymbols = symbolsToEntities.Where(kvp => kvp.Value.Count >= _minSupport).Select(kvp => kvp.Key).ToHashSet();
            
            foreach (Entity entity in _entitiesDb.Entities)
            {
                entity.SymbolicTimeIntervals = entity.SymbolicTimeIntervals.Where(symbolicTimeInterval => frequentSymbols.Contains(symbolicTimeInterval.Symbol)).ToList();
            }

            this._frequentOneSizedCount = frequentSymbols.Count;
            Console.WriteLine("Depth 1 " + frequentSymbols.Count);
            
        }

        private void MinePairs()
        {
            foreach (Entity entity in _entitiesDb.Entities)
            {
                SingleEntityPairs(entity);
            }

            List<Tirp> frequentTwoSized = new List<Tirp>();
            foreach (Tirp tirp in _tirpsByKey.Values)
            {
                if (tirp.SupportingEntities.Count >= _minSupport)
                {
                    frequentTwoSized.Add(tirp);
                }
            }

            _tirpsByDepth[INITIAL_DEPTH] = frequentTwoSized;
            Prune();
        }

        private void SingleEntityPairs(Entity entity)
        {
            for (int i = 0; i < entity.SymbolicTimeIntervals.Count; i++)
            {
                var first = entity.SymbolicTimeIntervals[i];
                for (int j = 0; j < entity.SymbolicTimeIntervals.Count; j++)
                {
                    var second = entity.SymbolicTimeIntervals[j];
                    if (first.Index == second.Index)
                    {
                        continue;
                    }

                    if (!RelationHelper.IsLexicographicallyValid(first, second))
                    {
                        continue;
                    }
                    var relation = RelationHelper.CheckRelation(first, second, _maxGap);

                    if (relation == Relation.NOT_DEFINED)
                    {
                        continue;
                    }

                    var tirpKey = TirpHelpers.CalculateKey(
                        new List<int> { first.Symbol, second.Symbol },
                        new List<Relation> { relation }
                    );

                    if (!_tirpsByKey.TryGetValue(tirpKey, out Tirp tirp))
                    {
                        tirp = new Tirp(
                            new List<int> { first.Symbol, second.Symbol },
                            new List<Relation> { relation },
                            new Dictionary<int, SupportingEntity>()
                        );
                        _tirpsByKey[tirpKey] = tirp;
                    }

                    if (!tirp.SupportingEntities.TryGetValue(entity.Index, out SupportingEntity supportingEntity))
                    {
                        supportingEntity = new SupportingEntity(entity.Index);
                        tirp.SupportingEntities[entity.Index] = supportingEntity;
                    }

                    supportingEntity.AddSupportingInstance(
                        new SupportingInstance(new List<SymbolicTimeInterval> { first, second })
                    );
                }
            }
        }

        private void PruneExtendWithKeys()
        {
            this._tirpsByExtendWithKey = new Dictionary<string, List<Tirp>>();

            foreach (var tirp in _tirpsByDepth[_currentDepth])
            {
                if (!_tirpsByExtendWithKey.TryGetValue(tirp.ExtendWithKey, out var tirps))
                {
                    tirps = new List<Tirp>();
                    _tirpsByExtendWithKey[tirp.ExtendWithKey] = tirps;
                }

                tirps.Add(tirp);
            }
        }

        private void Prune()
        {
            var frequentTirps = _tirpsByDepth[_currentDepth].Where(tirp => tirp.SupportingEntities.Count  >= _minSupport).ToList();
            Console.WriteLine($"Depth {_currentDepth}, {frequentTirps.Count} / {_tirpsByDepth[_currentDepth].Count}");
            _tirpsByDepth[_currentDepth] = frequentTirps;

            PruneExtendWithKeys();
        }

        private void MineDepth()
        {
            while (this._currentDepth <= this._maxDepth)
            {
                if (!this._tirpsByDepth[this._currentDepth - 1].Any())
                {
                    return;
                }
                this.MineSingleDepth();
                this.Prune();

                this._currentDepth++;
            }
        }

        private void MineSingleDepth()
        {
            List<Tirp> currentDepthTirps = new List<Tirp>();
            foreach (var tirp in this._tirpsByDepth[this._currentDepth - 1])
            {
                currentDepthTirps.AddRange(this.ExtendTirp(tirp));
            }
            this._tirpsByDepth[this._currentDepth] = currentDepthTirps;

        }

        private List<Tirp> ExtendTirp(Tirp tirp)
        {
            List<Tirp> extensionTirps = new List<Tirp>();
            if (!this._tirpsByExtendWithKey.ContainsKey(tirp.ExtendByKey))
            {
                return extensionTirps;
            }

            List<Tirp> intersectingTirps = this._tirpsByExtendWithKey[tirp.ExtendByKey];

            foreach (var intersectingTirp in intersectingTirps)
            {
                var intersectionTirps = this.Intersect(tirp, intersectingTirp);

                var intersectionFrequentTirps = new List<Tirp>();
                foreach (Tirp intersectionTirp in intersectionTirps)
                {
                    if (intersectionTirp.SupportingEntities.Count >= _minSupport)
                    {
                        intersectionFrequentTirps.Add(intersectionTirp);
                    }
                }
                extensionTirps.AddRange(intersectionFrequentTirps);
            }

            return extensionTirps;
        }

        public List<Tirp> Intersect(Tirp first, Tirp second)
        {

            var intersectionTirps = new Dictionary<Relation, Tirp>();
            
            var intersectionEntities = first.SupportingEntities.Keys.Intersect(second.SupportingEntities.Keys);
            if (intersectionEntities.Count() < this._minSupport)
            {
                return new List<Tirp>();
            }
            
            HashSet<Relation> validRelations = new HashSet<Relation>();
            foreach (Relation relation in Enum.GetValues(typeof(Relation)))
            {
                string twoSizedTirpKey = TirpHelpers.CalculateKey(new List<int> { first.Symbols.First(), second.Symbols.Last() }, new List<Relation> { relation });
                Tirp twoSizedTirp;
                _tirpsByKey.TryGetValue(twoSizedTirpKey, out twoSizedTirp);
                if (twoSizedTirp == null || twoSizedTirp.SupportingEntities.Keys.Intersect(intersectionEntities).Count() < this._minSupport)
                {
                    continue;
                }
                validRelations.Add(relation);
            }

            foreach (SupportingEntity firstSupportingEntity in first.SupportingEntities.Values)
            {
                if (!second.SupportingEntities.ContainsKey(firstSupportingEntity.Index))
                {
                    continue;
                }
                SupportingEntity secondSupportingEntity = second.SupportingEntities[firstSupportingEntity.Index];

                var supportingEntityByRelation = new Dictionary<Relation, SupportingEntity>();

                foreach (SupportingInstance firstSupportingInstance in firstSupportingEntity.SupportingInstances)
                {
                    var secondSupportingInstances = secondSupportingEntity.GetSupportingInstances(firstSupportingInstance.ExtendByKey);

                    if (secondSupportingInstances.Count == 0)
                    {
                        continue;
                    }
                    foreach (SupportingInstance secondSupportingInstance in secondSupportingInstances)
                    {
                        var firstSymbolicTimeInterval = firstSupportingInstance.SymbolicTimeIntervals[0];
                        var lastSymbolicTimeInterval = secondSupportingInstance.SymbolicTimeIntervals[^1];

                        Relation relation = RelationHelper.CheckRelation(firstSymbolicTimeInterval, lastSymbolicTimeInterval, this._maxGap);

                        if (relation == Relation.NOT_DEFINED || !validRelations.Contains(relation))
                        {
                            continue;
                        }

                        if (!supportingEntityByRelation.ContainsKey(relation))
                        {
                            supportingEntityByRelation[relation] = new SupportingEntity(
                                index: firstSupportingEntity.Index
                            );
                        }
                        var supportingEntity = supportingEntityByRelation[relation];

                        var supportingInstance = new SupportingInstance(
                            symbolicTimeIntervals: firstSupportingInstance.SymbolicTimeIntervals.Concat(new[] { lastSymbolicTimeInterval }).ToList()
                        );
                        supportingEntity.AddSupportingInstance(supportingInstance);
                    }
                }

                foreach (var (relation, supportingEntity) in supportingEntityByRelation)
                {
                    if (!intersectionTirps.ContainsKey(relation))
                    {
                        intersectionTirps[relation] = new Tirp(
                            symbols: first.Symbols.Concat(second.Symbols.Skip(second.Symbols.Count - 1)).ToList(),
                            relations: first.Relations.Concat(new List<Relation> { relation }).Concat(second.Relations.Skip(second.Relations.Count == 1? 0: second.Relations.Count - (second.Symbols.Count - 1))).ToList(),
                            supportingEntities: new Dictionary<int, SupportingEntity>()
                        );
                    }
                    var tirp = intersectionTirps[relation];
                    tirp.AddSupportingEntity(supportingEntity);
                }
            }

            return intersectionTirps.Values.ToList();
        }
    }
}
