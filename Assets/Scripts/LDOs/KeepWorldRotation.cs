using UnityEngine;

public class KeepWorldRotation : MonoBehaviour
{

    private void Update() {
        transform.rotation = Quaternion.identity;
    }

}
