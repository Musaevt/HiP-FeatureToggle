using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Rest
{
    public class FeatureGroupArgs
    {
        [Required]
        public string Name { get; set; }
        
        /// <summary>
        /// IDs of features that are enabled in this group.
        /// </summary>
        public IReadOnlyCollection<int> EnabledFeatures { get; set; }

        /// <summary>
        /// IDs of users that are assigned to this group.
        /// </summary>
        public IReadOnlyCollection<string> Members { get; set; }
    }
}