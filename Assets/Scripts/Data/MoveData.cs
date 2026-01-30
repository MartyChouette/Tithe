using UnityEngine;

public enum TargetType
{
    Single,
    All
}

[CreateAssetMenu(fileName = "NewMove", menuName = "Tithe/Move")]
public class MoveData : ScriptableObject
{
    public string moveName;
    public int power;
    public Element element;
    public TargetType targetType;
    [TextArea] public string description;
}
