using System;
using System.Collections.Generic;
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
    public ParticleSystem particles;
    
    [Header("向上发射设置")]
    public float upwardSpeed = 0.5f;           // 向上速度（单位/秒）
    public bool keepWorldUp = true;            // 是否强制世界向上（+Y）
    
    private Rigidbody2D rb;
    private RopeController ropeController;
    private Collider2D collider;
    
    
    [Header("匹配的目标 Tag")]
    public string targetTag = "CollectibleItem";

    // 记录当前所有接触中的 2D 碰撞体（包含 Trigger 与非 Trigger）
    private readonly HashSet<Collider2D> _contacts = new HashSet<Collider2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentDurability = maxDurability;
        ropeController = GetComponentInParent<RopeController>();
        collider = GetComponent<Collider2D>();

        if (particles != null)
        {
            // 粒子在世界空间模拟，避免跟随父物体旋转
            var main = particles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            // 避免重力把粒子拉下（如你需要重力可调回）
            main.gravityModifierMultiplier = -2f;

            // 直接给粒子一个世界坐标系下的向上速度
            var vel = particles.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = 0f;
            vel.y = upwardSpeed;  // 关键：始终向 +Y
            vel.z = 0f;

            // 如果你的粒子 Shape 是圆/锥，角度越小越像“直喷”
            var shape = particles.shape;
            shape.enabled = true;
            // 可选：让喷发更“直”
            // shape.shapeType = ParticleSystemShapeType.Cone;
            // shape.angle = 0f;
            // shape.radius = 0.02f;
        }
        
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }


// 记录当前满足条件(CollectibleItem type==5)的接触体，避免多 Collider 误触发
private readonly HashSet<Collider2D> _activeContacts = new HashSet<Collider2D>();

// 判定是否命中目标：未 broken + 对方 CollectibleItem 且 itemType==5
private bool MatchesTarget(Collider2D col, out CollectibleItem item)
{
    item = null;
    if (broken || col == null) return false;
    return col.TryGetComponent(out item) && (int)item.itemType == 5;
}

private void EnsureParticlesPlaying()
{
    if (particles != null && !_activeContacts.Count.Equals(0))
    {
        if (!particles.isPlaying) particles.Play(true);
    }
}

private void LateUpdate()
{
    TakeDamage(-healPerSecond * Time.deltaTime);
}

private void EnsureParticlesStoppedIfNoContacts()
{
    if (particles != null && _activeContacts.Count == 0)
    {
        if (particles.isPlaying)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}

void OnDisable()
{
    // 清理状态，避免停用后残留播放
    _activeContacts.Clear();
    if (particles != null)
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
}


// —— 非 Trigger 碰撞 —— //
void OnCollisionEnter2D(Collision2D collision)
{
    var other = collision.collider;
    if (MatchesTarget(other, out _))
    {
        _activeContacts.Add(other);
        EnsureParticlesPlaying();
    }
}

void OnCollisionStay2D(Collision2D collision)
{
    var other = collision.collider;
    if (MatchesTarget(other, out _))
    {
        // 保持你原本的伤害逻辑
        TakeDamage(damagePerSecond * Time.deltaTime);
        EnsureParticlesPlaying();
    }
}

void OnCollisionExit2D(Collision2D collision)
{
    var other = collision.collider;
    if (other != null && _activeContacts.Remove(other))
        EnsureParticlesStoppedIfNoContacts();
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