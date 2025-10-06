using System.Collections;
using UnityEngine;

namespace Kuchinashi.Utils
{
    public class FollowTransform : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;

        [Header("Additional Settings")]
        public bool followX = true;
        public bool followY = true;
        public bool followZ = true;

        public float thresholdDistance = 0.1f;

        [Header("Smooth Follow Settings")]
        public bool smoothFollow = false;
        public float smoothSpeed = 0.125f;
        private Coroutine m_SmoothFollowCoroutine;

        [Header("Proportional Settings")]
        public bool isProportional = false;
        public Vector3 proportion = Vector3.one;

        void FixedUpdate()
        {
            if (target == null) return;
            if (m_SmoothFollowCoroutine != null) return;

            Vector3 targetPosition = Vector3.Scale(target.position, isProportional ? proportion : Vector3.one) + offset;
            Vector3 currentPosition = transform.position;
            // var newPosition = smoothFollow ? Vector3.Lerp(currentPosition, targetPosition, smoothSpeed) : targetPosition;

            if (Vector3.Distance(currentPosition, targetPosition) > thresholdDistance && smoothFollow)
            {
                m_SmoothFollowCoroutine = StartCoroutine(SmoothFollowCoroutine());
            }
            else if (!smoothFollow)
            {
                transform.position = targetPosition;
            }
        }

        private IEnumerator SmoothFollowCoroutine()
        {
            Vector3 targetPosition = Vector3.Scale(target.position, isProportional ? proportion : Vector3.one) + offset;

            while (Mathf.Abs(transform.position.x - targetPosition.x) > 0.01f
                || Mathf.Abs(transform.position.y - targetPosition.y) > 0.01f
                || Mathf.Abs(transform.position.z - targetPosition.z) > 0.01f)
            {
                var newPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
                transform.position = new Vector3(
                    followX ? newPosition.x : transform.position.x,
                    followY ? newPosition.y : transform.position.y,
                    followZ ? newPosition.z : transform.position.z
                );
                yield return new WaitForFixedUpdate();

                targetPosition = Vector3.Scale(target.position, isProportional ? proportion : Vector3.one) + offset;
            }
            m_SmoothFollowCoroutine = null;
        }
    }
}
