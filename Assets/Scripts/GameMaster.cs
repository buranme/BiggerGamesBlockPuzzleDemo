using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Object to control the game flow
public class GameMaster : MonoBehaviour
{
    [SerializeField] private Lookup lookup;
    [SerializeField] private Board board;
    [SerializeField] private Score score;
    
    [SerializeField] private Text loadInputText;
    [SerializeField] private Text loadErrorText;
    [SerializeField] private Text saveInputText;
    [SerializeField] private Text saveErrorText;
    
    [SerializeField] private GameObject helpButton;
    [SerializeField] private GameObject backDrop;
    [SerializeField] private GameObject mainMenu;
    
    private PuzzleReadWrite _rw;
    private bool _playSinglePuzzle;
    
    private void Awake()
    {
        lookup.isRunning = false;
        
        _rw = new PuzzleReadWrite();
        lookup.topShapeZ = 0f;
        lookup.cam = Camera.main;
    }

    // Method to generate puzzle
    // The easy, medium and hard buttons call it with difficulty 0, 1, 2 respectively
    // LoadPuzzle() calls it with difficulty of -1, if so gets the size and shape count from the file and
    // Calls loadShapes instead of CreateShapes and FillShapes
    public void GeneratePuzzle(int difficulty)
    {
        lookup.size = difficulty == -1 ? _rw.Size : difficulty + 4;
        lookup.shapeCount = difficulty == -1 ? _rw.ShapeCount : difficulty * 3 + 6;
        lookup.minimumYToSnap = (4 - lookup.size) / 2f;
        lookup.boardOffset = (1 - lookup.size) * 0.5f;
        lookup.originPosition = board.transform.position + new Vector3(lookup.boardOffset, lookup.boardOffset, 0);
        
        board.gameObject.SetActive(true);
        if (!board.initialized)
        {
            board.InitializeBoard();
            score.Initialize(lookup.shapeCount);
        }

        if (difficulty == -1)
        {
            _playSinglePuzzle = true;
            board.LoadShapes(ref _rw.ShapesData);
        }
        else
        {
            _playSinglePuzzle = false;
            board.CreateShapes();
            board.FillShapes();
        }
        lookup.isRunning = true;
    }

    // Gets called by the Load button
    // If the file with the given name doesn't exist it returns
    public void LoadPuzzle()
    {
        if (!_rw.LoadData(loadInputText.text))
        {
            loadErrorText.enabled = true;
            gameObject.SetActive(false);
            return;
        }
        
        GeneratePuzzle(-1);
        loadErrorText.transform.parent.gameObject.SetActive(false);
    }

    // Gets called by the Save button, passes the size, shape count and shapes to PuzzleReadWrite
    // If no name is given it returns
    public void SavePuzzle()
    {
        if (saveInputText.text == "")
        {
            saveErrorText.enabled = true;
            return;
        }

        board.gameObject.SetActive(true);
        saveErrorText.enabled = false;
        saveErrorText.transform.parent.gameObject.SetActive(false);
        
        var fileName = saveInputText.text;
        var shapes = board.GetShapes();
        _rw.SaveData(fileName, lookup.size, lookup.shapeCount, ref shapes);
        
        lookup.isRunning = true;
    }

    public void DecreaseScore()
    {
        score.UpdatePuzzleScore(1);
    }

    // Method to check for end game. According to what is chosen in the Scriptable Object uses one of the algorithms
    // If the algorithm returns true continues to call the coroutine ProcessEnd
    public void ProcessMove()
    {
        if ((int)lookup.algorithm == 0)
        {
            if (!board.PositionCheckAlgorithm()) return;
        }
        else if ((int)lookup.algorithm == 1)
        {
            if (!board.RaycastCheckAlgorithm()) return;
        }
        else return;

        StartCoroutine(ProcessEnd());
    }

    // Gets called by the question mark button on top right. Can be used once per puzzle
    public void GiveHint()
    {
        score.UpdatePuzzleScore(lookup.size);
        board.PlaceAShapeCorrectly();
    }

    // Method to end the puzzle and generate a new one
    private IEnumerator ProcessEnd()
    {
        score.EnlargeScore();
        backDrop.SetActive(true);
        yield return new WaitForSeconds(1);
        
        score.ProcessCompletePuzzle();
        backDrop.SetActive(false);
        board.RefreshLists();
        
        if (_playSinglePuzzle)
        {
            mainMenu.SetActive(true);
            board.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        else
        {
            board.CreateShapes();
            board.FillShapes();
            helpButton.SetActive(true);
        }
    }

    public void SetIsRunning(bool set)
    {
        lookup.isRunning = set;
    }
}
