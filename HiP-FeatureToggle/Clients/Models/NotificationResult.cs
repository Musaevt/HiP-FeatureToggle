// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Clients.Models
{
    using System.Linq;

    public partial class NotificationResult
    {
        /// <summary>
        /// Initializes a new instance of the NotificationResult class.
        /// </summary>
        public NotificationResult() { }

        /// <summary>
        /// Initializes a new instance of the NotificationResult class.
        /// </summary>
        public NotificationResult(int? notificationId = default(int?), System.DateTime? timeStamp = default(System.DateTime?), UserResult updater = default(UserResult), string type = default(string), System.Collections.Generic.IList<object> data = default(System.Collections.Generic.IList<object>), bool? isRead = default(bool?))
        {
            NotificationId = notificationId;
            TimeStamp = timeStamp;
            Updater = updater;
            Type = type;
            Data = data;
            IsRead = isRead;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "notificationId")]
        public int? NotificationId { get; set; }

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
        [Newtonsoft.Json.JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "data")]
        public System.Collections.Generic.IList<object> Data { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "isRead")]
        public bool? IsRead { get; set; }

    }
}
