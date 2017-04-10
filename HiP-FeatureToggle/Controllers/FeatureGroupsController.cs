using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Managers;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.FeatureGroups;
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
        /// <param name="groupVM">Creation arguments</param>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public IActionResult Create([FromBody]FeatureGroupViewModel groupVM)
        {
            if (!IsAdministrator)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newGroup = new FeatureGroup { Name = groupVM.Name };

                newGroup.EnabledFeatures = _manager.GetFeatures(groupVM.EnabledFeatures)
                    .Select(f => new FeatureToFeatureGroupMapping(f, newGroup))
                    .ToList();

                newGroup.Members = _manager.GetOrCreateUsers(groupVM.Members).ToList();

                _manager.AddGroup(newGroup);
                return Ok();
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Deletes a feature group. Members are moved to the default group.
        /// </summary>
        [HttpDelete("{groupId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int groupId)
        {
            if (!IsAdministrator)
                return Forbid();

            var success = _manager.RemoveGroup(groupId);

            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpPut("{groupId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public IActionResult Update(int groupId, [FromBody]FeatureGroupViewModel groupVM)
        {
            if (!IsAdministrator)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var features = _manager.GetFeatures(groupVM.EnabledFeatures);
                var members = _manager.GetOrCreateUsers(groupVM.Members);
                _manager.UpdateGroup(groupId, groupVM.Name, features, members);
                return Ok();
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
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

            var user = _manager.GetOrCreateUser(userId);
            var group = _manager.GetGroup(groupId, loadMembers: true);

            if (group == null)
                return NotFound($"There is no feature group with ID '{groupId}'");

            _manager.MoveUserToGroup(user, group);
            return Ok();
        }
    }
}
