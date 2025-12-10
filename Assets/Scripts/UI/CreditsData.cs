using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

[Serializable]
public class AuthorCluster
{
    [Header("Your Name and Role")]
    public string authorName;
    [TextArea] public string description;

    [Header("Your Clips (used in order of the list)")]
    public List<VideoClip> clips = new List<VideoClip>();

    [Header("Your Panel")]
    public CreditsPanelController panel;

    [Header("Your Display Time (0 = displayed for the length of your clip or fallback)")]
    public float displayDuration;
}