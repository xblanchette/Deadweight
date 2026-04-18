using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PressurePlate : MonoBehaviour {

    public bool isPermanentPress = false;

    public List<ButtonPresserType> thingsThatCanActivateButton = new();
    public int numberOfObjectsRequired = 1;

    public Color notActivatedColour;
    public Color pressedColour;

    public bool displayTextOnButton = true;
    public float pressedButtonYPos = -0.1f;
    public float buttonMoveLerpSpeed = 5;

    [Header("Object references")]
    public GameObject fullCollider;
    public GameObject smallerCollider;
    public GameObject partThatMoves;
    public TextMeshProUGUI textOnButton;
    public MeshRenderer partToRecolour;

    private List<GameObject> objectsPressingOnButton = new();
    private bool isPressed = false;
    private float targetYForPartThatMoves = 0;

    private void Start() {
        notActivatedColour.a = 1;
        pressedColour.a = 1;

        partToRecolour.material.color = notActivatedColour;
        fullCollider.SetActive(false);
        smallerCollider.SetActive(true);
    }

    private void OnTriggerEnter(Collider other) {
        var objectEntered = other.gameObject;
        if (objectEntered == null) {
            return;
        }

        var buttonPresser = objectEntered.GetComponent<ButtonPresser>();
        if (buttonPresser == null) {
            return;
        }

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
        if (objectsPressingOnButton.Contains(other.gameObject)) {
            objectsPressingOnButton.Remove(other.gameObject);
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
    }

    private void PressButton() {
        isPressed = true;
        targetYForPartThatMoves = pressedButtonYPos;
        fullCollider.SetActive(false);
        smallerCollider.SetActive(true);

        partToRecolour.material.color = pressedColour;
    }

    private void UnPressButton() {
        if (isPermanentPress) {
            return;
        }

        isPressed = false;
        targetYForPartThatMoves = 0;
        fullCollider.SetActive(true);
        smallerCollider.SetActive(false);
        partToRecolour.material.color = notActivatedColour;
    }

    private void Update() {
        UpdateHeightOfPartThatMoves();
    }

    private void UpdateHeightOfPartThatMoves() {
        if (partThatMoves.transform.position.y != targetYForPartThatMoves) {
            var newPos = partThatMoves.transform.position;
            newPos.y = Mathf.Lerp(newPos.y, targetYForPartThatMoves, buttonMoveLerpSpeed * Time.deltaTime);
            partThatMoves.transform.position = newPos;
        }
    }

}
