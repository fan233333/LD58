using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class ScaleProgressable : Progressable
    {
        public Transform TargetTransform;

        public Vector3 StartScale;
        public Vector3 EndScale;

        private void Awake()
        {
            if (TargetTransform == null) TargetTransform = transform;
        }

        internal override void Update()
        {
            if (TargetTransform == null) return;
            base.Update();

            TargetTransform.localScale = Vector3.Lerp(StartScale, EndScale, evaluation);
        }
    }
}