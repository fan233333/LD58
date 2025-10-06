using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class ImageColorProgressable : Progressable
    {
        public Image TargetImage;

        public Color StartColor = Color.white;
        public Color EndColor = Color.white;

        private void Awake()
        {
            if (TargetImage == null) TargetImage = TryGetComponent<Image>(out var image) ? image : null;
        }

        internal override void Update()
        {
            if (TargetImage == null) return;
            base.Update();

            TargetImage.color = Color.Lerp(StartColor, EndColor, evaluation);
        }
    }
}