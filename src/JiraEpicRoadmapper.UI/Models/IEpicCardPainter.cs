namespace JiraEpicRoadmapper.UI.Models
{
    public interface IEpicCardPainter
    {
        string GetColor(EpicVisualBlock block);
    }
}
