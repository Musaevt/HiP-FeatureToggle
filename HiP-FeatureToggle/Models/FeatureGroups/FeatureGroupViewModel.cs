using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Models.FeatureGroups
{
    public class FeatureGroupViewModel
    {
        [Required]
        public string Name { get; set; }
        
        /// <summary>
        /// IDs of features that are enabled in this group.
        /// </summary>
        public IList<int> EnabledFeatures { get; set; }

        /// <summary>
        /// IDs of users that are assigned to this group.
        /// </summary>
        public IList<string> Members { get; set; }
    }
}
