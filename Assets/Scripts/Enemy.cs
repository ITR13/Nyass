using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

class Enemy : MonoBehaviour
{
    [field: SerializeField] public bool Inverted { get; private set; }
    [SerializeField]
    private Rigidbody2D _rigidbody;

    public ObjectPool<Enemy> Pool;

    private Vector2 _startPosition;
    private int _health;
    private float _t;
    private MovePattern _movement;

    private HashSet<Enemy> _group;

    public void SetStats(Vector2 startPosition, int health, float t, MovePattern movement, HashSet<Enemy> group)
    {
        _startPosition = startPosition;
        transform.position = startPosition;
        _health = health;
        _t = t;
        _movement = movement;
        gameObject.SetActive(true);
        _group = group;
    }

    public void WasHit()
    {
        _health -= 1;
        if (_health > 0)
        {
            return;
        }

        GameManager.AddScore(10);
        _group.Remove(this);
        if (_group.Count == 0)
        {
            GameManager.AddScore(200);
        }


        Pool.Release(this);
    }

    private void FixedUpdate()
    {
        _t += Time.fixedDeltaTime;
        if (_t < 0) return;
        var offset = _movement.Get(_t) + Mathf.Clamp01(Mathf.Sqrt(_t / SpawnManager.SpB)) * Vector2.left * 1.5f;

        _rigidbody.MovePosition(_startPosition + offset);

        if (transform.position.x < -10)
        {
            Pool.Release(this);
        }
    }

}
