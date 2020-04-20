namespace JiraEpicRoadmapper.Client.Model
{
    public interface IViewOptions
    {
        bool CompactView { get; }
        bool HideClosedEpics { get; }
        bool HideUnplannedEpics { get; }
    }
}