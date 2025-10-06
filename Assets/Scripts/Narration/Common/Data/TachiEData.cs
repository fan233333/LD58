using System;
using System.Collections;
using System.Collections.Generic;
using Kuchinashi.DataSystem;
using UnityEngine;

namespace Phosphorescence.Narration.Common
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Tachi E Data", menuName = "Scriptable Objects/Tachi E Data", order = 1)]
    public class TachiEData : ScriptableObject, IHaveId
    {
        public string Id => id;
        public string id;

        public TachiEType type;

        public Sprite sprite;
        public AnimationClip animationClip;
        public bool isAnimationLoop = false;

        [Header("Left Offset Settings")]
        public Vector3 positionOffsetForLeft = new Vector3(0, -640, 0);
        public Vector3 rotationOffsetForLeft = Vector3.zero;
        public Vector3 scaleOffsetForLeft = Vector3.one;

        [Header("Right Offset Settings")]
        public Vector3 positionOffsetForRight = new Vector3(-706, -640, 0);
        public Vector3 rotationOffsetForRight = Vector3.zero;
        public Vector3 scaleOffsetForRight = new Vector3(-1f, 1f, 1);
    }

    public enum TachiEType
    {
        Static,
        Animation
    }
}