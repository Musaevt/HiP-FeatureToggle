using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Managers;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Rest;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Services;
using PaderbornUniversity.SILab.Hip.Webservice;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Controllers
{
    /// <summary>
    /// Provides methods to add/modify/remove features.
    /// </summary>
    [Authorize]
    [Route("Api/[controller]")]
    public class FeaturesController : Controller
    {
        private readonly FeaturesManager _manager;
        private readonly CmsService _cmsService;

        private bool IsAdministrator => _cmsService.GetUserRole(User.Identity.GetUserIdentity()) == "Administrator";

        public FeaturesController(FeaturesManager manager, CmsService cmsService)
        {
            _manager = manager;
            _cmsService = cmsService;
        }

        /// <summary>
        /// Gets all features.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FeatureResult>), 200)]
        [ProducesResponseType(403)]
        public IActionResult GetAll()
        {
            if (!IsAdministrator)
                return Forbid();

            var features = _manager.GetAllFeatures(loadChildren: true, loadGroups: true);
            var results = features.ToList().Select(f => new FeatureResult(f));
            return Ok(results);
        }

        /// <summary>
        /// Gets a specific feature by ID.
        /// </summary>
        [HttpGet("{featureId}")]
        [ProducesResponseType(typeof(FeatureResult), 200)]
        [ProducesResponseType(403)]
        public IActionResult GetById(int featureId)
        {
            if (!IsAdministrator)
                return Forbid();

            var feature = _manager.GetFeature(featureId, loadChildren: true, loadGroups: true);

            if (feature == null)
                return NotFound();

            return Ok(new FeatureResult(feature));
        }

        /// <summary>
        /// Stores a new feature.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public IActionResult Create([FromBody]FeatureArgs args)
        {
            if (!IsAdministrator)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _manager.CreateFeature(args);
                return Ok();
            }
            catch (ResourceNotFoundException<Feature> e)
            {
                return StatusCode(422, e.Message); // invalid parent feature ID
            }
            catch (ArgumentException e)
            {
                return StatusCode(409, e.Message); // feature name already in use
            }
        }

        /// <summary>
        /// Deletes a feature. If the feature has children, these are made children of the deleted feature's parent.
        /// </summary>
        /// <param name="featureId"></param>
        /// <returns></returns>
        [HttpDelete("{featureId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int featureId)
        {
            if (!IsAdministrator)
                return Forbid();

            try
            {
                _manager.DeleteFeature(featureId);
                return Ok();
            }
            catch (ResourceNotFoundException e)
            {
                return NotFound(e.Message); // feature does not exist
            }
        }

        /// <summary>
        /// Updates a feature. If the reference to the parent is modified, this moves the whole subtree of features.
        /// </summary>
        [HttpPut("{featureId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public IActionResult Update(int featureId, [FromBody]FeatureArgs args)
        {
            if (!IsAdministrator)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _manager.UpdateFeature(featureId, args);
                return Ok();
            }
            catch (ArgumentException e)
            {
                return StatusCode(409, e.Message); // new feature name already in use
            }
            catch (ResourceNotFoundException<Feature> e) when ((int)e.Keys.FirstOrDefault() == featureId)
            {
                return NotFound(e.Message); // feature to be updated does not exist
            }
            catch (ResourceNotFoundException<Feature> e)
            {
                return StatusCode(422, e.Message); // referenced parent feature does not exist
            }
            catch (InvalidOperationException e)
            {
                return StatusCode(409, e.Message); // invalid parent modification
            }
        }

        /// <summary>
        /// Checks whether a specific feature is effectively enabled for the current user.
        /// This is the case if the user is assigned to a feature group in which the feature
        /// and all its ancestor features are enabled.
        /// </summary>
        [HttpGet("{featureId}/IsEnabled")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        public IActionResult IsFeatureEnabledForCurrentUser(int featureId)
        {
            var userId = User.Identity.IsAuthenticated ? User.Identity.GetUserIdentity() : null;

            try
            {
                var isEffectivelyEnabled = _manager.IsFeatureEffectivelyEnabledForUser(userId, featureId);
                return Ok(isEffectivelyEnabled);
            }
            catch (ResourceNotFoundException<Feature> e)
            {
                return NotFound(e.Message);
            }
        }

        /// <summary>
        /// Gets all features that are effectively enabled for the current user.
        /// These are all features X where X itself and all ancestor features of X are enabled in the
        /// group the user is assigned to.
        /// </summary>
        [HttpGet("IsEnabled")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<FeatureResult>), 200)]
        public IActionResult GetEnabledFeaturesForCurrentUser()
        {
            var userId = User.Identity.IsAuthenticated ? User.Identity.GetUserIdentity() : null;
            var features = _manager.GetEffectivelyEnabledFeaturesForUser(userId);
            return Ok(features.Select(f => new FeatureResult(f)));
        }
    }
}
