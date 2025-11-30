using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : Singleton<GameManager>
{
    public bool IsPaused;
    InputAction pauseAction;

    public static readonly int GroundLayer = 6; 
    public static readonly int PlayerLayer = 7; 
    public static readonly int DestructibleLayer = 8; 

    override protected void Initialize()
    {
        pauseAction = InputSystem.actions.FindAction("Cancel");
        pauseAction.performed += ctx => TogglePause();
        pauseAction.Enable();
    }

    void TogglePause()
    {
        IsPaused = !IsPaused;
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

}
