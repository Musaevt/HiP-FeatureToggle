// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Clients.Models
{
    using System.Linq;

    public partial class Layer
    {
        /// <summary>
        /// Initializes a new instance of the Layer class.
        /// </summary>
        public Layer() { }

        /// <summary>
        /// Initializes a new instance of the Layer class.
        /// </summary>
        public Layer(int? id = default(int?), string name = default(string), System.Collections.Generic.IList<LayerRelationRule> relations = default(System.Collections.Generic.IList<LayerRelationRule>), System.Collections.Generic.IList<LayerRelationRule> incomingRelations = default(System.Collections.Generic.IList<LayerRelationRule>))
        {
            Id = id;
            Name = name;
            Relations = relations;
            IncomingRelations = incomingRelations;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "relations")]
        public System.Collections.Generic.IList<LayerRelationRule> Relations { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "incomingRelations")]
        public System.Collections.Generic.IList<LayerRelationRule> IncomingRelations { get; set; }

    }
}
