using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Color drawColor;
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
        });
    }
    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameDraw += GameManager_OnGameDraw;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
        Hide();
    }

    private void GameManager_OnGameDraw(object sender, EventArgs e)
    {
        resultText.text = "DRAW!";
        resultText.color = drawColor;
        Show();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(e.winnerPlayerType == GameManager.Instance.LocalPlayerType)
        {
            resultText.text = "YOU WIN!";
            resultText.color = winColor;
        }
        else
        {
            resultText.text = "YOU LOSE!";
            resultText.color = loseColor;
        }
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    
}
