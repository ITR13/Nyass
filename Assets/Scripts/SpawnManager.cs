using UnityEngine;
using System.Collections.Generic;
using System;

public class SpawnManager : MonoBehaviour
{
    private struct Wave
    {
        public float Time;
        public Action Action;

        public Wave(float time, Action action)
        {
            Time = time;
            Action = action;
        }
    }

    [SerializeField]
    private Spawner _spawner;

    public float GameTime;
    private float _nextWave = 0;

    private List<Wave> _waves;

    private void Awake()
    {
        _waves = new List<Wave>
        {
            new ( 1, Wave1 ),
            new ( 10, Wave2 ),
            new ( 10, Wave3 ),
        };
    }

    private void Start()
    {
        _nextWave = _waves[0].Time;
        while (_nextWave < GameTime && _waves.Count == 0)
        {
            _waves.RemoveAt(0);
            _nextWave += _waves[0].Time;
        }
    }

    private void Update()
    {
        GameTime += Time.deltaTime;
        if (_nextWave > GameTime) return;
        if (_waves.Count == 0)
        {
            _nextWave = 10000;
            GameManager.EndGame();
            return;
        }

        var action = _waves[0].Action;
        _waves.RemoveAt(0);
        _nextWave += _waves.Count == 0 ? 10 : _waves[0].Time;
        
        action();
    }

    private void Wave1()
    {
        _spawner.Spawn(3, 1, SpawnFormation.Single, SpawnPattern.Regular, 0.5f, new MLeft());
    }
    private void Wave2()
    {
        _spawner.Spawn(3, 1, SpawnFormation.Single, SpawnPattern.Inverted, 0.5f, new MLeft());
    }
    private void Wave3()
    {
        _spawner.Spawn(4, 1, SpawnFormation.Single, SpawnPattern.Alternating, 0.5f, new MLeft());
    }
}
