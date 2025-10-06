using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RopeNodeDurability : MonoBehaviour
{
    [Header("初始耐久度")]
    public float maxDurability = 100f;

    [HideInInspector] public float currentDurability;
    [HideInInspector] public bool broken = false;

    public float damagePerSecond = 40f;
    public float healPerSecond = 10f;
    private Rigidbody2D rb;
    private RopeController ropeController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentDurability = maxDurability;
        ropeController = GetComponentInParent<RopeController>();
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (broken) return;
        if (other.gameObject.TryGetComponent<CollectibleItem>(out var obj))
            if ((int)obj.itemType == 5)
                TakeDamage(damagePerSecond * Time.deltaTime);
    }

    private void LateUpdate()
    {
        TakeDamage(-healPerSecond * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        if (broken) return;

        currentDurability -= amount;
        currentDurability = Mathf.Clamp(currentDurability, 0, maxDurability);

        if (currentDurability <= 0)
        {
            Break();
        }
    }

    public void Break()
    {
        broken = true;
        
        if (ropeController != null)
        {
            if (ropeController.SplitRopeAtNode(this))
                if (GetComponent<SpringJoint2D>() != null)
                {
                    Debug.Log(name);
                    Destroy(GetComponent<SpringJoint2D>()); 
                }
        }
    }

    public float GetDurabilityRatio()
    {
        return Mathf.Clamp01(currentDurability / maxDurability);
    }
}