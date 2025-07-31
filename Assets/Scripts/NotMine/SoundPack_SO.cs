using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundPack_SO", menuName = "Defender/SoundPack", order = 1)]
public class SoundPack_SO : ScriptableObject
{
    public List<AudioClip> clips;
}