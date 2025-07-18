using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // �Ǐ]�ΏہiPlayer�j
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10); // �J�����̑��Έʒu
    [SerializeField] private float smoothSpeed = 0.125f; // �Ǐ]�̂Ȃ߂炩��

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
