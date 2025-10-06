using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class TrayShake : MonoBehaviour
{
    private Rigidbody2D player;
    public float coef = 1f;
    public float randomness = 0.1f;

    private Vector2 lastVelocity;
    private float accelerationMagnitude;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        lastVelocity = player.velocity;
    }

    void Update()
    {
        if(player == null)
        {
            player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            lastVelocity = player.velocity;
        }
        // // 计算加速度 = (当前速度 - 上一帧速度) / deltaTime
        Vector2 currentVelocity = player.velocity;
        Vector2 acceleration = (currentVelocity - lastVelocity) / Time.deltaTime;
        
        lastVelocity = currentVelocity;
        
        // Vector2 currentVelocity = (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))).normalized;
        //
        // Vector2 acceleration = (currentVelocity - lastVelocity);
        //
        // lastVelocity = currentVelocity;
        
        Shake(-acceleration);
    }

    private void Shake(Vector2 acc)
    {
        foreach (Transform child in transform)
        {
            Rigidbody2D rgd;
            try
            {
                rgd = child.GetComponent<Rigidbody2D>();

                if (rgd is not null && !child.CompareTag("Rope"))
                {
                    rgd.AddForce(
                        acc.magnitude * rgd.mass * coef * (acc.normalized + randomness * Random.insideUnitCircle),
                        ForceMode2D.Force);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            

        }
    }
}
