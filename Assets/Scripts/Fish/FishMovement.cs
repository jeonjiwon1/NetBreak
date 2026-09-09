using UnityEngine;

[RequireComponent(typeof(FishController))]
public class FishMovement : MonoBehaviour
{
    [SerializeField] private float exitMargin = 0.5f;

    private Camera mainCamera;
    private FishController fishController;

    private void Awake()
    {
        mainCamera = Camera.main;
        fishController = GetComponent<FishController>();
    }

    private void Update()
    {
        if (fishController.Data == null)
        {
            return;
        }

        transform.position +=
            Vector3.right *
            fishController.Data.MoveSpeed *
            Time.deltaTime;

        float cameraRight =
            mainCamera.transform.position.x
            + mainCamera.orthographicSize * mainCamera.aspect;

        if (transform.position.x >= cameraRight + exitMargin)
        {
            gameObject.SetActive(false);
        }
    }
}