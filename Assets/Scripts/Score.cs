using UnityEngine;
using UnityEngine.UI;

// Class to update and show the score
public class Score : MonoBehaviour
{
    [SerializeField] private Text totalScoreText;
    [SerializeField] private Text puzzleScoreText;
    
    private readonly Vector3 _changePosition = new (0,-370,0);     // Variables to manipulate the puzzle score
    private const int ChangeFontSize = 160;

    private int _totalScore;
    private int _puzzleScore;
    private int _scorePerPuzzle;

    private int _turnYellowScore;
    private int _turnRedScore;

    // Method to initialize the score values according to the shape count
    public void Initialize(int shapeCount)
    {
        _totalScore = 0;
        _scorePerPuzzle = shapeCount * 2;
        _puzzleScore = _scorePerPuzzle;

        _turnYellowScore = shapeCount - 1;
        _turnRedScore = -1;

        ResetTexts();
    }
    
    public void UpdatePuzzleScore(int decreaseBy)
    {
        _puzzleScore -= decreaseBy;
        puzzleScoreText.text = _puzzleScore.ToString();
        if (_puzzleScore == _turnYellowScore)
        {
            puzzleScoreText.color = Color.yellow;
        }
        else if (_puzzleScore == _turnRedScore)
        {
            puzzleScoreText.color = Color.red;
        }
    }

    public void ProcessCompletePuzzle()
    {
        _totalScore += _puzzleScore;
        _puzzleScore = _scorePerPuzzle;
        
        puzzleScoreText.transform.localPosition -= _changePosition;
        puzzleScoreText.fontSize -= ChangeFontSize;
        
        ResetTexts();
    }

    public void EnlargeScore()
    {
        puzzleScoreText.transform.localPosition += _changePosition;
        puzzleScoreText.fontSize += ChangeFontSize;
    }

    private void ResetTexts()
    {
        totalScoreText.text = _totalScore.ToString();
        puzzleScoreText.text = _scorePerPuzzle.ToString();
        puzzleScoreText.color = Color.green;
    }
}
