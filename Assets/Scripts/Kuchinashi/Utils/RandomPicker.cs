using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Kuchinashi.Utils
{
    public abstract class RandomPicker<T> : MonoBehaviour
    {
        public T[] Items = new T[0];

        public T Pick()
        {
            return Items[Random.Range(0, Items.Length)];
        }
    }
}