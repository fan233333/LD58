using System;
using System.Collections;
using System.Collections.Generic;
using Kuchinashi.DataSystem;
using UnityEngine;
using UnityEngine.Video;

namespace Phosphorescence.Narration.Common
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Video Data", menuName = "Scriptable Objects/Video Data", order = 0)]
    public class VideoData : ScriptableObject, IHaveId
    {
        public string Id => id;
        public string id;
        public VideoClip videoClip;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public Vector3 scaleOffset = Vector3.one;
    }
}