using UnityEngine;

[CreateAssetMenu(menuName = "UNO/Card")]
public class UNOCard : ScriptableObject
{
    public UNOCardType type;
    public UNOCardColor colour;
    public Sprite sprite;
}
