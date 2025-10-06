using System;
using UnityEngine;

[DisallowMultipleComponent]
public class IceOre : MonoBehaviour
{
    [Header("仅在Tag == ReadyToFall时生效")]
    [SerializeField, Tooltip("缩小至多少比例后销毁")]
    private float minScale = 0.1f;

    [SerializeField, Tooltip("缩小速度(每秒比例变化)")]
    private float shrinkSpeed = 0.5f;

    [SerializeField, Tooltip("可选：消失时播放的粒子特效")]
    private ParticleSystem disappearEffect;

    private bool isShrinking = false;

    private void Update()
    {
        // 检查当前是否标记为 ReadyToFall
        if (CompareTag("ReadyToFall"))
        {
            // 防止重复启动
            if (!isShrinking)
                isShrinking = true;
        }

        if (isShrinking)
        {
            // 逐渐缩小
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                Vector3.zero,
                shrinkSpeed * Time.deltaTime);

            // 达到阈值后销毁
            if (transform.localScale.magnitude <= minScale)
            {
                TriggerEffect();
                Destroy(gameObject);
            }
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        var otherName = other.gameObject.name;
        Debug.Log(otherName);
        if (CompareTag("Collectible"))
            if (otherName == "IceMap")
            {
                if (isShrinking)
                    isShrinking = false;
            }
            else if (otherName == "GrassMap" || otherName == "SandMap" || otherName == "LavaMap")
            {
                if (!isShrinking)
                    isShrinking = true;
            }
    }

    private void TriggerEffect()
    {
        if (disappearEffect != null)
        {
            var ps = Instantiate(disappearEffect, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetimeMultiplier);
        }
    }
}