using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;
using UnityEngine.Serialization;

public class CreditsSequenceController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RenderTexture sharedRenderTexture;

    [Header("Authors in order")]
    public List<AuthorCluster> authors = new List<AuthorCluster>();

    [Header("Timings")]
    public float backupDisplayDuration = 5f;     // used if you didn't have clip and didn't put in a display duration in SO
    public float initialDelaySeconds = 0.5f;
    public float delayBetweenAuthorsSeconds = 0.3f;

    [Header("Auto start")]
    public bool playOnStart = true;

    private void Start()
    {
        if (playOnStart)
            Begin();
    }

    public void Begin()
    {
        StopAllCoroutines();
        DOTween.KillAll(false);
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = sharedRenderTexture;
        }

        DeactivateAllPanels();
        yield return new WaitForSeconds(initialDelaySeconds);

        int authorIndex = 0;
        while (authorIndex < authors.Count)
        {
            AuthorCluster authorCluster = authors[authorIndex];
            CreditsPanelController panel = authorCluster != null ? authorCluster.panel : null;

            if (panel != null)
            {
                SetPanelActive(panel, true);
                BringPanelToFront(panel);
                yield return null;

                string displayName = authorCluster.authorName != null ? authorCluster.authorName : "";
                string displayDescription = authorCluster.description != null ? authorCluster.description : "";
                Texture targetTexture = sharedRenderTexture != null ? sharedRenderTexture : null;

                panel.Prepare(displayName, displayDescription, targetTexture);
                
                float totalPlannedSeconds = 0f;
                bool hasAnyClip = authorCluster.clips != null && authorCluster.clips.Count > 0;

                if (authorCluster.displayDuration > 0f)
                {
                    totalPlannedSeconds = authorCluster.displayDuration;
                }
                else if (hasAnyClip)
                {
                    int clipIndexForSum = 0;
                    while (clipIndexForSum < authorCluster.clips.Count)
                    {
                        VideoClip clipForSum = authorCluster.clips[clipIndexForSum];
                        float clipSeconds = (clipForSum != null && clipForSum.length > 0.0) ? (float)clipForSum.length : backupDisplayDuration;
                        totalPlannedSeconds += clipSeconds;
                        clipIndexForSum++;
                    }
                }
                else
                {
                    totalPlannedSeconds = backupDisplayDuration;
                }
                Sequence animateInSequence = panel.AnimateIn();
                yield return animateInSequence.WaitForCompletion();
                float elapsedSeconds = 0f;

                if (hasAnyClip)
                {
                    int clipIndex = 0;
                    while (clipIndex < authorCluster.clips.Count && elapsedSeconds < totalPlannedSeconds)
                    {
                        VideoClip selectedClip = authorCluster.clips[clipIndex];

                        float thisClipDuration = backupDisplayDuration;
                        if (selectedClip != null && selectedClip.length > 0.0)
                            thisClipDuration = (float)selectedClip.length;
                        float remainingSeconds = totalPlannedSeconds - elapsedSeconds;
                        float secondsToPlayThisClip = thisClipDuration > remainingSeconds ? remainingSeconds : thisClipDuration;

                        if (videoPlayer != null && selectedClip != null && secondsToPlayThisClip > 0f)
                        {
                            videoPlayer.clip = selectedClip;
                            videoPlayer.Play();
                            yield return new WaitForSeconds(secondsToPlayThisClip);
                            videoPlayer.Stop();
                        }
                        else
                        {
                            if (secondsToPlayThisClip > 0f)
                                yield return new WaitForSeconds(secondsToPlayThisClip);
                        }

                        elapsedSeconds += secondsToPlayThisClip;
                        clipIndex++;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(totalPlannedSeconds);
                }

                if (videoPlayer != null)
                    videoPlayer.Stop();

                Sequence animateOutSequence = panel.AnimateOut();
                yield return animateOutSequence.WaitForCompletion();
                SetPanelActive(panel, false);
                yield return new WaitForSeconds(delayBetweenAuthorsSeconds);
            }
            authorIndex++;
        }
    }
    
    private void DeactivateAllPanels()
    {
        int i = 0;
        while (i < authors.Count)
        {
            CreditsPanelController panel = authors[i] != null ? authors[i].panel : null;
            if (panel != null && panel.gameObject.activeSelf)
                panel.gameObject.SetActive(false);
            i++;
        }
    }

    private void SetPanelActive(CreditsPanelController panel, bool isActive)
    {
        if (panel != null && panel.gameObject.activeSelf != isActive)
            panel.gameObject.SetActive(isActive);
    }

    private void BringPanelToFront(CreditsPanelController panel)
    {
        if (panel != null)
            panel.transform.SetAsLastSibling(); 
    }
}

