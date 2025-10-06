using System;
using System.Collections;
using System.Collections.Generic;
using Kuchinashi.DataSystem;
using UnityEngine;

namespace Phosphorescence.DataSystem
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Audio Data", menuName = "Scriptable Objects/Audio Data", order = 1)]
    public class AudioData : ScriptableObject, IHaveId
    {
        public string Id => id;
        public string id;

        public AudioType type;
        public AudioClip clip;

        [Header("Settings")]
        public bool isLoop;
        public float standardVolume = 0.8f;
        public float standardPitch = 1f;
    }

    public enum AudioType
    {
        Music,
        Voice,
        SFX
    }
}