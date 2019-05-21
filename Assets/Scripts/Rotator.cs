using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float x;
    public float y;
    public float z;

    void Update()
    {
        transform.Rotate(x * Time.deltaTime, y * Time.deltaTime, z * Time.deltaTime);
    }
}
