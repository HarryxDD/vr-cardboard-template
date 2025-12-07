using UnityEngine;

public class LessonFlowController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject welcomePanel;
    public GameObject refractionPanel;
    public GameObject sphericalPanel;
    public GameObject chromaticPanel;
    public GameObject endPanel;

    void Start()
    {
        SetActiveSafe(welcomePanel, true);
        SetActiveSafe(refractionPanel, false);
        SetActiveSafe(sphericalPanel, false);
        SetActiveSafe(chromaticPanel, false);
        SetActiveSafe(endPanel, false);
    }

    void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null)
        {
            go.SetActive(active);
        }
    }

    public void StartLesson()
    {
        SetActiveSafe(welcomePanel, false);
        SetActiveSafe(refractionPanel, true);
    }

    public void GoToSpherical()
    {
        SetActiveSafe(refractionPanel, false);
        SetActiveSafe(sphericalPanel, true);
    }

    public void GoToChromatic()
    {
        SetActiveSafe(sphericalPanel, false);
        SetActiveSafe(chromaticPanel, true);
    }

    public void FinishLesson()
    {
        SetActiveSafe(chromaticPanel, false);
        SetActiveSafe(endPanel, true);
    }

    public void CancelLesson()
    {
        SetActiveSafe(welcomePanel, false);
        SetActiveSafe(refractionPanel, false);
        SetActiveSafe(sphericalPanel, false);
        SetActiveSafe(chromaticPanel, false);
        SetActiveSafe(endPanel, false);
    }

    public void RestartLesson()
    {
        SetActiveSafe(welcomePanel, true);
        SetActiveSafe(refractionPanel, false);
        SetActiveSafe(sphericalPanel, false);
        SetActiveSafe(chromaticPanel, false);
        SetActiveSafe(endPanel, false);
    }
}
