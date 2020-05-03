﻿namespace JiraEpicRoadmapper.Contracts
{
    public class Epic
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Project { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public string StatusCategory { get; set; }
    }
}
