using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Rest
{
    public class FeatureGroupResult
    {
        public int Id { get; }

        public string Name { get; }

        public IReadOnlyCollection<string> Members { get; }

        public IReadOnlyCollection<int> EnabledFeatures { get; }

        public FeatureGroupResult(FeatureGroup group)
        {
            Id = group.Id;
            Name = group.Name;
            EnabledFeatures = group.EnabledFeatures.Select(f => f.FeatureId).ToList();
            Members = group.Members.Select(m => m.Id).ToList();
        }
    }
}
