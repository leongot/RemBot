using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform focus;

    [SerializeField]
    Camera camera;

    [SerializeField, Range(20f, 100f)]
    float distance = 20f;

    [SerializeField, Range(0f, 1080f)]
    float rotationSpeed = 360f;

    Vector2 orbitAngles = new Vector2(40f, 0f);

    float prevDistance;

    float GetZoomFactor()
    {
        const float scale = 4f;

        float value = prevDistance;
        float input = Input.GetAxis("Mouse ScrollWheel");
        //Debug.Log(input);
        value -= input * scale;

        return Mathf.Clamp(value, 20f, 100f);
    }

    void Rotate()
    {
        Vector2 input = new Vector2(
            Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X")
        );  

        const float e = 0.001f;
        if (input.x < -e || input.x < e || input.y < e || input.y > e)
            orbitAngles += rotationSpeed * Time.deltaTime * input;
    }

    Vector3 GetMouseCursorPosition() => camera.ScreenToViewportPoint(Input.mousePosition);

    void Awake()
    {
        Quaternion lookRotation = Quaternion.Euler(orbitAngles);
        Vector3 lookPosition = focus.position - transform.forward * distance;

        transform.SetLocalPositionAndRotation(lookPosition, lookRotation);

        prevDistance = distance;
    }

    void LateUpdate()
    {
        if (Input.GetMouseButton(2))
            Rotate();

        Debug.Log(GetZoomFactor());

        float newDistance = GetZoomFactor();
        Quaternion lookRotation = Quaternion.Euler(orbitAngles);        
        Vector3 lookPosition = focus.position - transform.forward * newDistance; 

        transform.SetLocalPositionAndRotation(lookPosition, lookRotation);

        prevDistance = newDistance;
    }
}
