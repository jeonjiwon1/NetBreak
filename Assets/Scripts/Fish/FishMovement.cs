using UnityEngine;

public class FishMovement : MonoBehaviour
{
    [SerializeField] private FishData fishData;
    [SerializeField] private float exitMargin = 0.5f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (fishData == null)
        {
            return;
        }

        transform.position +=
            Vector3.right * fishData.MoveSpeed * Time.deltaTime;

        float cameraRight =
            mainCamera.transform.position.x
            + mainCamera.orthographicSize * mainCamera.aspect;

        if (transform.position.x >= cameraRight + exitMargin)
        {
            gameObject.SetActive(false);
        }
    }
}