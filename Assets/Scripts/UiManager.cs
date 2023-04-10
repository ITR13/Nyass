using GlobalstatsIO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _tutorial;
    [SerializeField]
    private TextMeshProUGUI _scoreText;

    [SerializeField]
    private GameObject _progress;
    [SerializeField]
    private Image _progressBar;
    [SerializeField]
    private TextMeshProUGUI _progressText;

    [SerializeField]
    private GameObject _highscores1, _highscores2;
    [SerializeField]
    private TextMeshProUGUI _instruction, _nameText;

    [SerializeField]
    private GameObject _scoreEntryPrefab;

    private int _currentTutorial = -1;

    private void Awake()
    {
        SpawnManager.OnNextWave += NextTutorial;
        GameManager.OnScoreChanged += UpdateScore;
        GameManager.OnEndGame += EndGame;

        SpawnManager.OnTimeChanged += UpdateTime;

        UpdateScore(GameManager.Score);
    }

    private void OnDestroy()
    {
        SpawnManager.OnNextWave -= NextTutorial;
        GameManager.OnScoreChanged -= UpdateScore;
        GameManager.OnEndGame -= EndGame;

        SpawnManager.OnTimeChanged -= UpdateTime;
    }
    private void UpdateTime(int time)
    {
        var max = 60 + 60 + 6;
        if(time > max) time = max;

        _progressBar.fillAmount = time / (float)max;
        var minutes = time / 60;
        var seconds = time % 60;
        _progressText.text = $"{minutes:0}:{seconds:00} / 2:06";
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
        _progress.gameObject.SetActive(_currentTutorial >= _tutorial.Length);
    }

    private void UpdateScore(int score)
    {
        _scoreText.text = score.ToString();
    }
    private void EndGame()
    {
        StartCoroutine(ShowScores());
    }

    private IEnumerator ShowScores()
    {
        if (GameManager.Score > PlayerPrefs.GetInt("Highscore", 0))
        {
            PlayerPrefs.SetInt("Highscore", GameManager.Score);
        }

        var client = new GlobalstatsIOClient("uoyL6blSmd98pcsAe7zOCbYnmlRDsnAWM9ITl3LH", "UQG6jhlqQa328qoui2WFDTsMBl8F6QW0GD7ZVtpL");
        _highscores1.SetActive(true);
        _instruction.text = "Enter username:";
        _nameText.text = PlayerPrefs.GetString("Username", "");

        while (true)
        {
            foreach (var letter in Input.inputString)
            {
                if (!char.IsLetterOrDigit(letter))
                {
                    continue;
                }
                _nameText.text += letter;
            }

            if (Input.GetKeyDown(KeyCode.Backspace) && _nameText.text.Length > 0)
            {
                _nameText.text = _nameText.text.Substring(0, _nameText.text.Length - 1);
            }

            if (_nameText.text.Length > 12)
            {
                _nameText.text = _nameText.text.Substring(0, 12);
            }

            if (Input.GetKeyDown(KeyCode.Return) && _nameText.text.Length > 0)
            {
                break;
            }

            yield return null;
        }
        _instruction.text = "Uploading score...";
        PlayerPrefs.SetString("Username", _nameText.text);

        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("scorekey", $"{GameManager.Score}");
        var accuracy = GameManager.Hits == 0 ? 0 : GameManager.Hits / (float)(GameManager.Hits + GameManager.Misses);
        Debug.Log(accuracy);
        values.Add("accuracy", $"{100 * accuracy}");

        // use StartCoroutine to submit the score asynchronously and use the optional callback parameter
        var success = false;
        for (var i = 0; i < 10; i++)
        {
            yield return client.Share(values, "", _nameText.text, wasSuccess =>
            {
                success |= wasSuccess;
                if (!wasSuccess)
                {
                    Debug.LogError("Failed to upload score");
                    _instruction.text = "Failed to upload score... trying again";
                    if (i > 0)
                    {
                        _instruction.text += $" ({i}/10)";
                    }
                }
            });
            yield return null;
            if (success) break;
            yield return new WaitForSeconds(2);
        }

        _instruction.text = "Downloading highscores...";
        Leaderboard leaderboard = null;
        yield return client.GetLeaderboard("scorekey", 8, gottenLeaderboard =>
        {
            leaderboard = gottenLeaderboard;
        });

        _highscores1.SetActive(false);

        foreach (var score in leaderboard.data)
        {
            var entry = Instantiate(_scoreEntryPrefab, _highscores2.transform);
            var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = score.name;
            texts[1].text = score.value;
        }

        _highscores2.SetActive(true);
        yield return new WaitForSeconds(1);

        while (Input.anyKey)
        {
            yield return null;
        }

        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        SceneManager.LoadScene(0);
    }
}
