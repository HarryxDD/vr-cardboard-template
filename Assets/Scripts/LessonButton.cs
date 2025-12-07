using UnityEngine;

// Attach this to a button GameObject that also uses your existing Interaction system
public class LessonButton : Interactive
{
    [Header("Lesson Flow")]
    public LessonFlowController controller;

    // Name of the method on LessonFlowController to call, e.g. "StartLesson", "GoToChromatic", "CancelLesson"
    public string actionName;

    public new void Interact()
    {
        if (controller != null && !string.IsNullOrEmpty(actionName))
        {
            controller.SendMessage(actionName, SendMessageOptions.DontRequireReceiver);
        }
    }
}
