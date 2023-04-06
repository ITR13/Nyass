using UnityEngine;

public class MSine : MovePattern
{
    public Vector2 Direction;
    public float Period;

    public override Vector2 Get(float t)
    {
        return Direction * Mathf.Sin(t * Mathf.PI * 2 / Period);
    }
}
