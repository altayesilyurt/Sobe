using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPredictor : MonoBehaviour
{
    LineRenderer trajectoryLine;
    public int maxPoints = 50;
    public float increment = 0.025f;
    public float rayOverlap = 1.1f;
    public Transform hitMarker;

    private void Start()
    {
        trajectoryLine = GetComponent<LineRenderer>();
    }

    public void PredictTrajectory(ProjectileProperties projectile)
    {
        Vector3 velocity = projectile.direction * (projectile.initialSpeed / projectile.mass);
        Vector3 position = projectile.initialPosition;
        Vector3 nextPosition;
        
        trajectoryLine.positionCount = maxPoints;
        trajectoryLine.SetPosition(0, position);

        for (int i = 1; i < maxPoints; i++)
        {
            // linearDamping kullanarak yerçekimi ve hava sürtünmesi simülasyonu
            velocity += Physics.gravity * increment;
            velocity *= Mathf.Clamp01(1f - projectile.linearDamping * increment);
            nextPosition = position + velocity * increment;

            float overlap = Vector3.Distance(position, nextPosition) * rayOverlap;

            if (Physics.Raycast(position, velocity.normalized, out RaycastHit hit, overlap))
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, hit.point);
                
                if (hitMarker != null)
                {
                    hitMarker.gameObject.SetActive(true);
                    hitMarker.position = hit.point + hit.normal * 0.025f;
                    hitMarker.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                }
                break;
            }

            if (hitMarker != null) hitMarker.gameObject.SetActive(false);
            position = nextPosition;
            trajectoryLine.SetPosition(i, position);
        }
    }

    public void SetTrajectoryVisible(bool visible)
    {
        trajectoryLine.enabled = visible;
        if (hitMarker != null) hitMarker.gameObject.SetActive(visible);
    }
}
