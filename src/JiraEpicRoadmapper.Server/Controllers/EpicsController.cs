using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace JiraEpicRoadmapper.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EpicsController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            yield return "ok";
        }
    }
}
