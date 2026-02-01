using UnityEngine;

/// <summary>
/// Simple dungeon crawler input: W/S move forward/back, A/D turn left/right.
/// Attach to the same GameObject as AdvancedGridMovement.
/// Disable or remove the existing InputHandler component when using this.
/// </summary>
public class TithePlayerController : MonoBehaviour
{
    private AdvancedGridMovement movement;

    void Start()
    {
        movement = GetComponent<AdvancedGridMovement>();
        if (movement == null)
        {
            Debug.LogError($"[TithePlayerController] AdvancedGridMovement not found on {gameObject.name}. Disabling controller.");
            enabled = false;
        }
    }

    void Update()
    {
        if (!movement.IsStationary())
            return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            movement.MoveForward();
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            movement.MoveBackward();
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            movement.TurnLeft();
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            movement.TurnRight();
    }
}
