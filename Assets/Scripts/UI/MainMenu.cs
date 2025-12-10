using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [Header("Words")]
    public Transform[] wordTargets = new Transform[4]; // target transforms
    public RectTransform[] wordObjects = new RectTransform[4]; // words

    [Header("Start Button")]
    public GameObject startButton;
    public float bounceHeight = 10f;
    public float bounceDuration = 1f;
    public string sceneToLoad = "current";
    
    [Header("Fake Loading")]
    public GameObject loadingUI;           
    public Slider loadingSlider;          
    public float fakeLoadingDuration = 1.5f;

    public GameObject menuUI;

    private void Start()
    {
        startButton.SetActive(false);
        loadingUI.SetActive(false);
        StartCoroutine(AnimateWordsSequentially());
    }

    private IEnumerator AnimateWordsSequentially()
    {
	    // CAM: I moved this to before the sequence starts, instead of waiting, for quicker testing basically
	    ActivateStartButton();

	    for (int i = 0; i < wordObjects.Length; i++)
        {
            RectTransform word = wordObjects[i];
            Transform target = wordTargets[i];

            Tween tween = word.DOMove(target.position, .5f)
                .SetEase(Ease.OutCubic); // Ease.OutCubic: start fast then slow down near finish

            yield return tween.WaitForCompletion();
        }

    }

    private void ActivateStartButton()
    {
        startButton.SetActive(true);
        // bounce button :)
        RectTransform buttonRect = startButton.GetComponent<RectTransform>();
        buttonRect.DOAnchorPosY(buttonRect.anchoredPosition.y + bounceHeight, bounceDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    public void StartGame()
    {
	    menuUI.SetActive(false);
	    
	    // HACK: Cam put this in
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
    }
    
    public void OnStartButtonPressed()
    {
        startButton.SetActive(false);
        loadingUI.SetActive(true);
        loadingSlider.value = 0f;
        StartCoroutine(FillFakeLoadingBar());
    }

    private IEnumerator FillFakeLoadingBar()
    {
        loadingSlider.value = 0f;

        // Smoothly tween from 0 to 100 over duration
        Tween fillTween = DOTween.To(
                () => loadingSlider.value,              // getter: current value of slider
                x => loadingSlider.value = x,           // setter: update slider value
                100f,                                   // target value: 100
                fakeLoadingDuration                     // Duration: how long it should take
            )
            .SetEase(Ease.Linear); // Ease.linear: smooth and steady increase

        yield return fillTween.WaitForCompletion();
        OnLoadingComplete();
    }
    private void OnLoadingComplete()
    {
        StartGame();
    }
}
