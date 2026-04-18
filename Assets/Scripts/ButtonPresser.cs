using UnityEngine;

public enum ButtonPresserType {
    Player,
    Buddy,
    Swarm,
    Object
}

public class ButtonPresser : MonoBehaviour {

    public int weightValue = 1;
    public ButtonPresserType buttonPresserType;

}
