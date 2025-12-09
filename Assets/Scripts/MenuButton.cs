using UnityEngine;

// Attach this to menu buttons (main menu choices)
public class MenuButton : Interactive
{
    [Header("Menu Flow")]
    public MenuFlowController menuController;

    // Name of the method on MenuFlowController to call, e.g. "StartWithInstructions", "JoinMultiplayerNow"
    public string actionName;

    public new void Interact()
    {
        if (menuController != null && !string.IsNullOrEmpty(actionName))
        {
            menuController.SendMessage(actionName, SendMessageOptions.DontRequireReceiver);
        }
    }
}
