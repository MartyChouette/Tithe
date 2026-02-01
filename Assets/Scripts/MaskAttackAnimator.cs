using System.Collections;
using UnityEngine;

public class MaskAttackAnimator : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Transform maskMount;
    [SerializeField] private Transform[] enemyTargets;
    [SerializeField] private float attackSpeed = 8f;
    [SerializeField] private float returnSpeed = 6f;

    private GameObject activeMaskInstance;

    public void Initialize(PlayerStats stats, Transform mount, Transform[] targets)
    {
        playerStats = stats;
        maskMount = mount;
        enemyTargets = targets;
    }

    public void SpawnMask()
    {
        ClearMask();
        if (playerStats.EquippedMask != null && playerStats.EquippedMask.modelPrefab != null)
        {
            activeMaskInstance = Instantiate(playerStats.EquippedMask.modelPrefab, maskMount);
            activeMaskInstance.transform.localPosition = Vector3.zero;
            activeMaskInstance.transform.localRotation = Quaternion.identity;
        }
    }

    public void ClearMask()
    {
        if (activeMaskInstance != null)
        {
            Destroy(activeMaskInstance);
            activeMaskInstance = null;
        }
    }

    public IEnumerator PlayAttack(int targetIndex)
    {
        if (activeMaskInstance == null || enemyTargets == null || targetIndex < 0 || targetIndex >= enemyTargets.Length)
            yield break;

        if (enemyTargets[targetIndex] == null)
            yield break;

        Vector3 startPos = activeMaskInstance.transform.position;
        Vector3 targetPos = enemyTargets[targetIndex].position;

        // Lunge toward enemy
        float t = 0f;
        while (t < 1f)
        {
            if (activeMaskInstance == null) yield break;
            t += Time.deltaTime * attackSpeed;
            activeMaskInstance.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        // Return to mount
        t = 0f;
        while (t < 1f)
        {
            if (activeMaskInstance == null) yield break;
            t += Time.deltaTime * returnSpeed;
            activeMaskInstance.transform.position = Vector3.Lerp(targetPos, startPos, t);
            yield return null;
        }

        if (activeMaskInstance != null)
            activeMaskInstance.transform.position = startPos;
    }
}
