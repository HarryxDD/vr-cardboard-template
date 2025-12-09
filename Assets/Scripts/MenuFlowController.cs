using UnityEngine;

public class MenuFlowController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;

    [Header("References")]
    public LessonFlowController lessonController;
    public NetBootstrap netBootstrap;

    private bool hasChosenPath = false;

    void Start()
    {
        SetActiveSafe(mainMenuPanel, true);

        if (netBootstrap != null)
        {
            netBootstrap.enabled = false;
        }

        if (lessonController != null)
        {
            lessonController.CancelLesson();
        }
    }

    void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null)
        {
            go.SetActive(active);
        }
    }

    public void StartWithInstructions()
    {
        if (hasChosenPath) return;
        hasChosenPath = true;

        Debug.Log("[MenuFlow] Starting with instructions (lesson flow)...");

        SetActiveSafe(mainMenuPanel, false);

        if (lessonController != null)
        {
            lessonController.StartLessonWithMultiplayerAfter(this);
        }
    }

    public void OnLessonComplete()
    {
        Debug.Log("[MenuFlow] Lesson complete! Now connecting to multiplayer...");
        ConnectToMultiplayer();
    }

    public void JoinMultiplayerNow()
    {
        if (hasChosenPath) return;
        hasChosenPath = true;

        Debug.Log("[MenuFlow] Joining multiplayer immediately...");

        SetActiveSafe(mainMenuPanel, false);

        ConnectToMultiplayer();
    }

    void ConnectToMultiplayer()
    {
        if (netBootstrap != null)
        {
            netBootstrap.enabled = true;
            netBootstrap.StartConnection();
        }
    }
}
