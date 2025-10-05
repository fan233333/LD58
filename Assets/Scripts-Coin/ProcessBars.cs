using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessBars : MonoBehaviour
{
    public Transform processBar;
    public Transform healthBar;
    public Transform timeBar;
    public static float myHealth;

    private float collectingNum;
    public float totalNum;
    public float maxHP;
    public float maxTime;

    private bool isTiming = false;
    private float startTime;
    private float elapsedTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //totalNum = 100;
        myHealth = maxHP;
        //maxHP = 100;
        isTiming = false;
    }

    // Update is called once per frame
    void Update()
    {
        collectingNum = (float)CheckFull.attractCount;
        processBar.localScale = new Vector3(collectingNum / totalNum, processBar.localScale.y, processBar.localScale.z);

        healthBar.localScale = new Vector3(myHealth/maxHP, healthBar.localScale.y, healthBar.localScale.z);

        if (CheckSpecificObject.gainEnough && !isTiming)
        {
            isTiming = true;
            StartCoroutine(checkTime());
        }

        if (!StopAreaDetector.playerInArea && isTiming)
        {
            timeBar.localScale = new Vector3(0f, timeBar.localScale.y, timeBar.localScale.z);
            isTiming = false;
        }
    }

    IEnumerator checkTime()
    {
        startTime = Time.time;
        elapsedTime = 0f;

        while (elapsedTime < maxTime)
        {
            Debug.Log(StopAreaDetector.playerInArea);
            if (!StopAreaDetector.playerInArea)
            {
                elapsedTime = 0f;
                timeBar.localScale = new Vector3(0f, timeBar.localScale.y, timeBar.localScale.z);
                yield break;
            }
            yield return null;
            elapsedTime = Time.time - startTime;

            // 计算已经过去的时间比例 (0 到 1)
            float timeProgress = elapsedTime / maxTime;

            // 更新时间条的比例
            timeBar.localScale = new Vector3(timeProgress, timeBar.localScale.y, timeBar.localScale.z);

            // 每秒检测一次
            yield return new WaitForSeconds(1f);
        }

        // 30秒结束后，确保时间条是满的
        CheckSpecificObject.gainEnough = false;
        timeBar.localScale = new Vector3(1f, timeBar.localScale.y, timeBar.localScale.z);
        Debug.Log("30秒时间到！");

        yield return null;
    }
}

