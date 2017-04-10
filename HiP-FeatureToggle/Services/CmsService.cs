using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Clients;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Clients.Models;
using System;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Services
{
    public class CmsService
    {
        private readonly HiPCMSAPI _client;

        public CmsService(IOptions<HiPCMSAPIConfig> config)
        {
            _client = new HiPCMSAPI(new Uri(config.Value.CMS_HOST));
        }

        public UserResult GetUser(string identity)
        {
            // Method is broken, we currently do not use the AutoRest-generated client for HiP-CmsWebService
            // (in the future, user roles will be stored in the new HiP-Auth service)

            // TODO: Some auth token is required here
            //                            \/
            var user = _client.ApiUserGet("", identity);
            return user;
        }

        public string GetUserRole(string identity)
        {
            // temporary, just for testing
            if (identity == "admin@hipapp.de")
                return "Administrator";
            else
                return "Student";
        }
    }
}
