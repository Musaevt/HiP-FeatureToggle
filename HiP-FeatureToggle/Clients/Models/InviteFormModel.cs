// Code generated by Microsoft (R) AutoRest Code Generator 0.17.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Clients.Models
{
    using System.Linq;

    public partial class InviteFormModel
    {
        /// <summary>
        /// Initializes a new instance of the InviteFormModel class.
        /// </summary>
        public InviteFormModel() { }

        /// <summary>
        /// Initializes a new instance of the InviteFormModel class.
        /// </summary>
        public InviteFormModel(System.Collections.Generic.IList<string> emails)
        {
            Emails = emails;
        }

        /// <summary>
        /// </summary>
        [Newtonsoft.Json.JsonProperty(PropertyName = "emails")]
        public System.Collections.Generic.IList<string> Emails { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Emails == null)
            {
                throw new Microsoft.Rest.ValidationException(Microsoft.Rest.ValidationRules.CannotBeNull, "Emails");
            }
        }
    }
}
