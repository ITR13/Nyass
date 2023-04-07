using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour
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

    private bool _grow;
    private bool _missed;

    private void OnEnable()
    {
        Bullet.OnMissedShot += OnMiss;
    }

    private void OnDisable()
    {
        Bullet.OnMissedShot -= OnMiss;
    }


    private void OnMiss()
    {
        _missed = true;
    }

    public void SetStats(Vector2 startPosition, int health, float t, MovePattern movement, HashSet<Enemy> group, bool grow = false)
    {
        _startPosition = startPosition;
        transform.position = startPosition;
        _health = health;
        _t = t;
        _movement = movement;
        gameObject.SetActive(true);
        _group = group;

        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        _grow = grow;

        _missed = false;
    }

    public void WasHit()
    {
        _health -= 1;
        if (_health > 0)
        {
            GameManager.AddScore(5);
            transform.Rotate(0, 0, 45f);
            if (_grow)
            {
                transform.localScale *= 1.05f;
            }
            AudioManager.Dent();
            return;
        }
        if (_grow)
        {
            AudioManager.Explode();
        }
        else
        {
            AudioManager.Break();
        }

        var score = 20;
        _group.Remove(this);
        if (_group.Count == 0)
        {
            score += 100;
            if (!_missed)
            {
                Debug.Log("Perfect!");
                score += 20;
            }
        }
        GameManager.AddScore(score);

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
            GameManager.AddShot(false);
            Pool.Release(this);
        }
    }

}
