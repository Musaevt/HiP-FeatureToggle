// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Clients.Models
{
    using System.Linq;

    public partial class Pagination
    {
        /// <summary>
        /// Initializes a new instance of the Pagination class.
        /// </summary>
        public Pagination() { }

        /// <summary>
        /// Initializes a new instance of the Pagination class.
        /// </summary>
        public Pagination(int? itemsCount = default(int?), int? totalItems = default(int?), int? page = default(int?), int? pageSize = default(int?), int? totalPages = default(int?))
        {
            ItemsCount = itemsCount;
            TotalItems = totalItems;
            Page = page;
            PageSize = pageSize;
            TotalPages = totalPages;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "itemsCount")]
        public int? ItemsCount { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "totalItems")]
        public int? TotalItems { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "page")]
        public int? Page { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "pageSize")]
        public int? PageSize { get; set; }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "totalPages")]
        public int? TotalPages { get; set; }

    }
}
