using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class ObjectActiveProgressable : Progressable
    {
        [SerializeField] private GameObject TargetObject;

        [Header("Settings")]
        [Range(0f, 1f)] public float ActiveThreshold = 0.5f;

        internal override void Update()
        {
            if (TargetObject == null) return;
            base.Update();

            TargetObject.SetActive(evaluation > ActiveThreshold);
        }
    }
}