using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class CreditsPanelController : MonoBehaviour
{
    [Header("Refs")]
    public RawImage rawImage;
    public Text nameText;
    public Text descriptionText;

    [Header("RectTransforms (final positions are where-ever you placed these)")]
    public RectTransform nameRectTransform;
    public RectTransform descriptionRectTransform;
    public RectTransform imageRectTransform;

    [Header("If your are showing multiple videos, put the rendering image here and it will be DoTweened")]
    public List<RectTransform> additionalImageRectTransforms = new List<RectTransform>();

    [Header("From Offsets (relative to final anchoredPosition)")]
    public Vector2 nameFromOffset = new Vector2(0f, 300f);          // your name from above
    public Vector2 descriptionFromOffset = new Vector2(0f, -300f);  // your role description from below
    public Vector2 imageFromOffset = new Vector2(-600f, 0f);        // your video from left

    [Header("Timings")]
    public float fadeInSeconds = 0.75f;
    public float slideSeconds = 0.6f;
    public float fadeOutSeconds = 0.35f;

    private CanvasGroup canvasGroup;

    private Vector2 nameTargetPosition;
    private Vector2 descriptionTargetPosition;
    private Vector2 imageTargetPosition;
    private readonly List<Vector2> additionalImageTargetPositions = new List<Vector2>();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Prepare(string displayName, string displayDescription, Texture targetTexture)
    {
        if (nameText != null) nameText.text = displayName;
        if (descriptionText != null) descriptionText.text = displayDescription;
        if (rawImage != null) rawImage.texture = targetTexture;

        CacheTargetPositions();

        canvasGroup.alpha = 0f;

        if (nameRectTransform != null)
            nameRectTransform.anchoredPosition = nameTargetPosition + nameFromOffset;

        if (descriptionRectTransform != null)
            descriptionRectTransform.anchoredPosition = descriptionTargetPosition + descriptionFromOffset;

        if (imageRectTransform != null)
            imageRectTransform.anchoredPosition = imageTargetPosition + imageFromOffset;

        int rectIndex = 0;
        while (rectIndex < additionalImageRectTransforms.Count)
        {
            RectTransform rectTransform = additionalImageRectTransforms[rectIndex];
            if (rectTransform != null)
                rectTransform.anchoredPosition = additionalImageTargetPositions[rectIndex] + imageFromOffset;
            rectIndex++;
        }
    }

    public Sequence AnimateIn()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Join(canvasGroup.DOFade(1f, fadeInSeconds).SetEase(Ease.OutQuad));

        if (nameRectTransform != null)
            sequence.Join(nameRectTransform.DOAnchorPos(nameTargetPosition, slideSeconds).SetEase(Ease.OutCubic));

        if (descriptionRectTransform != null)
            sequence.Join(descriptionRectTransform.DOAnchorPos(descriptionTargetPosition, slideSeconds).SetEase(Ease.OutCubic));

        if (imageRectTransform != null)
            sequence.Join(imageRectTransform.DOAnchorPos(imageTargetPosition, slideSeconds).SetEase(Ease.OutCubic));

        int rectIndex = 0;
        while (rectIndex < additionalImageRectTransforms.Count)
        {
            RectTransform rectTransform = additionalImageRectTransforms[rectIndex];
            if (rectTransform != null)
                sequence.Join(rectTransform.DOAnchorPos(additionalImageTargetPositions[rectIndex], slideSeconds).SetEase(Ease.OutCubic));
            rectIndex++;
        }

        return sequence;
    }

    public Sequence AnimateOut()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Join(canvasGroup.DOFade(0f, fadeOutSeconds).SetEase(Ease.InQuad));
        return sequence;
    }

    private void CacheTargetPositions()
    {
        if (nameRectTransform != null) nameTargetPosition = nameRectTransform.anchoredPosition;
        if (descriptionRectTransform != null) descriptionTargetPosition = descriptionRectTransform.anchoredPosition;
        if (imageRectTransform != null) imageTargetPosition = imageRectTransform.anchoredPosition;

        additionalImageTargetPositions.Clear();
        int rectIndex = 0;
        while (rectIndex < additionalImageRectTransforms.Count)
        {
            RectTransform rectTransform = additionalImageRectTransforms[rectIndex];
            if (rectTransform != null) additionalImageTargetPositions.Add(rectTransform.anchoredPosition);
            else additionalImageTargetPositions.Add(Vector2.zero);
            rectIndex++;
        }
    }
}