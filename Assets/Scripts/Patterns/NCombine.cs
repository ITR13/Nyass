
using UnityEngine;

class MCombine : MovePattern
{
    public MovePattern[] Patterns;
    public MCombine(params MovePattern[] patterns)
    {
        Patterns = patterns;
    }

    public override Vector2 Get(float t)
    {
        var pos = Vector2.zero;
        for (var i = 0; i < Patterns.Length; i++)
        {
            pos += Patterns[i].Get(t);
        }
        return pos;
    }
}