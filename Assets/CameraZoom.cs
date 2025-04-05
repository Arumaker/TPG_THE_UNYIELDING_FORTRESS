using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            float newSize = mainCamera.orthographicSize - scrollInput * zoomSpeed;
            newSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            mainCamera.orthographicSize = newSize;
        }

        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals))
        {
            float newSize = mainCamera.orthographicSize + -1f * zoomSpeed * Time.deltaTime;
            newSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            mainCamera.orthographicSize = newSize;
        }
        if (Input.GetKey(KeyCode.Minus))
        {
            float newSize = mainCamera.orthographicSize + 1f * zoomSpeed * Time.deltaTime;
            newSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            mainCamera.orthographicSize = newSize;
        }
    }

}