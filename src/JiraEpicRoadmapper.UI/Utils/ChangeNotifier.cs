using System;

namespace JiraEpicRoadmapper.UI.Utils
{
    public static class ChangeNotifier
    {
        public static void Change<T>(ref T field, T value, Action changeTrigger) where T : class
        {
            if (ReferenceEquals(field, value))
                return;

            field = value;
            changeTrigger?.Invoke();
        }
    }
}
