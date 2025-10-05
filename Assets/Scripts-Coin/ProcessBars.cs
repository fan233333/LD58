using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessBars : MonoBehaviour
{
    public Transform processBar;
    public Transform healthBar;
    public static float myHealth = 100;

    public float collectingNum;
    public float totalNum = 100;
    public float maxHP = 100;

    // Start is called before the first frame update
    void Start()
    {
        totalNum = 100;
        myHealth = 100;
        maxHP = 100;
    }

    // Update is called once per frame
    void Update()
    {
        collectingNum = (float)ItemStatistics.Instance.GetItemCount("Circle");
        processBar.localScale = new Vector3(collectingNum / totalNum, processBar.localScale.y, processBar.localScale.z);

        healthBar.localScale = new Vector3(myHealth/maxHP, healthBar.localScale.y, healthBar.localScale.z);
    }
}
