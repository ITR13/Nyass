using UnityEngine;
using System.Collections.Generic;
using System;

public class SpawnManager : MonoBehaviour
{
    public const float Bpm = 140;
    public const float Bps = Bpm / 60;


    public const float Sp16b = 960 / Bpm;
    public const float Sp8b = 480 / Bpm;
    public const float Sp4b = 240 / Bpm;
    public const float Sp2b = 120 / Bpm;
    public const float SpB = 60 / Bpm;

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
    [SerializeField]
    private AudioSource _bgm;

    public float GameTime;
    private float _nextWave = 0;

    private List<Wave> _waves;

    public static event Action OnNextWave;

    private void Awake()
    {
        _waves = new List<Wave>
        {
            new ( 0, Wave1 ),
            new ( Sp16b, Wave2 ),
            new ( Sp16b, Wave3 ),
            new ( Sp16b, Wave4 ),
        };
    }

    private void Start()
    {
        _nextWave = _waves[0].Time;

        if (GameTime <= 0) return;
        _bgm.time = GameTime;

        while (_nextWave < GameTime && _waves.Count == 0)
        {
            _waves.RemoveAt(0);
            _nextWave += _waves[0].Time;
            OnNextWave?.Invoke();
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
        OnNextWave?.Invoke();
    }

    private void Wave1()
    {
        _spawner.Spawn(Sp4b, 3, 1, SpawnFormation.Single, SpawnPattern.Regular, Sp2b, new MLeft());
    }
    private void Wave2()
    {
        _spawner.Spawn(Sp4b, 3, 1, SpawnFormation.Single, SpawnPattern.Inverted, Sp2b, new MLeft());
    }
    private void Wave3()
    {
        _spawner.Spawn(Sp4b, 4, 1, SpawnFormation.Single, SpawnPattern.Alternating, Sp2b,
            new MCombine(new MLeft(), new MSine { Direction = Vector2.up, Period = Sp4b })
        );
    }
    private void Wave4()
    {
        _spawner.Spawn(Sp4b, 4, 1, SpawnFormation.Line, SpawnPattern.Alternating, Sp2b,
            new MCombine(new MLeft(), new MSine { Direction = Vector2.left * 2, Period = Sp4b })
        );
    }
}
