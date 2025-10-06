using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class ProgressableGroup : Progressable
    {
        public List<Progressable> Progressables = new();

        internal override void Update()
        {
            base.Update();

            foreach (var progressable in Progressables)
            {
                if (progressable == null) continue;
                progressable.Progress = evaluation;
                progressable.Update();
            }
        }
    }
}