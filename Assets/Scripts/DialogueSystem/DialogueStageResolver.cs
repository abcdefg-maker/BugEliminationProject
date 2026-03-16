using System.Collections.Generic;

namespace BugElimination
{
    public interface IDialogueStage
    {
        string UnlockFlag { get; }
        bool RequireUnlock { get; }
        DialogueData Dialogue { get; }
    }

    public static class DialogueStageResolver
    {
        public static DialogueData Resolve<T>(List<T> stages) where T : IDialogueStage
        {
            DialogueData result = null;

            foreach (var stage in stages)
            {
                if (stage.RequireUnlock)
                {
                    if (GameStateManager.Instance != null &&
                        GameStateManager.Instance.CheckFlag(stage.UnlockFlag))
                    {
                        result = stage.Dialogue;
                    }
                }
                else
                {
                    result = stage.Dialogue;
                }
            }

            return result;
        }
    }
}
