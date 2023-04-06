using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private bool _inverted;

    public ObjectPool<Transform> Pool;
    private bool _wasHit = false;

    private void OnEnable()
    {
        _rigidbody.velocity = transform.right * 20;
        _wasHit = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_wasHit) return;
        _wasHit = true;

        var enemy = collision.attachedRigidbody.GetComponent<Enemy>();
        if(_inverted == enemy.Inverted)
        {
            enemy.WasHit();
        }

        Pool.Release(transform);
    }

    private void FixedUpdate()
    {
        if (transform.position.x < -10 || transform.position.x > 10)
        {
            Pool.Release(transform);
            return;
        }
        if (transform.position.y < -6 || transform.position.y > 6)
        {
            Pool.Release(transform);
            return;
        }
    }
}