using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private Transform _gun;

    [SerializeField]
    private Transform _regularBulletPrefab, _invertedBulletPrefab;
    private ObjectPool<Transform> _regularBulletPool, _invertedBulletPool;

    public const float TimePerShot = SpawnManager.SpB;

    private bool _lastShotWasInverted;
    private float _shotTimer;

    private float _invulnerableTimer;
    [SerializeField]
    private Renderer _renderer;

    private void Awake()
    {
        void onGet(Transform bullet)
        {
            bullet.position = _gun.position;
            bullet.rotation = _gun.rotation;
            bullet.gameObject.SetActive(true);
        }

        void onRelease(Transform bullet)
        {
            bullet.gameObject.SetActive(false);
        }

        _regularBulletPool = new ObjectPool<Transform>(
            () =>
            {
                var bullet = Instantiate(_regularBulletPrefab);
                bullet.GetComponent<Bullet>().Pool = _regularBulletPool;
                bullet.gameObject.SetActive(false);
                return bullet;
            },
            onGet,
            onRelease
        );
        _invertedBulletPool = new ObjectPool<Transform>(
            () =>
            {
                var bullet = Instantiate(_invertedBulletPrefab);
                bullet.GetComponent<Bullet>().Pool = _invertedBulletPool;
                bullet.gameObject.SetActive(false);
                return bullet;
            },
            onGet,
            onRelease
        );

        GameManager.StartGame();
    }

    private void Update()
    {
        var velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _rigidbody.velocity = velocity * 5;


        if (_shotTimer >= TimePerShot)
        {
            _shotTimer = TimePerShot;
        }
        else
        {
            _shotTimer += Time.deltaTime;
            if (_shotTimer < TimePerShot) return;
        }

        var shootRegular = Input.GetButton("Fire1");
        var shootInverted = Input.GetButton("Fire2");

        if (!shootRegular && !shootInverted)
        {
            return;
        }
        _shotTimer -= TimePerShot;
        AudioManager.Shoot();

        _lastShotWasInverted = shootRegular && shootInverted ? !_lastShotWasInverted : shootInverted;
        var bullet = (_lastShotWasInverted ? _invertedBulletPool : _regularBulletPool).Get();
    }

    private void FixedUpdate()
    {
        if (_invulnerableTimer <= 0) return;
        _invulnerableTimer -= Time.fixedDeltaTime;
        if (_invulnerableTimer <= 0)
        {
            _renderer.material.SetInt("_Inverted", 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            enemy.Pool.Release(enemy);
        }

        if (_invulnerableTimer > 0) return;
        _renderer.material.SetInt("_Inverted", 1);
        _invulnerableTimer = 3;

        GameManager.AddScore(-50);
    }
}
