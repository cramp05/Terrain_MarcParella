using UnityEngine;

public class NigthDaySky : MonoBehaviour
{
    public float rotationSpeed = 10f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }
}
