using UnityEngine;

/// <summary>
/// Stairway down. Locked until the floor's mask boss is defeated.
/// Player needs a kinematic Rigidbody + Collider for OnTriggerEnter to fire.
/// </summary>
public class FloorExit : MonoBehaviour
{
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private GameObject unlockedVisual;

    private bool isLocked = true;

    void Start()
    {
        UpdateVisuals();
    }

    public void Unlock()
    {
        isLocked = false;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (lockedVisual != null) lockedVisual.SetActive(isLocked);
        if (unlockedVisual != null) unlockedVisual.SetActive(!isLocked);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isLocked) return;
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Exploring) return;

        GameManager.Instance.DescendToNextFloor();
    }
}
