using System.ComponentModel.DataAnnotations;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Rest
{
    public class FeatureArgs
    {
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// ID of the parent feature, or null if this is a root feature.
        /// </summary>
        public int? Parent { get; set; }
    }
}
