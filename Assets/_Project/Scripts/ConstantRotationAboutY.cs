using UnityEngine;

public class ConstantRotationAboutY : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 180f;
    
    private void Update()
    {
        transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.World);
    }
}
