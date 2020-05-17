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

        [HttpGet("{epicKey}/stats")]
        public async Task<EpicStats> Get(string epicKey)
        {
            return await _epicProvider.GetEpicStats(epicKey);
        }

        [HttpPost("{epicKey}/meta")]
        public async Task<Epic> UpdateEpic(string epicKey, EpicMeta meta)
        {
            return await _epicProvider.UpdateEpic(epicKey, meta);
        }
    }
}
