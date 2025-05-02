using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountDownTimerController : MonoBehaviour
{
    public int countDownTime;
    public TextMeshProUGUI countDownDisplay;

    /*private void Start()
    {
        StartCoroutine(CountDownToStart());
    }*/

    public IEnumerator CountDownToStart()
    {
        Time.timeScale = 0f;

        while (countDownTime > 0)
        {
            countDownDisplay.text = countDownTime.ToString();

            yield return new WaitForSecondsRealtime(1f);

            countDownTime--;
        }

        countDownDisplay.text = "Go!";

        yield return new WaitForSecondsRealtime(1f);
        countDownDisplay.gameObject.SetActive(false);
        GameManager.Instance.UpdateGameState(GameState.StartWave);
        Time.timeScale = 1f;
    }
}
