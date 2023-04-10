using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

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
    private Spawner _spawner, _littleSpawner, _topSpawner, _bottomSpawner;
    [SerializeField]
    private AudioSource _bgm, _sfx;

    public float GameTime;
    private int _prevGameTime;

    private float _nextWave = 0;

    private List<Wave> _waves;

    public static event Action OnNextWave;
    public static event Action<int> OnTimeChanged;

    private HashSet<Enemy> wave5Enemies, wave6Enemies, wave13Enemies;

    private bool _paused;
    [SerializeField]
    private GameObject _pauseMenu;

    private void Awake()
    {
        _waves = new List<Wave>
        {
            new ( 0, Wave1 ),
            new ( Sp16b, Wave2 ),
            new ( Sp16b, Wave3 ),
            new ( Sp16b, Wave4 ),
            new ( Sp16b, Wave5 ),
            new ( Sp16b, Wave6 ),
            new ( Sp16b, Wave7 ),
            new ( Sp16b, Wave8 ),
            new ( Sp16b, Wave9 ),
            new ( Sp16b, Wave10 ),
            new ( Sp16b, Wave11 ),
            new ( Sp8b, Wave12 ),
            new ( Sp16b, Wave13 ),
            new ( Sp16b+Sp8b, Wave14 ),
            new ( Sp16b, Wave15 ),
            new ( Sp16b+Sp8b, Wave16 ),
            new ( Sp16b+Sp8b, Wave17 ),
        };

        wave5Enemies = null;
        wave6Enemies = null;

        _paused = false;
    }

    private void Start()
    {
        _nextWave = _waves[0].Time;

        var bgmVolume = PlayerPrefs.GetInt("BgmVolume", 1);
        var sfxVolume = PlayerPrefs.GetInt("SfxVolume", 1);
        _bgm.volume = bgmVolume;
        _sfx.volume = sfxVolume;

        if (GameTime <= 0) return;
        _bgm.time = GameTime;

        while (_nextWave < GameTime && _waves.Count > 1)
        {
            _waves.RemoveAt(0);
            _nextWave += _waves[0].Time;
            OnNextWave?.Invoke();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _waves.Count > 0)
        {
            TogglePause();
            return;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            _bgm.volume = 1 - _bgm.volume;
            PlayerPrefs.SetInt("BgmVolume", Mathf.RoundToInt(_bgm.volume));
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            _sfx.volume = 1 - _sfx.volume;
            PlayerPrefs.SetInt("SfxVolume", Mathf.RoundToInt(_sfx.volume));
        }

        if (_paused)
        {
            if (_waves.Count == 0)
            {
                TogglePause();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
                TogglePause();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                for (var i = 0; i < _waves.Count; i++)
                {
                    OnNextWave?.Invoke();
                    GameManager.AddShot(false);
                }
                _waves.Clear();
                _nextWave = GameTime;
                GameTime = 60 + 60 + 6;
                TogglePause();
            }
            return;
        }

        GameTime += Time.deltaTime;

        var timeInt = Mathf.FloorToInt(GameTime);
        if (_prevGameTime != timeInt)
        {
            _prevGameTime = timeInt;
            OnTimeChanged?.Invoke(timeInt);
        }

        if (_nextWave > GameTime) return;
        if (_waves.Count == 0)
        {
            _nextWave = 10000;
            GameManager.EndGame();
            return;
        }

        var action = _waves[0].Action;
        _waves.RemoveAt(0);
        _nextWave += _waves.Count == 0 ? Sp16b * 2 : _waves[0].Time;

        action();
        OnNextWave?.Invoke();
    }

    private void TogglePause()
    {
        _paused = !_paused;

        if (_paused)
        {
            _pauseMenu.SetActive(true);
            _bgm.Pause();
            Time.timeScale = 0;
        }
        else
        {
            _pauseMenu.SetActive(false);
            _bgm.time = GameTime;
            _bgm.Play();
            Time.timeScale = 1;
        }
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
        _littleSpawner.Spawn(Sp4b, 4, 1, SpawnFormation.Line, SpawnPattern.Regular, 0,
            new MCombine(new MLeft(), new MSine { Direction = Vector2.left * 1, Period = Sp4b })
        );
    }

    private void Wave5()
    {
        wave5Enemies = _littleSpawner.Spawn(0, 8, 1, SpawnFormation.Circle, SpawnPattern.Inverted, SpB,
            new MCombine(new MLeft(), new MSine { Direction = Vector2.up * 1, Period = Sp4b })
        );
    }

    private void Wave6()
    {
        if (wave5Enemies != null && wave5Enemies.Count != 0)
        {
            Debug.Log("Skipping wave 6");
            return;
        }
        wave6Enemies = _spawner.Spawn(0, 8, 1, SpawnFormation.Line, SpawnPattern.Alternating, SpB,
            new NEaseInOut(new MCombine(new MLeft(), new MSine { Direction = Vector2.up, Period = Sp4b }, new MSine { Direction = Vector2.down, Period = Sp2b }), SpB)
        );
    }


    private void Wave7()
    {
        if (wave6Enemies != null && wave6Enemies.Count != 0)
        {
            Debug.Log("Skipping wave 7");
            return;
        }
        _spawner.Spawn(0, 4, 1, SpawnFormation.Line, SpawnPattern.Regular, Sp2b,
            new MCombine(new MLeft())
        );
        _spawner.Spawn(Sp2b * 3, 4, 1, SpawnFormation.Line, SpawnPattern.Inverted, -Sp2b,
            new MCombine(new MLeft())
        );
    }

    private void Wave8()
    {
        _spawner.Spawn(0, 8, 1, SpawnFormation.Single, SpawnPattern.Regular, SpB,
            new MCombine(new MLeft(), new NEaseInOut(new MCombine(
                    new MSine { Direction = Vector2.up * 4, Period = Sp4b },
                    new MSine { Direction = Vector2.right * 2, Period = Sp4b, Offset = 0.25f }
                ),
                Sp4b
            ))
        );
    }

    private void Wave9()
    {
        _spawner.Spawn(0, 8, 1, SpawnFormation.Single, SpawnPattern.Inverted, SpB,
            new MCombine(new MLeft(), new NEaseInOut(new MPingPong(), Sp2b))
        );
    }

    private void Wave10()
    {
        _topSpawner.Spawn(0, 2, 1, SpawnFormation.Single, SpawnPattern.Regular, SpB, new NEaseInOut(new MLeft(), SpB));
        _bottomSpawner.Spawn(Sp2b, 2, 1, SpawnFormation.Single, SpawnPattern.Inverted, SpB, new NEaseInOut(new MLeft(), SpB));
        _topSpawner.Spawn(Sp4b, 4, 1, SpawnFormation.Single, SpawnPattern.Alternating, SpB, new NEaseInOut(new MLeft(), SpB));
        _bottomSpawner.Spawn(Sp2b * 4, 4, 1, SpawnFormation.Single, SpawnPattern.Alternating, SpB, new NEaseInOut(new MLeft(), SpB));
    }

    private void Wave11()
    {
        _spawner.Spawn(0, 2, 2, SpawnFormation.Single, SpawnPattern.Alternating, Sp4b, new MLeft { Speed = 1.5f });
    }

    private void Wave12()
    {
        _spawner.Spawn(0, 8, 2, SpawnFormation.Single, SpawnPattern.Alternating, Sp2b, new NEaseInOut(new MLeft { Speed = 1.5f }, Sp4b));
    }

    private void Wave13()
    {
        wave13Enemies = _littleSpawner.Spawn(0, 8, 1, SpawnFormation.Circle, SpawnPattern.Alternating, Sp2b,
            new MCombine(new MLeft(), new NEaseInOut(new MPingPong { One = Vector2.up * 2, Zero = Vector2.down * 2, Period = SpB }, Sp2b))
        );
    }

    private void Wave14()
    {
        if (wave13Enemies != null && wave13Enemies.Count > 0)
        {
            Debug.Log("Spawning easy wave 14");
            _spawner.Spawn(Sp2b * 2, 4, 1, SpawnFormation.Line, SpawnPattern.Regular, Sp2b,
                new MCombine(new MLeft(), new MSine { Direction = Vector2.up * 0.25f, Period = Sp4b })
            );
            _spawner.Spawn(Sp2b * 5, 4, 1, SpawnFormation.Line, SpawnPattern.Inverted, -Sp2b,
                new MCombine(new MLeft(), new MSine { Direction = Vector2.down * 0.25f, Period = Sp4b })
            );
            return;
        }

        _spawner.Spawn(0, 6, 1, SpawnFormation.Line, SpawnPattern.Regular, Sp2b,
            new MCombine(new MLeft(), new MSine { Direction = Vector2.up * 0.25f, Period = Sp4b })
        );
        _spawner.Spawn(Sp2b * 5, 6, 1, SpawnFormation.Line, SpawnPattern.Inverted, -Sp2b,
            new MCombine(new MLeft(), new MSine { Direction = Vector2.down * 0.25f, Period = Sp4b })
        );
    }
    private void Wave15()
    {
        _spawner.Spawn(0, 16, 1, SpawnFormation.Single, SpawnPattern.Regular, SpB / 2,
            new MCombine(new MLeft { Speed = 0.8f }, new NEaseInOut(new MCombine(
                    new MSine { Direction = Vector2.up * 4, Period = Sp4b },
                    new MSine { Direction = Vector2.right * 2, Period = Sp4b, Offset = 0.25f }
                ),
                Sp4b
            ))
        );
    }

    private void Wave16()
    {
        _topSpawner.Spawn(0, 8, 1, SpawnFormation.Single, SpawnPattern.Regular, Sp2b,
            new NEaseInOut(new MCombine(new MLeft(), new MPingPong { One = Vector2.down * 2, Zero = Vector2.zero, Period = Sp2b }), Sp2b)
        );
        _bottomSpawner.Spawn(SpB, 8, 1, SpawnFormation.Single, SpawnPattern.Inverted, Sp2b,
            new NEaseInOut(new MCombine(new MLeft(), new MPingPong { One = Vector2.up * 2, Zero = Vector2.zero, Period = Sp2b }), Sp2b)
        );
    }

    private void Wave17()
    {
        _spawner.Spawn(0, 1, 16, SpawnFormation.Single, SpawnPattern.Regular, Sp4b, new MLeft { Speed = 1f }, true);
    }
}
