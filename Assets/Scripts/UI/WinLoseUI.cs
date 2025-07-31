using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class WinLoseUI : MonoBehaviour
{
    [Header("Win Lose UI")]
    [SerializeField] private TextMeshProUGUI youText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button actionButton;
    [SerializeField] private RectTransform youStartLeft;
    [SerializeField] private RectTransform resultStartRight;
    [SerializeField] private RectTransform youTargetPosition;
    [SerializeField] private RectTransform resultTargetPosition;
    [SerializeField] private Color winTextColor = Color.green;
    [SerializeField] private Color loseTextColor = Color.red;
    
    private bool isResultShown = false;
    
    public void VisualSuccessEffect()
    {
        if (isResultShown) return;
        StartCoroutine(ShowResultSequence("You", "Won!", isSuccess: true));
        SetupResultVisuals(true);
    }

    public void VisualFailEffect()
    {
        if (isResultShown) return;
        StartCoroutine(ShowResultSequence("Game", "Over!", isSuccess: false));
        SetupResultVisuals(false);
    }

    private IEnumerator ShowResultSequence(string youWord, string resultWord, bool isSuccess)
    {
        isResultShown = true;

        // Prepare UI
        youText.text = youWord;
        resultText.text = resultWord;
        youText.rectTransform.position = youStartLeft.position;
        resultText.rectTransform.position = resultStartRight.position;
        youText.gameObject.SetActive(true);
        resultText.gameObject.SetActive(true);
        actionButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f); // short start delay

        // Move "You" and "Win"/"Lost"
        Sequence moveSequence = DOTween.Sequence();
        moveSequence.Join(youText.rectTransform.DOMove(youTargetPosition.position, 1f).SetEase(Ease.OutQuad));
        moveSequence.Join(resultText.rectTransform.DOMove(resultTargetPosition.position, 1f).SetEase(Ease.OutQuad));

        yield return moveSequence.WaitForCompletion(); 

        // After text in place, show button
        actionButton.gameObject.SetActive(true);
        actionButton.transform.localScale = Vector3.zero;
        actionButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        actionButton.onClick.RemoveAllListeners();

        if (isSuccess)
        {
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Proceed to Next Level";
            actionButton.onClick.AddListener(() => SceneManager.LoadScene("Main"));
        }
        else
        {
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Restart";
            actionButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }
    }
    
    private void SetupResultVisuals(bool success)
    {
        if (success)
        {
            resultText.color = winTextColor;
            youText.color = winTextColor;
        }
        else
        {
            resultText.color = loseTextColor;
            youText.color = loseTextColor;
        }
    }
}
