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
    public float requiredTimeOnCameras = 20f;
    private float cameraTimer = 0f;
    private float timerDecreaseRate = 1f;
    private bool isScriptActivated = false;

    public List<Laboři> foxyRobots; // Seznam více robotů
    public Button resetButton; // UI tlačítko pro reset Foxyho

    void Start()
    {
        // Automaticky vyplní seznam kamer z cameraWaypointData
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

            bool foxyCanBeReset = CanAnyFoxyBeReset();
            if (resetButton != null)
            {
                resetButton.gameObject.SetActive(foxyCanBeReset);
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

    public void ReturnToMainCamera()
    {
        if (!VKamerach) return;

        ActiveCam.gameObject.SetActive(false);
        ActiveCam = mainCamera;
        ActiveCam.gameObject.SetActive(true);

        VKamerach = false;

        playerUICanvas.gameObject.SetActive(true);
        cameraUICanvas.gameObject.SetActive(false);
    }

    private void ActivateTargetScript()
    {
        if (Hoření != null)
        {
            cameraTimer = 0f;
            Hoření.enabled = true;
            isScriptActivated = true;
        }
    }

    private void TryResetFoxys()
    {
        foreach (Laboři foxy in foxyRobots)
        {
            if (foxy != null && IsFoxyOnWatchedWaypoint(foxy) && !foxy.IsAtFirstWaypoint())
            {
                foxy.ResetToNextAction();
                Debug.Log(foxy.name + " byl resetován na první waypoint.");
            }
        }
        ActivateTargetScript();
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
}
