using System;
using Kuchinashi.Utils.Progressable;
using UnityEngine;
using UnityEngine.UI;

namespace Phosphorescence.Narration
{
    public abstract class NarrationComponent : MonoBehaviour
    {
        public CanvasGroupAlphaProgressable CanvasGroup;
        public bool IsComplete { get; protected set; }
        public bool IsSkipped { get; protected set; }
        public bool IsAuto { get; protected set; }
        public Action SkipAction;
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
    }
}