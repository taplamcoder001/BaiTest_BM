using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MazeUIView : MonoBehaviour {
    public MazeDataSO MazeDataSO;
    public MazeController mazeController;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Button generatorButton;
    public Button findPathButton;

    private void Start()
    {
        generatorButton.onClick.AddListener(OnGeneratorButtonClicked);
        findPathButton.onClick.AddListener(OnFindPathButtonClicked);
    }

    private void OnFindPathButtonClicked()
    {
        mazeController.FindAndMoveAgent();
    }

    private void OnGeneratorButtonClicked()
    {
        if (!int.TryParse(widthInput.text, out int width) ||
            !int.TryParse(heightInput.text, out int height))
        {
            Debug.LogError("Width/Height must be integer numbers");
            return;
        }

        if (width <= 4 || height <= 4)
        {
            Debug.LogError("Width/Height must be greater than 4");
            return;
        }

        mazeController.GenerateMaze(height,width);
    }
}