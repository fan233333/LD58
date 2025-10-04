using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 2f;
    public float maxRotationSpeed = 90f;
    public Sprite right;
    public Sprite left;
    public SpriteRenderer spriteRenderer;
    public Transform rotateObject;
    public float minMovementThreshold = 0.1f; // ��С�ƶ���ֵ

    private Rigidbody2D rb;
    private Vector2 movement;
    
    private Quaternion targetRotation;
    private Vector2 lastValidDirection = Vector2.right;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetRotation = rotateObject.rotation;
        
    }

    void Update()
    {
        // ��ȡ����
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D �� ���Ҽ�ͷ
        movement.y = Input.GetAxisRaw("Vertical");   // W/S �� ���¼�ͷ

        //if (movement != Vector2.zero)
        //{
        //   float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
        //    targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //}

        // ʹ�ù̶���ת�ٶ���ת��Ŀ�귽��
        //rotateObject.localRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);

        //rotateObject.localRotation = targetRotation;

        if(Input.GetKeyDown(KeyCode.A))
        {
            spriteRenderer.sprite = left;
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            spriteRenderer.sprite = right;
        }
    }

    void FixedUpdate()
    {
        // �ƶ���ɫ
        rb.MovePosition(rb.position + movement * maxSpeed * Time.fixedDeltaTime);


        if (movement != Vector2.zero)
        {
            // ֱ�Ӽ��㿴����ĽǶ�
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

            // ����Ŀ����ת
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // ƽ����ת
            rotateObject.rotation = targetRotation;//Quaternion.Slerp(rotateObject.rotation, targetRotation, maxRotationSpeed * Time.fixedDeltaTime);
        }
    }
}
