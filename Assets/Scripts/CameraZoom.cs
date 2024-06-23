using System.Collections;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float zoomDuration = 2.0f; // Duration of the zoom animation
    public float targetSize = 5.0f;   // Target orthographic size of the camera
    public float delayTime = 3.0f;    // Delay before zooming starts

    private Camera mainCamera;
    private float initialSize;
    private float startTime;

    void Start()
    {
        mainCamera = Camera.main;
        initialSize = mainCamera.orthographicSize;
        startTime = Time.time;
        StartCoroutine(ZoomCoroutine());
    }

    IEnumerator ZoomCoroutine()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delayTime);

        float elapsedTime = 0f;
        while (elapsedTime < zoomDuration)
        {
            // Calculate the progress of the zoom animation
            float t = elapsedTime / zoomDuration;

            // Smoothly interpolate between the initial size and the target size
            mainCamera.orthographicSize = Mathf.Lerp(initialSize, targetSize, t);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null; // Wait for the end of the frame
        }

        // Ensure the camera ends exactly at the target size
        mainCamera.orthographicSize = targetSize;
    }
}
