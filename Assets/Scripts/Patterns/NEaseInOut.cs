using UnityEngine;

public class NEaseInOut : MovePattern
{
    public MovePattern Pattern;
    public float Period;

    public NEaseInOut(MovePattern pattern)
    {
        Pattern = pattern;
    }

    public override Vector2 Get(float t)
    {
        var count = Mathf.FloorToInt(t / Period);
        t = (t - count * Period) / Period;
        t = t * t * (3.0f - 2.0f * t);
        t = (t + count) * Period;
        return Pattern.Get(t);
    }
}