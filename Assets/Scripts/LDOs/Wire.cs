using UnityEngine;

public class Wire : MonoBehaviour
{

    public MeshRenderer partToRecolour;

    public Color activatedColor;
    public Color deactivatedColour;

    private void Start() {
        activatedColor.a = 1;
        deactivatedColour.a = 1;
        Deactivate();
    }

    public void Activate() {
        partToRecolour.material.color = activatedColor;
    }

    public void Deactivate() {
        partToRecolour.material.color = deactivatedColour;
    }

}
