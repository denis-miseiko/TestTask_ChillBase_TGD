using UnityEngine;

public class MarkerBehavior : MonoBehaviour
{
    public float bobAmplitude = 0.3f;
    public float bobSpeed = 2f;
    public float rotateSpeed = 60f;

    private Vector3 startPos;

    private void Start() => startPos = transform.localPosition;

    private void Update()
    {
        transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
}