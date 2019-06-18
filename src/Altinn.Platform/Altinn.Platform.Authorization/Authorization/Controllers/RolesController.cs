using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altinn.Platform.Authorization.Services.Interface;
using Authorization.Interface.Models;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Platform.Authorization.Controllers
{
    /// <summary>
    /// Contains all actions related to the roles model
    /// </summary>
    [Route("authorization/api/v1/roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoles _rolesWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorsController"/> class
        /// </summary>
        public RolesController(IRoles rolesWrapper)
        {
            _rolesWrapper = rolesWrapper;
        }

        /// <summary>
        /// Get the decision point roles for the loggedin user for a selected party
        /// </summary>
        /// <param name="coveredByUserId">the logged in user id</param>
        /// <param name="offeredByPartyId">the partyid of the person/org the logged in user is representing</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Get(int coveredByUserId, int offeredByPartyId)
        {
            List<Role> actorList = await _rolesWrapper.GetDecisionPointRolesForUser(coveredByUserId, offeredByPartyId);
            if (actorList == null || actorList.Count == 0)
            {
                return NotFound();
            }

            return Ok(actorList);
        }
    }
}
