namespace JiraEpicRoadmapper.UI.Models
{
    public class EpicVisualBlock
    {

        public EpicVisualBlock(EpicMetadata meta, int rowIndex)
        {
            RowIndex = rowIndex;
            Meta = meta;
        }

        public int RowIndex { get; }
        public EpicMetadata Meta { get; }
    }
}