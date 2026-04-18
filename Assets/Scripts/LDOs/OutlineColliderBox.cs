using UnityEngine;

public class OutlineColliderBox : MonoBehaviour {

    public Color boxColour = Color.white;
    public bool drawGizmos = true;

    private void OnDrawGizmos() {
        if (!drawGizmos) {
            return;
        }
        if (!enabled) {
            return;
        }

        Gizmos.color = boxColour;
        var scale = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * transform.lossyScale;
        Gizmos.DrawWireCube(transform.position, scale);
    }

}
