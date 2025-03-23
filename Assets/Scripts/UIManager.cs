using System;
using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject crossArrow;
    [SerializeField] private GameObject circleArrow;
    [SerializeField] private GameObject crossYOUText;
    [SerializeField] private GameObject circleYOUText;

    [SerializeField] private TMP_Text playerCrossScoreText;
    [SerializeField] private TMP_Text playerCircleScoreText;


    private void Awake()
    {
        crossArrow.SetActive(false);
        circleArrow.SetActive(false);
        crossYOUText.SetActive(false);
        circleYOUText.SetActive(false);
        playerCrossScoreText.text = "";
        playerCircleScoreText.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnScoreUpdated += GameManager_OnScoreUpdated;
    }

    private void GameManager_OnScoreUpdated(object sender, EventArgs e)
    {
        playerCrossScoreText.text = GameManager.Instance.PlayerCrossScore.ToString();
        playerCircleScoreText.text = GameManager.Instance.PlayerCircleScore.ToString();
    }

    /// <summary>
    /// This function is called when the current playable player type changes
    /// It will update the current arrow to the correct player type for all clients as this is a network variable
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    /// <summary>
    /// This function is called when the game starts
    /// It will call for all clients through Rpc to show your PlayerType
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if(GameManager.Instance.LocalPlayerType == GameManager.PlayerType.Cross)
        {
            crossYOUText.SetActive(true);
        }
        else
        {
            circleYOUText.SetActive(true);
        }
        UpdateCurrentArrow();
        playerCrossScoreText.text = "0";
        playerCircleScoreText.text = "0";
    }

    /// <summary>
    /// This function is called to update the current arrow
    /// </summary>
    private void UpdateCurrentArrow()
    {
        if(GameManager.Instance.CurrentPlayablePlayerType == GameManager.PlayerType.Cross)
        {
            crossArrow.SetActive(true);
            circleArrow.SetActive(false);
        }
        else
        {
            circleArrow.SetActive(true);
            crossArrow.SetActive(false);
        }
    }
}
