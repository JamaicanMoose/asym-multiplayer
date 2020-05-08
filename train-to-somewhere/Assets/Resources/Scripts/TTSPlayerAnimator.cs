using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSPlayerAnimator : MonoBehaviour
{

    public Animator playerAnim;

    private bool Walk = false;
    private bool WalkTired = false;
    private bool isTired = false;
    private bool isHolding = false;
    private bool Eat = false;
    private bool Jump = false;
    private bool Cook = false;
    private bool Fix = false;
    private bool Shovel = false;
    private bool isCannon = false;
    private bool isShooting = false;
    private bool Change = false;
    private bool Lose = false;
    private bool Win = false;
    private float WalkSpeed = 1.0f;

    private bool isServer = false;

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
    }

    public void SetWalk(bool value)
    {
        if(Walk != value)
        {
            Walk = value;
            playerAnim.SetBool("Walk", Walk);
        }
    }

    public void SetWalkTired(bool value)
    {
        if (WalkTired != value)
        {
            WalkTired = value;
            playerAnim.SetBool("WalkTired", WalkTired);
        }
    }

    public void SetisTired(bool value)
    {
        if (isTired != value)
        {
            isTired = value;
            playerAnim.SetBool("isTired", isTired);
        }
    }

    public void SetisHolding(bool value)
    {
        if (isHolding != value)
        {
            isHolding = value;
            playerAnim.SetBool("isHolding", isHolding);
        }
    }

    public void SetEat(bool value)
    {
        if (Eat != value)
        {
            Eat = value;
            playerAnim.SetBool("Eat", Eat);
        }
    }

    public void SetJump(bool value)
    {
        if (Jump != value)
        {
            Jump = value;
            playerAnim.SetBool("Jump", Jump);
        }
    }

    public void SetCook(bool value)
    {
        if (Cook != value)
        {
            Cook = value;
            playerAnim.SetBool("Cook", Cook);
        }
    }

    public void SetFix(bool value)
    {
        if (Fix != value)
        {
            Fix = value;
            playerAnim.SetBool("Fix", Fix);
        }
    }

    public void SetShovel(bool value)
    {
        if (Shovel != value)
        {
            Shovel = value;
            playerAnim.SetBool("Shovel", Shovel);
        }
    }

    public void SetisCannon(bool value)
    {
        if (isCannon != value)
        {
            isCannon = value;
            playerAnim.SetBool("isCannon", isCannon);
        }
    }

    public void SetisShooting(bool value)
    {
        if (isShooting != value)
        {
            isShooting = value;
            playerAnim.SetBool("isShooting", isShooting);
        }
    }

    public void SetChange(bool value)
    {
        if (Change != value)
        {
            Change = value;
            playerAnim.SetBool("Change", Change);
        }
    }

    public void SetLose(bool value)
    {
        if (Lose != value)
        {
            Lose = value;
            playerAnim.SetBool("Lose", Lose);
        }
    }

    public void SetWin(bool value)
    {
        if (Win != value)
        {
            Win = value;
            playerAnim.SetBool("Win", Win);
        }
    }

    public void SetWalkSpeed(float value)
    {
        if (WalkSpeed != value)
        {
            WalkSpeed = value;
            playerAnim.SetFloat("WalkSpeed", WalkSpeed);
        }
    }




}
