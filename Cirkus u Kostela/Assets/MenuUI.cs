using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public Button continueButton; // Tlaèítko pro pokraèování

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
        continueButton.onClick.AddListener(ContinueGame);

        // Zobrazí nebo skryje tlaèítko Continue podle stavu v GameManageru
        if (PlayerPrefs.GetInt("CanContinue", 0) == 1)
        {
            continueButton.gameObject.SetActive(true); // Zobrazit "Continue" pokud je to povoleno
            continueButton.transform.SetSiblingIndex(1); // Umístit "Continue" mezi "New Game" a "Quit"
        }
        else
        {
            continueButton.gameObject.SetActive(false); // Skrytí pokud není možné pokraèovat
        }
    }

    private void StartGame()
    {
        GameManager.Instance.NewGame();
    }

    private void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }

    private void ContinueGame()
    {
        GameManager.Instance.NextLevel(); // Pokraèování do dalšího levelu
    }
}