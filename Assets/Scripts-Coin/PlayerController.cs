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
    public Transform suckEffect;
    public float minMovementThreshold = 0.1f; // ��С�ƶ���ֵ
    public float maxMass = 10f;
    public float baseBounceForce = 5f;
    public float velocityMultiplier = 1.5f;
    public float maxBounceForce = 20f;
    public Transform InitParent;

    public static float playerangle = 0;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float totalMass = 0;
    private Vector2 lastPosition;
    private float currentSpeed;
    private bool isColliding = false;
    private bool isInit = false;


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
        totalMass = 0;
        lastPosition = transform.position;
        isColliding = false;
        isInit = false;

    }

    void Update()
    { 
        if(!isInit && InitParent.childCount > 0)
        {
            int index = Random.Range(0, InitParent.childCount);
            Vector3 InitPos = InitParent.GetChild(index).position;
            transform.position = InitPos;
            isInit = true;
        }
        // ���㵱ǰ�ٶ�
        currentSpeed = rb.velocity.magnitude;
        lastPosition = transform.position;

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
        totalMass = ItemStatistics.Instance.GetTotalMass();
        if(totalMass <= 0)
        {
            totalMass = 1;
        }else if(totalMass > maxMass)
        {
            totalMass = maxMass;
        }

        if (!isColliding)
        {
            // �ƶ���ɫ
            //rb.MovePosition(rb.position + movement * maxSpeed * Time.fixedDeltaTime);
            rb.AddForce(movement * moveForce);  //(1 / totalMass)

            // ��������ٶ�
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }


        if (movement != Vector2.zero)
        {
            // ֱ�Ӽ��㿴����ĽǶ�
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            playerangle = angle;
            //Debug.Log(playerangle);

            //if(angle == 0)
            //{
            //    Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
            //    rotateObject.rotation = targetRotation;
            //}else if(angle == 180)
            //{
            //    Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
            //    rotateObject.rotation = targetRotation;
            //}
            //else
            //{
            //    // ����Ŀ����ת
            //    Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            //    // ƽ����ת
            //    rotateObject.rotation = targetRotation;//Quaternion.RotateTowards(rotateObject.rotation, targetRotation, maxRotationSpeed * Time.deltaTime); ;
            //}

            //Debug.Log(z);
            if(angle == 0)
            {
                spriteRenderer.sprite = right;
                suckEffect.rotation = Quaternion.Euler(27f, 90f, -90f);
                rotateObject.rotation = Quaternion.Euler(0f, 0f, -30f);
            }
            else if(angle == 180)
            {
                spriteRenderer.sprite = left;
                suckEffect.rotation = Quaternion.Euler(143f, 90f, -90f);
                rotateObject.rotation = Quaternion.Euler(0f, 0f, -146f);
            }
            else if(angle == 90)
            {
                spriteRenderer.sprite = up;
                suckEffect.rotation = Quaternion.Euler(-90f, 90f, -90f);
                rotateObject.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            else if(angle == -90)
            {
                spriteRenderer.sprite = down;
                suckEffect.rotation = Quaternion.Euler(90f, 90f, -90f);
                rotateObject.rotation = Quaternion.Euler(0f, 0f, -90f);
            }
            else if(angle == 45)
            {
                spriteRenderer.sprite = ur;
                suckEffect.rotation = Quaternion.Euler(-45f, 90f, -90f);
                rotateObject.rotation = Quaternion.Euler(0f, 0f, 45f);
            }
            else if(angle == -45)
            {
                spriteRenderer.sprite = dr;
                suckEffect.rotation = Quaternion.Euler(45f, 90f, -90f);
                rotateObject.rotation = Quaternion.Euler(0f, 0f, -45f);
            }
            else if(angle == 135)
            {
                spriteRenderer.sprite = ul;
                suckEffect.rotation = Quaternion.Euler(-135f, 90f, -90f);
                rotateObject.rotation = Quaternion.Euler(0f, 0f, 135f);
            }
            else if(angle == -135)
            {
                spriteRenderer.sprite = dl;
                suckEffect.rotation = Quaternion.Euler(135f, 90f, -90f);
                rotateObject.rotation = Quaternion.Euler(0f, 0f, -135f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            isColliding = true;

            // ��ȡ��ײ����
            Vector2 collisionNormal = GetCollisionNormal2D(other);

            // ��������ٶȵķ�����
            float dynamicBounceForce = CalculateDynamicBounceForce();

            // ���㷴�䷽��
            Vector2 reflectDirection = Vector2.Reflect(rb.velocity.normalized, collisionNormal);

            // Ӧ�û����ٶȵķ�����
            rb.velocity = reflectDirection * dynamicBounceForce;

            // �����ӳٺ�������ײ״̬
            Invoke("ResetCollision", 0.1f);

            Debug.Log($"��ײ�ٶ�: {currentSpeed}, ������: {dynamicBounceForce}");
        }
    }

    float CalculateDynamicBounceForce()
    {
        // ���������� + �ٶȳ��� * ��ǰ�ٶ�
        float calculatedForce = baseBounceForce + (velocityMultiplier * currentSpeed);

        // ������󷴵���
        return Mathf.Min(calculatedForce, maxBounceForce);
    }

    void ResetCollision()
    {
        isColliding = false;
    }

    Vector2 GetCollisionNormal2D(Collider2D wallCollider)
    {
        // ʹ�ö�����߻�ȡ����ȷ�ķ���
        Vector2[] rayDirections = {
            Vector2.left, Vector2.right, Vector2.up, Vector2.down,
            new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
            new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
        };

        RaycastHit2D hit;
        Vector2 averageNormal = Vector2.zero;
        int hitCount = 0;

        foreach (Vector2 dir in rayDirections)
        {
            hit = Physics2D.Raycast(transform.position, dir, 1f);
            if (hit.collider != null && hit.collider == wallCollider)
            {
                averageNormal += hit.normal;
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            return (averageNormal / hitCount).normalized;
        }

        // ���÷��߼���
        Vector2 closestPoint = wallCollider.ClosestPoint(transform.position);
        return ((Vector2)transform.position - closestPoint).normalized;
    }
}
