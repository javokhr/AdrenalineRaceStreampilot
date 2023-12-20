using UnityEngine;

public class CarAnimation : MonoBehaviour
{
    [SerializeField] private Vector3 finalPosition;
    private Vector3 initialPosition;

    private void Awake()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, finalPosition, 0.1f);
        transform.RotateAround(gameObject.transform.position, Vector3.up, 20 * Time.deltaTime);
    }

    private void OnDisable()
    {
        transform.position = initialPosition;
    }
}
