using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GameManager;

public class TimeGame : MonoBehaviour

{
    public TMP_Text timeText;

   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    
    private void Update()
    {
        if (GameManager.Instance != null)
        {
            float timeLeft = GameManager.Instance.GetTimeRemaining();
            int hodina = Mathf.FloorToInt(timeLeft / 20);
            int min = Mathf.FloorToInt((timeLeft % 20)* 3);
            timeText.text = $"{hodina:00}:{min:00}/6:00";
        }
    }
}
