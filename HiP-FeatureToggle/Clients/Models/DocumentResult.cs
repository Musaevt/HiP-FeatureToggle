// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Clients.Models
{
    using System.Linq;

    public partial class DocumentResult
    {
        /// <summary>
        /// Initializes a new instance of the DocumentResult class.
        /// </summary>
        public DocumentResult() { }

        /// <summary>
        /// Initializes a new instance of the DocumentResult class.
        /// </summary>
        public DocumentResult(System.DateTime? timeStamp = default(System.DateTime?), UserResult updater = default(UserResult), string content = default(string))
        {
            TimeStamp = timeStamp;
            Updater = updater;
            Content = content;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "timeStamp")]
        public System.DateTime? TimeStamp { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "updater")]
        public UserResult Updater { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

    }
}