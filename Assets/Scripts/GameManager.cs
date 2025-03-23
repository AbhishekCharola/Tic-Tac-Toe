using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Events
    public event EventHandler<OnGridPositionClickedEventArgs> OnGridPositionClicked;
    public class OnGridPositionClickedEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line winLine;
        public PlayerType winnerPlayerType;
    }

    public event EventHandler OnCurrentPlayablePlayerTypeChanged;

    public event EventHandler OnRematch;

    public event EventHandler OnGameDraw;

    public event EventHandler OnScoreUpdated;

    public event EventHandler OnPlaceObject;

    #endregion

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }
    private PlayerType localPlayerType;
    public PlayerType LocalPlayerType => localPlayerType;


    private NetworkVariable<PlayerType> currentPlayablePlayerType = new();
    public PlayerType CurrentPlayablePlayerType => currentPlayablePlayerType.Value;

    private PlayerType[,] playerTypeArray;

    private NetworkVariable<int> playerCrossScore = new();
    public int PlayerCrossScore => playerCrossScore.Value;

    private NetworkVariable<int> playerCircleScore = new();
    public int PlayerCircleScore => playerCircleScore.Value;


    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }
    public struct Line
    {
        public List<Vector2Int> gridPositions;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }
    private List<Line> lineList;

    private void Awake()
    {
        Singleton();
        playerTypeArray = new PlayerType[3, 3];
        lineList = new List<Line>{
            // Horizontal lines
            new Line{
                gridPositions = new List<Vector2Int>{
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(2, 0)
                },
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal
            },
            new Line{
                gridPositions = new List<Vector2Int>{
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal
            },
            new Line{
                gridPositions = new List<Vector2Int>{
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal
            },

            // Vertical lines
            new Line{
                gridPositions = new List<Vector2Int>{
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2)
                },
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical
            },
            new Line{
                gridPositions = new List<Vector2Int>{
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical
            },
            new Line{
                gridPositions = new List<Vector2Int>{
                    new Vector2Int(2, 0),
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical
            },

            // Diagonal lines
            new Line{
                gridPositions = new List<Vector2Int>{
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA
            },
            new Line{
                gridPositions = new List<Vector2Int>{
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 0)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB
            }
        };
    }
    private void Singleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// This function is called when the object is spawned on the network
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        // This event is triggered when the value of currentPlayablePlayerType changes for both clients and server
        // This is a network variable, so it will be synced to all clients
        currentPlayablePlayerType.OnValueChanged += (PlayerType previousValue, PlayerType newValue) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCrossScore.OnValueChanged += (int previousValue, int newValue) =>
        {
            OnScoreUpdated?.Invoke(this, EventArgs.Empty);
        };

        playerCircleScore.OnValueChanged += (int previousValue, int newValue) =>
        {
            OnScoreUpdated?.Invoke(this, EventArgs.Empty);
        };
    }

    /// <summary>
    /// This function is called when a client connects to the server
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    /// <summary>
    /// This function is called on the server to trigger the OnGameStarted event for all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This function is called on the server to trigger the OnGridPositionClicked event
    /// </summary>
    [Rpc(SendTo.Server)] // Always runs on Server this function never run on clients
    /// <param name="x">The x coordinate of the grid position clicked</param>
    /// <param name="y">The y coordinate of the grid position clicked</param>
    /// <param name="playerType">The player type that clicked on the grid position</param>
    public void ClickOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        Debug.Log("Clicked on grid position: " + x + ", " + y);

        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None)
        {
            return;
        }

        playerTypeArray[x, y] = playerType;
        
        TriggerOnPlaceObjectRpc();

        OnGridPositionClicked?.Invoke(this, new OnGridPositionClickedEventArgs
        {
            x = x,
            y = y,
            playerType = playerType
        });

        switch (playerType)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinCondition();
    }

    /// <summary>
    /// This function is called on the server to trigger the OnPlaceObject event for all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlaceObjectRpc()
    {
        OnPlaceObject?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This function is called on the server to test the win condition
    /// </summary>
    /// <param name="line">The line to test</param>
    /// <returns>True if the player type is the same for all three grid positions, false otherwise</returns>
    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(playerTypeArray[line.gridPositions[0].x, line.gridPositions[0].y],
                              playerTypeArray[line.gridPositions[1].x, line.gridPositions[1].y],
                              playerTypeArray[line.gridPositions[2].x, line.gridPositions[2].y]);
    }

    /// <summary>
    /// This function is called on the server to test the win condition
    /// </summary>
    /// <param name="aPlayerType">The player type of the first grid position</param>
    /// <param name="bPlayerType">The player type of the second grid position</param>
    /// <param name="cPlayerType">The player type of the third grid position</param>
    /// <returns>True if the player type is the same for all three grid positions, false otherwise</returns>
    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None && aPlayerType == bPlayerType && bPlayerType == cPlayerType;
    }

    /// <summary>
    /// This function is called on the server to test the win condition
    /// </summary>
    private void TestWinCondition()
    {
        for (int i = 0; i < lineList.Count; i++)
        {   
            if (TestWinnerLine(lineList[i]))
            {
                currentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winPlayerType = playerTypeArray[lineList[i].centerGridPosition.x, lineList[i].centerGridPosition.y];
                if (winPlayerType == PlayerType.Cross)
                {
                    playerCrossScore.Value++;
                }
                else if (winPlayerType == PlayerType.Circle)
                {
                    playerCircleScore.Value++;
                }
                TriggerOnGameWinRpc(i, winPlayerType);
                return;
            }
        }

        bool isDraw = true;
        for (int i = 0; i < playerTypeArray.GetLength(0); i++)
        {
            for (int j = 0; j < playerTypeArray.GetLength(1); j++)
            {
                if (playerTypeArray[i, j] == PlayerType.None)
                {
                    isDraw = false;
                    break;
                }
            }
        }
        if (isDraw)
        {
            TriggerOnGameDrawRpc();
        }
    }

    /// <summary>
    /// This function is called on the server to trigger the OnGameDraw event for all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameDrawRpc()
    {
        OnGameDraw?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This function is called on the server to trigger the OnGameWin event for all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winnerPlayerType)
    {
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            winLine = lineList[lineIndex],
            winnerPlayerType = winnerPlayerType
        });
    }

    /// <summary>
    /// This function is called on the server to reset the game
    /// </summary>
    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for( int i =0; i < playerTypeArray.GetLength(0); i++)
        {
            for( int j =0; j < playerTypeArray.GetLength(1); j++)
            {
                playerTypeArray[i, j] = PlayerType.None;
            }
        }
        currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }

    /// <summary>
    /// This function is called on the server to trigger the OnRematch event for all clients
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }
}
