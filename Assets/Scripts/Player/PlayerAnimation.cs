using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAnimation", menuName = "Scriptable Objects/PlayerAnimation")]
public class PlayerAnimation : ScriptableObject
{
    public Sprite[] sprites;
    public float spriteTime = 0.1f;
    public float height = 0f;
}
