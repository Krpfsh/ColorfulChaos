using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using YG;

public class GameplayManager : MonoBehaviour
{
    #region START

    private bool hasGameFinished;

    public static GameplayManager Instance;

    public List<Color> Colors;

    private void Awake()
    {
        Instance = this;

        hasGameFinished = false;
        GameManager.Instance.IsInitialized = true;

        score = 0;
        _scoreText.text = ((int)score).ToString();
        StartCoroutine(SpawnScore());

    }

    #endregion

    #region GAME_LOGIC

    [SerializeField] private ScoreEffect _scoreEffect;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && !hasGameFinished)
        {
            if(CurrentScore == null)
            {
                GameEnded();
                return;
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if(!hit.collider || !hit.collider.gameObject.CompareTag("Block"))
            {
                GameEnded();
                return;
            }

            int currentScoreId = CurrentScore.ColorId;
            int clickedScoreId = hit.collider.gameObject.GetComponent<Player>().ColorId;


            if(currentScoreId != clickedScoreId)
            {
                GameEnded();
                return;
            }

            var t = Instantiate(_scoreEffect, CurrentScore.gameObject.transform.position, Quaternion.identity);
            t.Init(Colors[currentScoreId]);

            var tempScore = CurrentScore;
            if(CurrentScore.NextScore != null)
            {
                CurrentScore = CurrentScore.NextScore;
            }
            Destroy(tempScore.gameObject);

            UpdateScore();
            
        }
    }

    #endregion

    #region SCORE

    private float score;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private AudioClip _pointClip;

    private void UpdateScore()
    {
        score++;
        SoundManager.Instance.PlaySound(_pointClip);
        _scoreText.text = ((int)score).ToString();
    }

    [SerializeField] private float _spawnTime;
    [SerializeField] private Score _scorePrefab;
    private Score CurrentScore;

    private IEnumerator SpawnScore()
    {
        Score prevScore = null;

        while(!hasGameFinished)
        {
            var tempScore = Instantiate(_scorePrefab);

            if(prevScore == null)
            {
                prevScore = tempScore;
                CurrentScore = prevScore;
            }
            else
            {
                prevScore.NextScore = tempScore;
                prevScore = tempScore;
            }

            yield return new WaitForSeconds(_spawnTime);
        }
    }

    #endregion

    #region GAME_OVER

    [SerializeField] private AudioClip _loseClip;
    public UnityAction GameEnd;

    public void GameEnded()
    {
        hasGameFinished = true;
        GameEnd?.Invoke();
        SoundManager.Instance.PlaySound(_loseClip);
        GameManager.Instance.CurrentScore = (int)score;
        StartCoroutine(GameOver());
    }

    [SerializeField] private GameObject _gameOverPopup;
    [SerializeField] private TMP_Text _scoreTextGameOver;
    [SerializeField] private TMP_Text _bestScoreTextGameOver;
    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);
        _gameOverPopup.SetActive(true);
        _scoreTextGameOver.text = GameManager.Instance.CurrentScore.ToString();

        int currentScore = GameManager.Instance.CurrentScore;
        int highScore = GameManager.Instance.HighScore;
        if (highScore < currentScore)
        {
            GameManager.Instance.HighScore = currentScore;
            PlayerPrefs.SetInt("HighScore", currentScore);
            YandexGame.NewLeaderboardScores("LiderBordColorChaos", currentScore);
        }
        _bestScoreTextGameOver.text = PlayerPrefs.GetInt("HighScore").ToString();
    }
    public void RestartButton()
    {
        GameManager.Instance.GoToGameplay();
    }
    #endregion
}
