using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitManager : MonoBehaviour
{
    public void OnPlay()
    {
        SceneManager.LoadScene("ClientScene");
    }

    public void OnStartServer()
    {
        SceneManager.LoadScene("ServerScene");
    }
}
