using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class TTSGeneric : MonoBehaviour
{
    public abstract Transform GetLocalPlayer();

    public event EventHandler GameStarted;
    
    protected virtual void OnGameStarted(EventArgs e)
    {
        EventHandler handler = GameStarted;
        handler?.Invoke(this, e);
    }

    public void GameStart()
    {
        EventArgs e = new EventArgs();
        OnGameStarted(e);
    }
}
