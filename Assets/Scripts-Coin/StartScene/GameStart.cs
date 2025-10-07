using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        SceneManager.LoadScene(1);
    }

    public void Retry()
    {
        SeedStatic.lightYear = 1;
        SceneManager.LoadScene(1);
    }

    public void Back()
    {
        SeedStatic.lightYear = 1;
        SceneManager.LoadScene(0);
    }
}
