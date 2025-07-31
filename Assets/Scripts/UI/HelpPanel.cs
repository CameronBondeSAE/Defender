using UnityEngine;

public class HelpPanel : MonoBehaviour
{
    public GameObject tutorialPanel;
    public GameObject tutorialutton;
    
    public void HidePanel()
    {
        tutorialPanel.SetActive(false);
        tutorialutton.SetActive(true);
    }
    public void ShowPanel()
    {
        tutorialPanel.SetActive(true);
        tutorialutton.SetActive(false);
    }
}


