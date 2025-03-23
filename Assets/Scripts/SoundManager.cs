using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource playSFX;
    [SerializeField] private AudioClip placeSFX;
    [SerializeField] private AudioClip playWinSFX;
    [SerializeField] private AudioClip playLoseSFX;

    private void Start()
    {
        GameManager.Instance.OnPlaceObject += GameManager_OnPlaceObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnPlaceObject(object sender, EventArgs e)
    {
        playSFX.PlayOneShot(placeSFX);
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(e.winnerPlayerType == GameManager.Instance.LocalPlayerType)
        {
            playSFX.PlayOneShot(playWinSFX);
        }
        else
        {
            playSFX.PlayOneShot(playLoseSFX);
        }
    }
    
}
