using System.Collections.Generic;
using UnityEngine;

public class ActivateDoorsTrigger : MonoBehaviour
{

    public List<GameObject> objectsToActivate = new();

    private bool hasActivated = false;

    private void OnTriggerEnter(Collider other) {
        if (hasActivated) {
            return;
        }
        hasActivated = true;

        var player = other.gameObject.GetComponent<PlayerController>();
        if (player == null) {
            return;
        }

        foreach (var obj in objectsToActivate) {
            var comp = obj.GetComponentInChildren<ActivateByPressurePlate>();

            if (comp != null) {
                comp.ActivateObject();
            }
        }
    }

}
