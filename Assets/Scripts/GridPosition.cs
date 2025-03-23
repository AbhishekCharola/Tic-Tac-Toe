using UnityEngine;

public class GridPosition : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;

    /// <summary>
    /// This function is called when the mouse button is clicked on the grid position
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log("Mouse clicked on " + gameObject.name);
        GameManager.Instance.ClickOnGridPositionRpc(x, y, GameManager.Instance.LocalPlayerType);
    }
}
