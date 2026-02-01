using UnityEngine;
using System.Reflection;

public static class GridMovementHelper
{
    public static void TeleportTo(AdvancedGridMovement movement, Vector3 position)
    {
        movement.transform.position = position;

        var type = typeof(AdvancedGridMovement);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        var moveTowards = type.GetField("moveTowardsPosition", flags);
        var moveFrom = type.GetField("moveFromPosition", flags);

        if (moveTowards != null) moveTowards.SetValue(movement, position);
        if (moveFrom != null) moveFrom.SetValue(movement, position);
    }
}
