using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TV : MonoBehaviour
{
    [System.Serializable]
    public class CameraWaypoints
    {
        public Camera camera;
        public List<Transform> watchedWaypoints;
    }

    public List<CameraWaypoints> cameraWaypointData = new List<CameraWaypoints>();
    private List<Camera> cameras = new List<Camera>();

    private Camera ActiveCam;
    private Camera mainCamera;
    private int PosledniCam = 1;
    public bool VKamerach = false;

    public Canvas playerUICanvas;
    public Canvas cameraUICanvas;

    public MonoBehaviour Hoření;
    public float baseRequiredTimeOnCameras = 20f; // výchozí čas přehřátí (noc 1)
    private float requiredTimeOnCameras;
    private float cameraTimer = 0f;
    private float timerDecreaseRate = 1f;
    private bool isScriptActivated = false;

    public List<Laboři> foxyRobots;
    public Button resetButton;

    [Header("Přehřívání kamery UI")]
    public Image overheatingOverlay;
    public Slider overheatingSlider;

    void Start()
    {
        // Inicializace kamer
        foreach (CameraWaypoints data in cameraWaypointData)
        {
            if (data.camera != null)
            {
                cameras.Add(data.camera);
            }
        }

        if (cameras.Count == 0)
        {
            Debug.LogError("Žádné kamery nebyly nalezeny v cameraWaypointData!");
            return;
        }

        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].gameObject.SetActive(i == 0);
        }

        ActiveCam = cameras[0];
        mainCamera = cameras[0];

        playerUICanvas.gameObject.SetActive(true);
        cameraUICanvas.gameObject.SetActive(false);

        if (Hoření != null)
        {
            Hoření.enabled = false;
        }

        if (resetButton != null)
        {
            resetButton.gameObject.SetActive(false);
            resetButton.onClick.AddListener(TryResetFoxys);
        }

        // === Nastavení času podle levelu ===
        int noc = GameManager.Instance != null ? GameManager.Instance.sunday : 1;
        float scaling = 0.25f; // každá noc o 25 % rychlejší
        requiredTimeOnCameras = baseRequiredTimeOnCameras / (1f + (noc - 1) * scaling);
    }

    void Update()
    {
        if (VKamerach)
        {
            cameraTimer += Time.deltaTime;

            if (cameraTimer >= requiredTimeOnCameras)
            {
                ActivateTargetScript();
            }

            if (resetButton != null)
            {
                bool horeniIsActive = Hoření != null && Hoření.enabled;

                // Tlačítko se zobrazí jen pokud Hoření není aktivní
                if (!horeniIsActive && CanAnyFoxyBeReset())
                {
                    resetButton.gameObject.SetActive(true);
                }
                else
                {
                    resetButton.gameObject.SetActive(false);
                }
            }

            // === Přehřívání UI efekty ===
            float ratio = cameraTimer / requiredTimeOnCameras;

            if (overheatingOverlay != null)
            {
                if (ratio >= 0.75f && (Hoření == null || !Hoření.enabled))
                {
                    Color overlayColor = overheatingOverlay.color;
                    overlayColor.a = Mathf.Clamp01((ratio - 0.75f) * 4f * 0.6f);
                    overheatingOverlay.color = overlayColor;
                }
                else
                {
                    Color overlayColor = overheatingOverlay.color;
                    overlayColor.a = 0f;
                    overheatingOverlay.color = overlayColor;
                }
            }

            if (overheatingSlider != null)
            {
                overheatingSlider.value = ratio;
            }
        }
        else
        {
            if (cameraTimer > 0)
            {
                cameraTimer -= timerDecreaseRate * Time.deltaTime;
                cameraTimer = Mathf.Max(cameraTimer, 0f);
            }

            if (resetButton != null)
            {
                resetButton.gameObject.SetActive(false);
            }

            if (overheatingOverlay != null)
            {
                Color overlayColor = overheatingOverlay.color;
                overlayColor.a = 0f;
                overheatingOverlay.color = overlayColor;
            }

            if (overheatingSlider != null)
            {
                overheatingSlider.value = 0f;
            }
        }
        // Projdi všechny roboty
        foreach (var robot in foxyRobots)
        {
            if (robot == null) continue;

            // Zjisti, jestli je robot na posledním waypointu poslední kamery
            var lastCamData = cameraWaypointData[cameraWaypointData.Count - 1];
            var lastCamWaypoints = lastCamData.watchedWaypoints;

            var currentWaypoint = robot.GetCurrentWaypoint();
            if (currentWaypoint != null && lastCamWaypoints.Contains(currentWaypoint))
            {
                // Přepni na poslední kameru
                PřepniNaPosledniKameru();
                break; // přepni jen jednou
            }
        }

    }

    private void OnMouseDown()
    {
        if (!VKamerach)
        {
            VKamerach = true;
            InCams(PosledniCam);
        }
    }

    public void InCams(int i)
    {
        if (i < 0 || i >= cameras.Count) return;

        ActiveCam.gameObject.SetActive(false);
        ActiveCam = cameras[i];
        ActiveCam.gameObject.SetActive(true);

        PosledniCam = i;

        playerUICanvas.gameObject.SetActive(false);
        cameraUICanvas.gameObject.SetActive(true);
    }

    private void ReturnToMainCamera()
    {
        if (!VKamerach) return;

        ActiveCam.gameObject.SetActive(false);
        ActiveCam = mainCamera;
        ActiveCam.gameObject.SetActive(true);

        VKamerach = false;

        playerUICanvas.gameObject.SetActive(true);
        cameraUICanvas.gameObject.SetActive(false);

        isScriptActivated = false;   // Reset stavu pro další kolo
        cameraTimer = 0f;            // Reset času
    }



    private void ActivateTargetScript()
    {
        if (Hoření != null && !isScriptActivated)
        {
            cameraTimer = 0f;                      // Reset timer
            Hoření.enabled = true;                 // Aktivuj Hoření
            isScriptActivated = true;              // Zamezí opakování

            if (resetButton != null)
            {
                resetButton.gameObject.SetActive(false); // Skryj tlačítko
            }

            // Vizuálně vypni efekt
            if (overheatingOverlay != null)
            {
                Color overlayColor = overheatingOverlay.color;
                overlayColor.a = 0f;
                overheatingOverlay.color = overlayColor;
            }

            if (overheatingSlider != null)
            {
                overheatingSlider.value = 0f;
            }
        }
    }


    private void TryResetFoxys()
    {
        if (Hoření != null && Hoření.enabled)
        {
            Debug.LogWarning("Reset nebyl povolen – TV je přehřátá.");
            return;
        }

        bool anyReset = false;

        foreach (Laboři foxy in foxyRobots)
        {
            if (foxy != null && IsFoxyOnWatchedWaypoint(foxy) && !foxy.IsAtFirstWaypoint())
            {
                foxy.ResetToNextAction();
                Debug.Log(foxy.name + " byl resetován na první waypoint.");
                anyReset = true;
            }
        }

        if (anyReset)
        {
            ActivateTargetScript(); // Aktivuj Hoření po úspěšném resetu
        }
    }


    private bool CanAnyFoxyBeReset()
    {
        foreach (Laboři foxy in foxyRobots)
        {
            if (foxy != null && IsFoxyOnWatchedWaypoint(foxy) && !foxy.IsAtFirstWaypoint())
            {
                return true;
            }
        }
        return false;
    }

    private bool IsFoxyOnWatchedWaypoint(Laboři foxy)
    {
        if (ActiveCam == null || foxy == null) return false;

        List<Transform> watchedWaypoints = null;

        foreach (CameraWaypoints data in cameraWaypointData)
        {
            if (data.camera == ActiveCam)
            {
                watchedWaypoints = data.watchedWaypoints;
                break;
            }
        }

        if (watchedWaypoints == null) return false;

        Transform foxyWaypoint = foxy.GetCurrentWaypoint();
        return watchedWaypoints.Contains(foxyWaypoint);
    }

    public Camera GetActiveCamera()
    {
        return ActiveCam;
    }
    public void ResetCameraTimer()
    {
        cameraTimer = 0f;
        isScriptActivated = false;
    }
    private void PřepniNaPosledniKameru()
    {
        // Vypni všechny kamery
        foreach (var cam in cameras)
        {
            cam.gameObject.SetActive(false);
        }

        // Zapni poslední kameru
        var posledniCam = cameraWaypointData[cameraWaypointData.Count - 1].camera;
        posledniCam.gameObject.SetActive(true);
        ActiveCam = posledniCam;

        VKamerach = true;

        playerUICanvas.gameObject.SetActive(false);
        cameraUICanvas.gameObject.SetActive(true);

        Debug.Log("Přepnuto na poslední kameru (jumpscare)!");
    }
}
