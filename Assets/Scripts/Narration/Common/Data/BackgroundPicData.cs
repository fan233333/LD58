using System;
using System.Collections;
using System.Collections.Generic;
using Kuchinashi.DataSystem;
using UnityEngine;

namespace Phosphorescence.Narration.Common
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Background Picture Data", menuName = "Scriptable Objects/Background Picture Data", order = 0)]
    public class BackgroundPicData : ScriptableObject, IHaveId
    {
        public string Id => id;
        public string id;
        public Sprite sprite;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public Vector3 scaleOffset = Vector3.one;
    }
}