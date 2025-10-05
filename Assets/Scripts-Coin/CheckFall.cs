using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFall : MonoBehaviour
{
    public GameObject player;
    public float minHorizontalForce = 10f;
    public float maxHorizontalForce = 20f;
    public float friction = 5f;

    private float angle = 180;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "ReadyToFall")
        {
            GameObject newObject = Instantiate(collision.gameObject, player.transform.position, Quaternion.identity);
            //Debug.Log(collision.transform.localScale);
            newObject.transform.localScale = collision.transform.localScale;

            Rigidbody2D rb = newObject.GetComponent<Rigidbody2D>();
            // 设置线性阻尼 - 模拟空气/流体阻力
            rb.drag = friction; // 值越大，停止越快
            rb.gravityScale = 0;
            

            Collider2D col = rb.GetComponent<Collider2D>();
            col.enabled = true;

            newObject.layer = 0;

            newObject.tag = "Collectible";

            SpriteRenderer spriteRenderer = newObject.GetComponent<SpriteRenderer>();

            // Sorting Layer
            spriteRenderer.sortingLayerName = "Default";

            angle = PlayerController.playerangle;

            if (rb != null)
            {
                // 添加随机水平力
                float randomForcex = Random.Range(minHorizontalForce, maxHorizontalForce);
                float randomForcey = Random.Range(minHorizontalForce, maxHorizontalForce);
                //rb.AddForce(new Vector2(randomForcex, randomForcey), ForceMode2D.Impulse);
                if (angle == 0)
                {
                    rb.AddForce(new Vector2(-randomForcex, 0), ForceMode2D.Impulse);
                }
                else if (angle == 180)
                {
                    rb.AddForce(new Vector2(randomForcex, 0), ForceMode2D.Impulse);
                }
                else if (angle == 90)
                {
                    rb.AddForce(new Vector2(0, randomForcey), ForceMode2D.Impulse);
                }
                else if (angle == -90)
                {
                    rb.AddForce(new Vector2(0, -randomForcey), ForceMode2D.Impulse);
                }
                else if (angle == 45)
                {
                    rb.AddForce(new Vector2(randomForcex, randomForcey), ForceMode2D.Impulse);
                }
                else if (angle == -45)
                {
                    rb.AddForce(new Vector2(-randomForcex, randomForcey), ForceMode2D.Impulse);
                }
                else if (angle == 135)
                {
                    rb.AddForce(new Vector2(randomForcex, -randomForcey), ForceMode2D.Impulse);
                }
                else if (angle == -135)
                {
                    rb.AddForce(new Vector2(-randomForcex, -randomForcey), ForceMode2D.Impulse);
                }

                rb.angularVelocity = 0;
            }


            Destroy(collision.gameObject);

        }
    }
}
