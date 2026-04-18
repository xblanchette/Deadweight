using UnityEngine;

public class Wire : MonoBehaviour
{

    public MeshRenderer partToRecolour;

    [ColorUsage(true, true)]
    public Color activatedColor;
    [ColorUsage(true, true)]
    public Color deactivatedColour;

    public int materialIndexToRecolour = 0;

    private void Start() {
        activatedColor.a = 1;
        deactivatedColour.a = 1;
        Deactivate();
    }

    public void Activate() {
        partToRecolour.materials[materialIndexToRecolour].color = activatedColor;
    }

    public void Deactivate() {
        partToRecolour.materials[materialIndexToRecolour].color = deactivatedColour;
    }

}
