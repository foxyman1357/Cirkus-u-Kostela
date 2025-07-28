using UnityEngine;

public class PohybHrace : MonoBehaviour
{
    public float edgeThreshold = 100f;
    public float minRotation = -60f;
    public float maxRotation = 60f;

    private Vector2 screenCenter;
    private float currentYRotation;
    private bool jeZaseknuty = false; // Indikuje, zda je hráè zaseknutý

    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        if (jeZaseknuty) return; // Pokud je hráè zaseknutý, nepohybujeme s ním

        Vector2 mousePosition = Input.mousePosition;

        float distanceFromCenterX = mousePosition.x - screenCenter.x;

        if (Mathf.Abs(distanceFromCenterX) > edgeThreshold)
        {
            float accelerationFactor = (Mathf.Abs(distanceFromCenterX) - edgeThreshold) / (Screen.width / 2 - edgeThreshold);
            accelerationFactor = Mathf.Clamp01(accelerationFactor);

            float turnDirection = Mathf.Sign(distanceFromCenterX);

            float turnSpeed = accelerationFactor * 200;
            currentYRotation += turnDirection * turnSpeed * Time.deltaTime;

            currentYRotation = Mathf.Clamp(currentYRotation, minRotation, maxRotation);

            transform.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
    }

    // Metoda pro zaseknutí nebo uvolnìní hráèe
    public void ZaseknoutHrace(bool zaseknout)
    {
        jeZaseknuty = zaseknout;
    }

    // Metoda pro otoèení hráèe k robotovi
    public void OtoèitHráèeKRobotovi(Transform robot)
    {
        if (robot == null) return;

        // Vypoèítáme smìr k robotovi
        Vector3 smerKRobotovi = robot.position - transform.position;
        smerKRobotovi.y = 0; // Ignorujeme výškový rozdíl

        // Vypoèítáme rotaci, aby se hráè díval na robota
        Quaternion cilovaRotace = Quaternion.LookRotation(smerKRobotovi);

        // Nastavíme rotaci hráèe
        transform.rotation = cilovaRotace;
    }
}