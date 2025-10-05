using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 2f;
    public float moveForce = 10f;
    public float maxRotationSpeed = 90f;
    public Sprite right;
    public Sprite left;
    public Sprite up;
    public Sprite down;
    public Sprite ur;
    public Sprite ul;
    public Sprite dr;
    public Sprite dl;
    public SpriteRenderer spriteRenderer;
    public Transform rotateObject;
    public float minMovementThreshold = 0.1f; // ��С�ƶ���ֵ
    public static float playerangle = 0;

    private Rigidbody2D rb;
    private Vector2 movement;
    
    private Quaternion targetRotation;
    private Vector2 lastValidDirection = Vector2.right;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // ������������ - ģ�����/��������
        rb.drag = 1f; // ֵԽ��ֹͣԽ��

        // ���ý��ٶ�����
        rb.angularDrag = 0.5f;
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

    }

    void FixedUpdate()
    {
        // �ƶ���ɫ
        //rb.MovePosition(rb.position + movement * maxSpeed * Time.fixedDeltaTime);
        rb.AddForce(movement * moveForce);

        // ��������ٶ�
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        if (movement != Vector2.zero)
        {
            // ֱ�Ӽ��㿴����ĽǶ�
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            playerangle = angle;

            // ����Ŀ����ת
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // ƽ����ת
            rotateObject.rotation = targetRotation;//Quaternion.RotateTowards(rotateObject.rotation, targetRotation, maxRotationSpeed * Time.deltaTime); ;

            float z = rotateObject.rotation.z;

            //Debug.Log(z);
            if(angle == 0)
            {
                spriteRenderer.sprite = right;
            }else if(angle == 180)
            {
                spriteRenderer.sprite = left;
            }else if(angle == 90)
            {
                spriteRenderer.sprite = up;
            }else if(angle == -90)
            {
                spriteRenderer.sprite = down;
            }else if(angle == 45)
            {
                spriteRenderer.sprite = ur;
            }
            else if(angle == -45)
            {
                spriteRenderer.sprite = dr;
            }
            else if(angle == 135)
            {
                spriteRenderer.sprite = ul;
            }
            else if(angle == -135)
            {
                spriteRenderer.sprite = dl;
            }
        }
    }
}
