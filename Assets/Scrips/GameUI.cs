using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; }

    [SerializeField] private Animator menuAnimator;

    public Action<bool> SetlocalGame;
    private void Awake()
    {
        Instance = this;
    }

    //Buttons
    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
        SetlocalGame?.Invoke(true);
    }

    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        SetlocalGame?.Invoke(false);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        SetlocalGame?.Invoke(false);
        Debug.Log("OnOnlineConnectButton"); // $$
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }


}
