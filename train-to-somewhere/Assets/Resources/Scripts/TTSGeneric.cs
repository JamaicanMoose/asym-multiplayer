using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class TTSGeneric : MonoBehaviour
{
    public abstract Transform GetLocalPlayer();

    public event EventHandler GameStarted;

    [HideInInspector]
    public bool gameStarted = false;

    protected virtual void OnGameStarted(EventArgs e)
    {
        EventHandler handler = GameStarted;
        handler?.Invoke(this, e);
    }

    public void GameStart()
    {
        EventArgs e = new EventArgs();
        gameStarted = true;
        OnGameStarted(e);
    }
}
