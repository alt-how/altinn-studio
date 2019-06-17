using System.Collections.Generic;
using System.Threading.Tasks;
using Altinn.Platform.Authorization.Services.Interface;
using Authorization.Interface.Models;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Platform.Authorization.Controllers
{
    /// <summary>
    /// Contains all actions related to the Actor model
    /// </summary>
    [Route("authorization/api/v1/actors")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly IActor _actorWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorsController"/> class
        /// </summary>
        public ActorsController(IActor actorWrapper)
        {
            _actorWrapper = actorWrapper;
        }

        [HttpGet]
        public async Task<ActionResult> Get(int userId)
        {
            List<Actor> actorList = await _actorWrapper.GetActors(userId);
            if (actorList == null || actorList.Count == 0)
            {
                return NotFound();
            }

            return Ok(actorList);
        }
    }
}
