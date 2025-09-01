using UnityEngine;

public class FlyingEnemy : BaseEnemy
{
    [Header("����|�C���g�i���ԂɈړ��j")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    private int currentWaypoint = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Transform target = waypoints[currentWaypoint];
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction;
        }
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
}
