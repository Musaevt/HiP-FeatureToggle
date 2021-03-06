// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Clients.Models
{
    using System.Linq;

    public partial class RelationFormModel
    {
        /// <summary>
        /// Initializes a new instance of the RelationFormModel class.
        /// </summary>
        public RelationFormModel() { }

        /// <summary>
        /// Initializes a new instance of the RelationFormModel class.
        /// </summary>
        public RelationFormModel(int? sourceId = default(int?), int? targetId = default(int?), string title = default(string), string arrowStyle = default(string), string color = default(string), string description = default(string))
        {
            SourceId = sourceId;
            TargetId = targetId;
            Title = title;
            ArrowStyle = arrowStyle;
            Color = color;
            Description = description;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "sourceId")]
        public int? SourceId { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "targetId")]
        public int? TargetId { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "arrowStyle")]
        public string ArrowStyle { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

    }
}
