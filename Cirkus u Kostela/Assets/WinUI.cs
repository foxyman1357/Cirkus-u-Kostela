using UnityEngine;
using UnityEngine.UI;

public class WinUI : MonoBehaviour
{
    public Button nextLevelButton;
    public Button goToMainMenuButton;

    private void Start()
    {
        // Pro tlaèítko pro pøechod na další level
        nextLevelButton.onClick.AddListener(NextLevel);

        // Pro tlaèítko pro návrat do hlavního menu
        goToMainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void NextLevel()
    {
        GameManager.Instance.NextLevel(); // Pokraèuje na další level
    }

    private void GoToMainMenu()
    {
        GameManager.Instance.GoToMainMenu(); // Návrat do hlavního menu
    }
}
