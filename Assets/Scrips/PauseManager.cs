using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private PlayerControls controls;
    private bool isPaused = false;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.Pause.performed += OnPause;
    }

    private void OnDisable()
    {
        controls.UI.Pause.performed -= OnPause;
        controls.UI.Disable();
    }

    private void OnPause(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            controls.Player.Disable();
            controls.Player2.Disable();

            Time.timeScale = 0f;

            Debug.Log("JUEGO PAUSADO | UI activa.");
        }
        else
        {
            controls.Player.Enable();
            controls.Player2.Enable();

            Time.timeScale = 1f;

            Debug.Log("JUEGO REANUDADO | Player y Player2 activos.");
        }
    }
}