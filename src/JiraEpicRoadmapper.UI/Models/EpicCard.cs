﻿namespace JiraEpicRoadmapper.UI.Models
{
    public class EpicCard
    {

        public EpicCard(EpicMetadata meta, int rowIndex)
        {
            RowIndex = rowIndex;
            Meta = meta;
        }

        public int StartIndex => Meta.Start.Index;
        public int EndIndex => Meta.End.Index;
        public int RowIndex { get; }
        public EpicMetadata Meta { get; }
    }
}