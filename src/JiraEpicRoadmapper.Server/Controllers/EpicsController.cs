using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Client.Model;
using JiraEpicRoadmapper.Models;
using JiraEpicRoadmapper.Server.Clients;
using Microsoft.AspNetCore.Mvc;

namespace JiraEpicRoadmapper.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EpicsController : ControllerBase
    {
        private readonly IJiraClient _jiraClient;
        private readonly DateTime _today;

        public EpicsController(IJiraClient jiraClient)
        {
            _today = DateTimeOffset.UtcNow.Date;
            _jiraClient = jiraClient;
        }

        [HttpGet]
        public async Task<IEnumerable<Epic>> Get()
        {
            return await _jiraClient.GetEpics();
        }

        [HttpGet("{epicKey}/stats")]
        public async Task<EpicStats> Get(string epicKey)
        {
            return await _jiraClient.GetEpicStats(epicKey);
        }
    }
}
