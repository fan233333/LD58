using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFall : MonoBehaviour
{
    public GameObject player;
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

            newObject.transform.localScale = collision.transform.localScale;

            Rigidbody2D rb = newObject.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;

            Collider2D col = rb.GetComponent<Collider2D>();
            col.enabled = true;

            newObject.layer = 0;

            newObject.tag = "Collectible";

            SpriteRenderer spriteRenderer = newObject.GetComponent<SpriteRenderer>();

            // 方法1：通过名称设置Sorting Layer
            spriteRenderer.sortingLayerName = "Default";

            Destroy(collision.gameObject);

        }
    }
}
