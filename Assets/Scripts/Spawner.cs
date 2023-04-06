using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    public float MaxHeight;

    [SerializeField]
    private Enemy _regularPrefab, _invertedPrefab;

    private ObjectPool<Enemy> _regularPool, _invertedPool;
    private bool _lastSpawnedWasInverted;

    private void Awake()
    {
        void onRelease(Enemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }

        _regularPool = new ObjectPool<Enemy>(
            () =>
            {
                var enemy = Instantiate(_regularPrefab);
                enemy.gameObject.SetActive(false);
                enemy.Pool = _regularPool;
                return enemy;
            },
            null, onRelease
        );
        _invertedPool = new ObjectPool<Enemy>(
            () =>
            {
                var enemy = Instantiate(_invertedPrefab);
                enemy.gameObject.SetActive(false);
                enemy.Pool = _invertedPool;
                return enemy;
            },
            null, onRelease
        );
    }


    public void Spawn(int count, int health, SpawnFormation formation, SpawnPattern pattern, float delay, MovePattern movement)
    {
        var enemies = new HashSet<Enemy>();
        for (var i = 0; i < count; i++)
        {
            var isInverted = pattern == SpawnPattern.Inverted;
            if (pattern == SpawnPattern.Alternating)
            {
                _lastSpawnedWasInverted = !_lastSpawnedWasInverted;
                isInverted = _lastSpawnedWasInverted;
            }

            var spawnPoint = transform.position;
            if (formation == SpawnFormation.Line)
            {
                var percent = i / (float)(count - 1);
                spawnPoint.y += Mathf.Lerp(-MaxHeight, MaxHeight, percent);
            }
            else if (formation == SpawnFormation.Circle)
            {
                var percent = i / (float)count;
                spawnPoint += new Vector3(
                    Mathf.Cos(percent * Mathf.PI * 2),
                    Mathf.Sin(percent * Mathf.PI * 2) * MaxHeight
                );
            }

            var enemy = (isInverted ? _invertedPool : _regularPool).Get();
            enemies.Add(enemy);
            enemy.SetStats(spawnPoint, health, -delay * i, movement, enemies);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.down * MaxHeight, transform.position + Vector3.up * MaxHeight);
    }
}