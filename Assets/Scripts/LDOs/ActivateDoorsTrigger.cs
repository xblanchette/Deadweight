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

        var player = other.gameObject.GetComponent<PlayerController>();
        if (player == null) {
            return;
        }

        hasActivated = true;

        foreach (var obj in objectsToActivate) {
            var comps = obj.GetComponentsInChildren<ActivateByPressurePlate>();

            foreach (var comp in comps) {
                comp.ActivateObject();
            }
        }
    }

}
