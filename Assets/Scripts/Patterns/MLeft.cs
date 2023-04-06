using UnityEngine;

public class MLeft : MovePattern
{
    public float Speed = 2.5f;

    public override Vector2 Get(float t)
    {
        return Vector2.left * t * Speed;
    }
}
