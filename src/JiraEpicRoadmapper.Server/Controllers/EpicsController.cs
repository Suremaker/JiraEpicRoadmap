using System.Collections.Generic;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.Server.Providers;
using Microsoft.AspNetCore.Mvc;

namespace JiraEpicRoadmapper.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EpicsController : ControllerBase
    {
        private readonly IEpicProvider _epicProvider;

        public EpicsController(IEpicProvider epicProvider)
        {
            _epicProvider = epicProvider;
        }

        [HttpGet]
        public async Task<IEnumerable<Epic>> Get()
        {
            return await _epicProvider.GetEpics();
        }
    }
}
