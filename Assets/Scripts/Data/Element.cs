public enum Element
{
    None,
    Fire,
    Ice,
    Shock,
    Dark,
    Light
}

public static class ElementChart
{
    public static float GetMultiplier(Element attack, Element defense)
    {
        if (attack == Element.None || defense == Element.None) return 1f;
        if (attack == defense) return 0.5f;

        // Fire > Ice > Shock > Fire
        if (attack == Element.Fire && defense == Element.Ice) return 2f;
        if (attack == Element.Ice && defense == Element.Shock) return 2f;
        if (attack == Element.Shock && defense == Element.Fire) return 2f;

        // Reverse: resisted
        if (attack == Element.Ice && defense == Element.Fire) return 0.5f;
        if (attack == Element.Shock && defense == Element.Ice) return 0.5f;
        if (attack == Element.Fire && defense == Element.Shock) return 0.5f;

        // Dark <> Light mutual weakness
        if (attack == Element.Dark && defense == Element.Light) return 2f;
        if (attack == Element.Light && defense == Element.Dark) return 2f;

        return 1f;
    }
}
