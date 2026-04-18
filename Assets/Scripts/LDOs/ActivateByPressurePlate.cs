using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActivateByPressurePlate : MonoBehaviour {

    public AudioSource audioSource;
    public bool isPermanentlyActivated = true;

    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 20;

    [HideInInspector]
    public List<PressurePlate> pressurePlatesThatActivateThis = new();

    private Vector3 currentTargetPosition;
    private Vector3 previousTargetPosition;

    private float t = 0;
    private bool manuallyStarted = false;

    private void Start() {
        currentTargetPosition = pointA.position;

        var distanceBetweenPoints = Vector3.Distance(pointA.position, pointB.position);

        // The speed should not scale based on the travel distance
        moveSpeed /= distanceBetweenPoints;
    }

    public void AddPressurePlateThatActivatesThis(PressurePlate plate) {
        pressurePlatesThatActivateThis.Add(plate);
    }

    public void ActivateObject() {
        manuallyStarted = true;
    }

    private void FixedUpdate() {
        if ((!pressurePlatesThatActivateThis.Any()) && (!manuallyStarted)) {
            return;
        }

        var allButtonsPressed = pressurePlatesThatActivateThis.All(x => x.isPressed);

        if (manuallyStarted) {
            allButtonsPressed = true;
        }

        currentTargetPosition = allButtonsPressed ? pointB.position : pointA.position;

        if (currentTargetPosition != previousTargetPosition) {
            SoundManager.instance.PlaySound(audioSource);
            t = 0;
        }

        t += Time.fixedDeltaTime * moveSpeed;
        transform.position = Vector3.Lerp(transform.position, currentTargetPosition, t);

        t = Mathf.Clamp01(t);

        previousTargetPosition = currentTargetPosition;
    }

    private void OnDrawGizmos() {
        if (!enabled) {
            return;
        }

        if (pointA == null || pointB == null) {
            return;
        }

        var radius = 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pointA.position, radius);
        Gizmos.DrawWireSphere(pointB.position, radius);

        Gizmos.DrawLine(pointA.position, pointB.position);
    }

}
