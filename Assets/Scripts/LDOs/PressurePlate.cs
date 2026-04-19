using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PressurePlate : MonoBehaviour {

    public AudioSource audioSource;

    public List<ButtonPresserType> thingsThatCanActivateButton = new();
    public int numberOfObjectsRequired = 1;

    public Color notActivatedColour;
    public Color pressedColour;

    public bool displayTextOnButton = true;
    public float pressedButtonYPos = -0.1f;
    public float buttonMoveLerpSpeed = 5;
    public int materialIndex = 1;

    [Header("Object references")]
    public GameObject fullCollider;
    public GameObject smallerCollider;
    public GameObject partThatMoves;
    public TextMeshProUGUI textOnButton;
    public MeshRenderer partToRecolour;
    public GameObject checkmark;
    public List<GameObject> objectsToActivateWithButton = new();

    [HideInInspector]
    public bool isPressed = false;

    // This is determined by the object this button activates
    [HideInInspector]
    public bool isPermanentPress = true;

    private bool allPermanentButtonsHaveBeenPressedAtTheSameTime = false;

    private List<GameObject> objectsPressingOnButton = new();
    private float targetYForPartThatMoves = 0;
    private List<Wire> wiresToActivate = new();
    private List<ActivateByPressurePlate> activatedByPressurePlate = new();
    private List<PressurePlate> otherButtonsThatActivateTheSameObjects = new();

    private void Start() {
        notActivatedColour.a = 1;
        pressedColour.a = 1;

        partToRecolour.materials[materialIndex].color = notActivatedColour;
        fullCollider.SetActive(false);
        smallerCollider.SetActive(true);
        checkmark.SetActive(false);
        Invoke(nameof(FindOtherButtonsThatActivateTheSameObjects), 0);

        if (!objectsToActivateWithButton.Any()) {
            Debug.LogError($"Button does not activate anything");
        }
        else {
            foreach (var obj in objectsToActivateWithButton) {
                var comps = obj.GetComponentsInChildren<ActivateByPressurePlate>();
                foreach (var comp in comps) {
                    activatedByPressurePlate.Add(comp);
                }
            }

            if (!activatedByPressurePlate.Any()) {
                Debug.LogError($"Button does not activate anything");
            }
            else {
                isPermanentPress = activatedByPressurePlate.First().isPermanentlyActivated;
            }
        }

        foreach (var thingToActivate in activatedByPressurePlate) {
            if (thingToActivate.isPermanentlyActivated != isPermanentPress) {
                Debug.LogError($"All things activated by this button must have the same isPermanentlyActivated value");
                return;
            }

            thingToActivate.AddPressurePlateThatActivatesThis(this);
        }

        var wires = GetComponentsInChildren<Wire>();
        wiresToActivate = wires.ToList();

        if (!wiresToActivate.Any()) {
            Debug.LogError($"A button does not have any wires. Add wires as children of the button");
        }
    }

    private void FindOtherButtonsThatActivateTheSameObjects() {
        foreach (var obj in activatedByPressurePlate) {
            otherButtonsThatActivateTheSameObjects.AddRange(obj.pressurePlatesThatActivateThis);
        }

        otherButtonsThatActivateTheSameObjects = otherButtonsThatActivateTheSameObjects.Distinct().ToList();
    }

    private void OnTriggerEnter(Collider other) {
        var objectEntered = other.gameObject;
        if (objectEntered == null) {
            return;
        }

        var buttonPresser = objectEntered.GetComponentInParent<ButtonPresser>();
        if (buttonPresser == null) {
            return;
        }
        objectEntered = buttonPresser.gameObject;

        if (objectsPressingOnButton.Contains(objectEntered)) {
            return;
        }

        foreach (var activator in thingsThatCanActivateButton) {
            if (activator == buttonPresser.buttonPresserType) {
                objectsPressingOnButton.Add(objectEntered);
                break;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        var objectEntered = other.gameObject;
        if (objectEntered == null) {
            return;
        }

        var buttonPresser = objectEntered.GetComponentInParent<ButtonPresser>();
        if (buttonPresser == null) {
            return;
        }
        objectEntered = buttonPresser.gameObject;

        if (objectsPressingOnButton.Contains(objectEntered)) {
            objectsPressingOnButton.Remove(objectEntered);
        }
    }

    private void FixedUpdate() {
        var currentPressCount = objectsPressingOnButton.Count;

        if (displayTextOnButton && textOnButton != null) {
            textOnButton.text = $"{currentPressCount}/{numberOfObjectsRequired}";
        }

        if (currentPressCount >= numberOfObjectsRequired && !isPressed) {
            PressButton();
        }

        if (currentPressCount < numberOfObjectsRequired && isPressed) {
            UnPressButton();
        }

        if (isPressed && isPermanentPress && !allPermanentButtonsHaveBeenPressedAtTheSameTime) {
            allPermanentButtonsHaveBeenPressedAtTheSameTime = otherButtonsThatActivateTheSameObjects.All(x => x.isPressed);
            if (allPermanentButtonsHaveBeenPressedAtTheSameTime) {
                PressButton();
            }
        }
    }

    private void PressButton() {
        isPressed = true;
        SoundManager.instance.PlaySound(audioSource);
        targetYForPartThatMoves = pressedButtonYPos;
        fullCollider.SetActive(false);
        smallerCollider.SetActive(true);

        partToRecolour.materials[materialIndex].color = pressedColour;

        if (isPermanentPress && allPermanentButtonsHaveBeenPressedAtTheSameTime) {
            if (textOnButton != null) {
                textOnButton.gameObject.SetActive(false);
            }

            checkmark.SetActive(true);
        }

        foreach (var wire in wiresToActivate) {
            wire.Activate();
        }
    }

    private void UnPressButton() {
        if (isPermanentPress && allPermanentButtonsHaveBeenPressedAtTheSameTime) {
            return;
        }

        isPressed = false;
        targetYForPartThatMoves = 0;
        fullCollider.SetActive(true);
        smallerCollider.SetActive(false);
        partToRecolour.materials[materialIndex].color = notActivatedColour;

        if (isPermanentPress) {
            if (textOnButton != null) {
                textOnButton.gameObject.SetActive(true);
            }

            checkmark.SetActive(false);
        }

        foreach (var wire in wiresToActivate) {
            wire.Deactivate();
        }
    }

    private void Update() {
        UpdateHeightOfPartThatMoves();
    }

    private void UpdateHeightOfPartThatMoves() {
        if (partThatMoves.transform.localPosition.y != targetYForPartThatMoves) {
            var newPos = partThatMoves.transform.localPosition;
            newPos.y = Mathf.Lerp(newPos.y, targetYForPartThatMoves, buttonMoveLerpSpeed * Time.deltaTime);
            partThatMoves.transform.localPosition = newPos;
        }
    }

}
