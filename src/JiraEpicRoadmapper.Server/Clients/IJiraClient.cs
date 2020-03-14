﻿using System.Threading.Tasks;
using JiraEpicRoadmapper.Models;

namespace JiraEpicRoadmapper.Server.Clients
{
    public interface IJiraClient
    {
        Task<Epic[]> GetEpics();
        Task<TicketStats> GetEpicStats(string epicKey);
    }
}
