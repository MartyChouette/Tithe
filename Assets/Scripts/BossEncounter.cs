using UnityEngine;

/// <summary>
/// Place on a trigger collider at the mask pedestal.
/// Player needs a kinematic Rigidbody + Collider for OnTriggerEnter to fire.
/// </summary>
public class BossEncounter : MonoBehaviour
{
    [SerializeField] private MaskData bossMask;
    [SerializeField] private FloorExit floorExit;

    private bool defeated;

    void OnTriggerEnter(Collider other)
    {
        if (defeated) return;
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Exploring) return;

        if (bossMask == null)
        {
            Debug.LogError("[BossEncounter] bossMask is not assigned.", this);
            return;
        }

        GameManager.Instance.SetActiveBossEncounter(this);
        GameManager.Instance.StartBossCombat(bossMask);
    }

    public void OnBossDefeated()
    {
        defeated = true;
        if (floorExit != null)
            floorExit.Unlock();
    }
}
