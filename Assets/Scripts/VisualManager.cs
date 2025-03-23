using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.0f;

    [SerializeField] private GameObject crossPrefab;
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private GameObject lineCompletePrefab;

    private List<GameObject> visualList;

    private void Awake()
    {
        visualList = new List<GameObject>();
    }

    private void Start()
    {
        GameManager.Instance.OnGridPositionClicked += GameManager_OnGridPositionClicked;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        foreach (GameObject visual in visualList)
        {
            visual.GetComponent<NetworkObject>().Despawn();
        }
        visualList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        float eulerZ = 0f;
        switch (e.winLine.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.Orientation.DiagonalA:
                eulerZ = 45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eulerZ = -45f;
                break;
        }
        GameObject lineCompleteVisual = Instantiate(lineCompletePrefab, GetWorldPosition(e.winLine.centerGridPosition.x, e.winLine.centerGridPosition.y), Quaternion.Euler(0f, 0f, eulerZ), this.transform);
        visualList.Add(lineCompleteVisual);
        lineCompleteVisual.GetComponent<NetworkObject>().Spawn(true);
    }

    /// <summary>
    /// This function is called when the grid position is clicked
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void GameManager_OnGridPositionClicked(object sender, GameManager.OnGridPositionClickedEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    /// <summary>
    /// This function is called on the server to spawn the object
    /// </summary>
    /// <param name="x">The x coordinate of the grid position</param>
    /// <param name="y">The y coordinate of the grid position</param>
    /// <param name="playerType">The player type that clicked on the grid position</param>
    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        GameObject prefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
        }
        GameObject visual = Instantiate(prefab, GetWorldPosition(x, y), Quaternion.identity, this.transform);
        visual.GetComponent<NetworkObject>().Spawn(true);
        visualList.Add(visual);
    }

    /// <summary>
    /// This function is called to get the world position of the grid position
    /// </summary>
    /// <param name="x">The x coordinate of the grid position</param>
    /// <param name="y">The y coordinate of the grid position</param>
    /// <returns>The world position of the grid position</returns>
    private Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
