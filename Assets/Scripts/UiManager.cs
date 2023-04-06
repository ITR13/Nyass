using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _tutorial;
    [SerializeField]
    private TextMeshProUGUI _scoreText;

    private int _currentTutorial = -1;

    private void Awake()
    {
        SpawnManager.OnNextWave += NextTutorial;
        GameManager.OnScoreChanged += UpdateScore;
        UpdateScore(GameManager.Score);
    }

    private void OnDestroy()
    {
        SpawnManager.OnNextWave -= NextTutorial;
        GameManager.OnScoreChanged -= UpdateScore;
    }

    private void NextTutorial()
    {
        if (_currentTutorial > 8)
        {
            return;
        }
        _currentTutorial++;
        for (var i = 0; i < _tutorial.Length; i++)
        {
            _tutorial[i].SetActive(i == _currentTutorial);
        }
        _scoreText.enabled = _currentTutorial >= _tutorial.Length - 1;
    }

    private void UpdateScore(int score)
    {
        _scoreText.text = score.ToString();
    }
}
