// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Clients.Models
{
    using System.Linq;

    public partial class TagFrequencyAnalyticsResult
    {
        /// <summary>
        /// Initializes a new instance of the TagFrequencyAnalyticsResult
        /// class.
        /// </summary>
        public TagFrequencyAnalyticsResult() { }

        /// <summary>
        /// Initializes a new instance of the TagFrequencyAnalyticsResult
        /// class.
        /// </summary>
        public TagFrequencyAnalyticsResult(System.Collections.Generic.IList<TagFrequency> tagFrequency = default(System.Collections.Generic.IList<TagFrequency>))
        {
            TagFrequency = tagFrequency;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "tagFrequency")]
        public System.Collections.Generic.IList<TagFrequency> TagFrequency { get; set; }

    }
}