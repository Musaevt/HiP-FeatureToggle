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
    /// Provides methods to add/remove feature groups and to assign users to these groups.
    /// </summary>
    [Authorize]
    [Route("Api/[controller]")]
    public class FeatureGroupsController : Controller
    {
        private readonly FeatureGroupsManager _manager;
        private readonly CmsService _cmsService;

        private bool IsAdministrator => _cmsService.GetUserRole(User.Identity.GetUserIdentity()) == "Administrator";

        public FeatureGroupsController(FeatureGroupsManager manager, CmsService cmsService)
        {
            _manager = manager;
            _cmsService = cmsService;
        }

        /// <summary>
        /// Gets all feature groups.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FeatureGroupResult>), 200)]
        [ProducesResponseType(403)]
        public IActionResult GetAll()
        {
            if (!IsAdministrator)
                return Forbid();

            var groups = _manager.GetGroups(loadMembers: true, loadFeatures: true);
            var results = groups.ToList().Select(g => new FeatureGroupResult(g)); // note: ToList() is required here
            return Ok(results);
        }

        /// <summary>
        /// Gets a specific feature group by ID.
        /// </summary>
        [HttpGet("{groupId}")]
        [ProducesResponseType(typeof(FeatureGroupResult), 200)]
        [ProducesResponseType(403)]
        public IActionResult GetById(int groupId)
        {
            if (!IsAdministrator)
                return Forbid();

            var group = _manager.GetGroup(groupId, loadMembers: true, loadFeatures: true);

            if (group == null)
                return NotFound();

            return Ok(new FeatureGroupResult(group));
        }

        /// <summary>
        /// Stores a new feature group.
        /// </summary>
        /// <param name="groupArgs">Creation arguments</param>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public IActionResult Create([FromBody]FeatureGroupArgs groupArgs)
        {
            if (!IsAdministrator)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _manager.CreateGroup(groupArgs);
                return Ok();
            }
            catch (ResourceNotFoundException<Feature> e)
            {
                return StatusCode(422, e.Message); // invalid feature ID
            }
            catch (ArgumentException e)
            {
                return StatusCode(409, e.Message); // group with that name already exists
            }
        }

        /// <summary>
        /// Deletes a feature group. Members are moved to the default group.
        /// </summary>
        [HttpDelete("{groupId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public IActionResult Delete(int groupId)
        {
            if (!IsAdministrator)
                return Forbid();

            try
            {
                _manager.RemoveGroup(groupId);
            }
            catch (ResourceNotFoundException<FeatureGroup> e)
            {
                return NotFound(e.Message); // group does not exist
            }
            catch (InvalidOperationException e)
            {
                return StatusCode(409, e.Message); // tried to delete protected group
            }

            return Ok();
        }

        /// <summary>
        /// Updates a feature group.
        /// </summary>
        [HttpPut("{groupId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult Update(int groupId, [FromBody]FeatureGroupArgs groupArgs)
        {
            if (!IsAdministrator)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _manager.UpdateGroup(groupId, groupArgs);
                return Ok();
            }
            catch (ResourceNotFoundException<FeatureGroup> e)
            {
                return NotFound(e.Message); // invalid group ID
            }
            catch (ResourceNotFoundException<Feature> e)
            {
                return StatusCode(422, e.Message); // invalid feature ID
            }
            catch (InvalidOperationException e)
            {
                return StatusCode(409, e.Message); // tried to rename protected group
            }
            catch (ArgumentException e)
            {
                return StatusCode(409, e.Message); // new group name already in use
            }
        }

        /// <summary>
        /// Removes a user from its current feature group and assigns it to a new feature group.
        /// </summary>
        /// <returns></returns>
        [HttpPut("/Api/Users/{userId}/FeatureGroup/{groupId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult AssignMember(string userId, int groupId)
        {
            if (!IsAdministrator)
                return Forbid();

            try
            {
                _manager.MoveUserToGroup(userId, groupId);
            }
            catch (ResourceNotFoundException<FeatureGroup> e)
            {
                return NotFound(e.Message);
            }
            return Ok();
        }
    }
}
