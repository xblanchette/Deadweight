using UnityEngine;

public class FlickerLight : MonoBehaviour {
    public Light lightSource;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 0.05f;

    private float timer;

    void Update() {
        timer += Time.deltaTime;
        if (timer >= flickerSpeed) {
            // Set a random intensity within the range
            lightSource.intensity = Random.Range(minIntensity, maxIntensity);
            timer = 0;
        }
    }
}