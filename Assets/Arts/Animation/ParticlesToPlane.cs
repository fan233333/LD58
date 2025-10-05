using System.Collections.Generic;
using UnityEngine;

public class ParticlesToPlane : MonoBehaviour
{
    public enum KillZoneShape { Sphere, Box }
    
    public ParticleSystem ps;
    public Transform targetPlane; // target point: particles will be killed when they get close to this point
    
    [Header("Kill Zone Settings")]
    [Tooltip("Shape of the kill zone: Sphere or Box")]
    public KillZoneShape killZoneShape = KillZoneShape.Box;
    
    [Tooltip("Distance (world units) from targetPlane at which to kill particles (for Sphere mode)")]
    public float killDistance = 1.5f;
    
    [Tooltip("Size of the box kill zone in local space (for Box mode)")]
    public Vector3 boxSize = new Vector3(1f, 1f, 1f);
    
    [Header("Debug")]
    [Tooltip("Enable to see debug information in Console and visualize kill range in Scene view")]
    public bool showDebug = false;

    ParticleSystem.Particle[] particles;
    int killedCount = 0;

    void Start()
    {
        Debug.Log("=== ParticlesToPlane Start() called ===");
        
        if (ps == null)
        {
            ps = GetComponent<ParticleSystem>();
            Debug.Log($"ps was null, GetComponent result: {ps}");
        }
        else
        {
            Debug.Log($"ps assigned: {ps.name}");
        }
        
        if (ps == null)
        {
            Debug.LogError("ParticleSystem is NULL! Script will not work.");
            enabled = false;
            return;
        }
        
        if (targetPlane == null)
        {
            Debug.LogError("targetPlane is NULL! Please assign it in Inspector.");
        }
        else
        {
            Debug.Log($"targetPlane assigned: {targetPlane.name} at position {targetPlane.position}");
        }
        
        var main = ps.main;
        particles = new ParticleSystem.Particle[main.maxParticles];
        
        Debug.Log($"Simulation Space: {main.simulationSpace}, Max Particles: {main.maxParticles}, Kill Distance: {killDistance}");
    }

    void Update()
    {
        if (ps == null)
        {
            Debug.LogError("ps is null in Update!");
            return;
        }
        
        if (targetPlane == null)
        {
            Debug.LogError("targetPlane is null in Update!");
            return;
        }

        int count = ps.GetParticles(particles);
        
        if (showDebug)
        {
            Debug.Log($"Frame {Time.frameCount}: Particle count = {count}");
        }
        
        if (count == 0) return;

        // Get target position in world space
        Vector3 targetPos = targetPlane.position;
        bool anyKilled = false;
        int killedThisFrame = 0;

        // Check if using local simulation space
        var main = ps.main;
        bool isLocalSpace = (main.simulationSpace == ParticleSystemSimulationSpace.Local);

        if (showDebug && Time.frameCount % 30 == 0) // Log every 30 frames
        {
            Debug.Log($"Checking {count} particles. Target: {targetPos}, IsLocalSpace: {isLocalSpace}");
        }

        for (int i = 0; i < count; i++)
        {
            var p = particles[i];

            // Convert particle position to world space if needed
            Vector3 particleWorldPos = isLocalSpace ? ps.transform.TransformPoint(p.position) : p.position;

            bool shouldKill = false;
            float dist = 0f;

            if (killZoneShape == KillZoneShape.Sphere)
            {
                // Sphere mode: check distance from particle to target point
                dist = Vector3.Distance(particleWorldPos, targetPos);
                shouldKill = (dist <= killDistance);
            }
            else // Box mode
            {
                // Convert particle world position to target plane's local space
                Vector3 localPos = targetPlane.InverseTransformPoint(particleWorldPos);
                
                // Check if particle is inside the box (half extents)
                Vector3 halfSize = boxSize * 0.5f;
                shouldKill = (Mathf.Abs(localPos.x) <= halfSize.x &&
                             Mathf.Abs(localPos.y) <= halfSize.y &&
                             Mathf.Abs(localPos.z) <= halfSize.z);
                
                if (showDebug && i == 0 && Time.frameCount % 30 == 0)
                {
                    dist = Vector3.Distance(particleWorldPos, targetPos);
                    Debug.Log($"First particle - WorldPos: {particleWorldPos}, LocalPos: {localPos}, BoxHalfSize: {halfSize}, Inside: {shouldKill}");
                }
            }
            
            if (showDebug && i == 0 && Time.frameCount % 30 == 0 && killZoneShape == KillZoneShape.Sphere)
            {
                Debug.Log($"First particle - Pos: {particleWorldPos}, Dist to target: {dist:F3}, Kill dist: {killDistance}");
            }
            
            // If particle is within kill zone, set its lifetime to 0 (will disappear)
            if (shouldKill)
            {
                p.remainingLifetime = 0f;
                particles[i] = p;
                anyKilled = true;
                killedThisFrame++;
            }
        }

        killedCount += killedThisFrame;

        // Only write back if we modified something
        if (anyKilled)
        {
            ps.SetParticles(particles, count);
            
            if (showDebug)
            {
                Debug.Log($"<color=green>Killed {killedThisFrame} particles this frame. Total killed: {killedCount}</color>");
            }
        }
    }

    // Visualize the kill range in Scene view
    void OnDrawGizmos()
    {
        if (targetPlane == null) return;

        if (killZoneShape == KillZoneShape.Sphere)
        {
            // Draw sphere
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPlane.position, killDistance);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(targetPlane.position + Vector3.up * killDistance, targetPlane.position - Vector3.up * killDistance);
            Gizmos.DrawLine(targetPlane.position + Vector3.right * killDistance, targetPlane.position - Vector3.right * killDistance);
            Gizmos.DrawLine(targetPlane.position + Vector3.forward * killDistance, targetPlane.position - Vector3.forward * killDistance);
        }
        else // Box
        {
            // Draw box in target plane's local space
            Gizmos.color = Color.red;
            Gizmos.matrix = targetPlane.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            
            // Reset matrix
            Gizmos.matrix = Matrix4x4.identity;
            
            // Draw axes
            Gizmos.color = Color.green;
            Gizmos.DrawLine(targetPlane.position, targetPlane.position + targetPlane.right * boxSize.x * 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(targetPlane.position, targetPlane.position + targetPlane.up * boxSize.y * 0.5f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(targetPlane.position, targetPlane.position + targetPlane.forward * boxSize.z * 0.5f);
        }
    }
}

