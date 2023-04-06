using UnityEngine;

public class MPingPong : MovePattern
{
    public Vector2 Zero = Vector2.down * 4, One = Vector2.up * 4;
    public float Period = SpawnManager.Sp2b;

    public override Vector2 Get(float t)
    {
        return Vector2.Lerp(Zero, One, Mathf.PingPong(t / Period, 1));
    }
}
