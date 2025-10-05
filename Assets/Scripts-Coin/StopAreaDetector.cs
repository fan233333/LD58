using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopAreaDetector : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float stopThreshold = 0.1f; // ֹͣ�ж���ֵ
    public CheckFull checkFull;
    public CheckSpecificObject checkSpecificObject;

    public static bool playerInArea = false;
    private bool readyToGetout = false;
    private Rigidbody2D playerRigidbody;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInArea = true;
            playerRigidbody = other.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInArea = false;
            readyToGetout = false;
            playerRigidbody = null;
        }
    }

    private void Update()
    {
        if (playerInArea && IsPlayerStopped() && !readyToGetout)
        {
            OnPlayerStoppedInArea();
            readyToGetout = true;
        }
    }

    private bool IsPlayerStopped()
    {
        if (playerRigidbody == null) return false;

        // ����ٶ��Ƿ�ӽ���
        return playerRigidbody.velocity.magnitude < stopThreshold;
    }

    private void OnPlayerStoppedInArea()
    {
        Debug.Log("�����������ͣ������!");
        // ������ִ����Ӧ���߼�
        if (transform.tag == "StopArea")
        {
            checkFull.StartAttractionProcess();
        }
        if(transform.tag == "ManufactureArea")
        {
            checkSpecificObject.StartAttractionProcess();
        }
        
    }

 
}
