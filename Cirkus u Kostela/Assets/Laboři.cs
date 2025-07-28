using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laboři : MonoBehaviour
{
    public List<Transform> waypoints; // Cesta Foxyho
    private int currentWaypointIndex = 0;

    public float moveDelay = 10f; // čas mezi pohyby
    private float moveTimer = 0f;

    private bool isResetting = false;

    void Update()
    {
        if (isResetting) return;

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveDelay)
        {
            MoveToNextWaypoint();
            moveTimer = 0f;
        }
    }

    private void MoveToNextWaypoint()
    {
        if (currentWaypointIndex < waypoints.Count - 1)
        {
            currentWaypointIndex++;
            transform.position = waypoints[currentWaypointIndex].position;
            Debug.Log($"{name} se přesunul na waypoint {currentWaypointIndex}");
        }
        else
        {
            Debug.Log($"{name} dosáhl konce cesty (např. office).");
            // Zde můžeš zavolat jumpscare, nebo jiný skript
        }
    }

    public bool IsAtFirstWaypoint()
    {
        return currentWaypointIndex == 0;
    }

    public void ResetToNextAction()
    {
        // Přesun zpět na první waypoint
        currentWaypointIndex = 0;
        transform.position = waypoints[0].position;
        moveTimer = 0f;
        isResetting = false;

        Debug.Log($"{name} byl resetován na začátek.");
    }
    public Transform GetCurrentWaypoint()
    {
        if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count)
            return waypoints[currentWaypointIndex];

        return null;
    }

}
