using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class SpriteRendererColorProgressable : Progressable
    {
        public SpriteRenderer TargetRenderer;

        public Color StartColor = Color.white;
        public Color EndColor = Color.white;

        private void Awake()
        {
            if (TargetRenderer == null) TargetRenderer = TryGetComponent<SpriteRenderer>(out var renderer) ? renderer : null;
        }

        internal override void Update()
        {
            if (TargetRenderer == null) return;
            base.Update();

            TargetRenderer.color = Color.Lerp(StartColor, EndColor, evaluation);
        }
    }
}