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
    public LayerMask wallLayer;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float totalMass = 0;
    private Vector2 lastPosition;
    private float currentSpeed;
    private bool isColliding = false;
    private bool isInit = false;
    private Vector2 lastPos;
    private Vector2 lastSafePos;


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
        lastPos = rb.position;

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
        Vector2 currentPos = rb.position;
        Vector2 moveDir = currentPos - lastPos;
        float moveDist = moveDir.magnitude;
        lastSafePos = rb.position;

        if (moveDist > 0f)
        {
            // 从上一帧位置到这一帧位置发一条 Ray
            RaycastHit2D hit = Physics2D.Raycast(
                lastPos,
                moveDir.normalized,
                moveDist + 1f,
                wallLayer
            );

            if (hit.collider != null)
            {
                // 命中了墙，相当于“补触发”一次
                HandleWallCollision(hit.collider, hit.normal);
            }
        }

        lastPos = currentPos;
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
        //if (other.CompareTag("Wall"))
        //{
        //    isColliding = true;

        //    // ��ȡ��ײ����
        //    Vector2 collisionNormal = GetCollisionNormal2D(other);

        //    // ��������ٶȵķ�����
        //    float dynamicBounceForce = CalculateDynamicBounceForce();

        //    // ���㷴�䷽��
        //    Vector2 reflectDirection = Vector2.Reflect(rb.velocity.normalized, collisionNormal);

        //    // Ӧ�û����ٶȵķ�����
        //    rb.velocity = reflectDirection * dynamicBounceForce;

        //    // �����ӳٺ�������ײ״̬
        //    Invoke("ResetCollision", 0.1f);

        //    Debug.Log($"��ײ�ٶ�: {currentSpeed}, ������: {dynamicBounceForce}");
        //}
        if (other.CompareTag("Wall"))
        {
            if (other.CompareTag("Wall"))
            {
                // 如果已经在碰撞状态里，就不要再处理了
                if (isColliding) return;

                // 这里当成“第一次检测到碰撞”
                Vector2 collisionNormal = GetCollisionNormal2D(other);
                HandleWallCollision(other, collisionNormal);
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            // ✅ 关键逻辑：
            // 如果上一帧“错过了 OnTriggerEnter2D”，
            // 但这一帧已经处于重叠状态，
            // 那么 isColliding 还是 false，
            // 我们就在 Stay 里补一次处理。
            if (!isColliding)
            {
                Vector2 collisionNormal = GetCollisionNormal2D(other);
                HandleWallCollision(other, collisionNormal);
            }
        }
    }

    void HandleWallCollision(Collider2D other, Vector2 collisionNormal)
    {
        isColliding = true;

        // ① 把物体从重叠状态拉回“上一个安全位置”
        rb.position = lastSafePos;

        // ② 再设置反弹速度
        float dynamicBounceForce = CalculateDynamicBounceForce();
        Vector2 reflectDirection =
            Vector2.Reflect(rb.velocity.normalized, collisionNormal);

        rb.velocity = reflectDirection * dynamicBounceForce;

        Invoke(nameof(ResetCollision), 0.1f);

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
