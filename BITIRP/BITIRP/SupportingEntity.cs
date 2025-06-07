using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFS_Intersection
{
    internal class SupportingEntity
    {
        public int Index { get; set; }
        public List<SupportingInstance> SupportingInstances { get; set; }
        private Dictionary<string, List<SupportingInstance>> _supportingInstancesByExtendWithKey { get; set; }

        public SupportingEntity(int index)
        {
            Index = index;
            SupportingInstances = new List<SupportingInstance>();
            _supportingInstancesByExtendWithKey = new Dictionary<string, List<SupportingInstance>>();
        }

        public void AddSupportingInstance(SupportingInstance supportingInstance)
        {
            SupportingInstances.Add(supportingInstance);

            if (!_supportingInstancesByExtendWithKey.ContainsKey(supportingInstance.ExtendWithKey))
            {
                _supportingInstancesByExtendWithKey[supportingInstance.ExtendWithKey] = new List<SupportingInstance>();
            }

            _supportingInstancesByExtendWithKey[supportingInstance.ExtendWithKey].Add(supportingInstance);
        }

        public List<SupportingInstance> GetSupportingInstances(string extendWithKey)
        {
            return _supportingInstancesByExtendWithKey.TryGetValue(extendWithKey, out var instances) ? instances : new List<SupportingInstance>();
        }

        public override string ToString()
        {
            return string.Join(" ", SupportingInstances.Select(si => $"{Index} {si}"));
        }
    }
}
