using UnityEngine;

public class MPingPong : MovePattern
{
    public Vector2 Zero = Vector2.down * 5, One = Vector2.up * 5;
    public float Period = 2;

    public override Vector2 Get(float t)
    {
        return Vector2.Lerp(Zero, One, Mathf.PingPong(t / Period, 1));
    }
}
