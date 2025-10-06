using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class RectPositionProgressable : Progressable
    {
        public RectTransform TargetRectTransform;

        public Vector2 StartPosition;
        public Vector2 EndPosition;

        private void Awake()
        {
            if (TargetRectTransform == null) TargetRectTransform = GetComponent<RectTransform>();
        }

        internal override void Update()
        {
            if (TargetRectTransform == null) return;
            base.Update();

            TargetRectTransform.anchoredPosition = Vector2.Lerp(StartPosition, EndPosition, evaluation);
        }
    }
}