using UnityEngine;

[RequireComponent(typeof(TrajectoryPredictor))]
public class PlayerThrow : MonoBehaviour
{
    [Header("Fırlatma Ayarları")]
    public GameObject ballPrefab;       
    
    [Tooltip("Animasyon sırasında topun çıkacağı GERÇEK hareketli El objesi")]
    public Transform throwSpawnPoint;   
    
    [Tooltip("Nişan alırken çizginin başlayacağı, topun elden çıkacağı ana denk gelen SABİT nokta")]
    public Transform predictedReleasePoint;

    public float throwForce = 15f;      
    public float upwardArc = 0.5f; 
    
    [Header("Atış Sınırları")]
    [Tooltip("Karakterin önüne göre maksimum sağ/sol fırlatma açısı")]
    public float maxThrowAngle = 45f;

    private Vector3 targetPoint; 
    private Animator animator;
    private TrajectoryPredictor trajectoryPredictor;
    private Rigidbody ballRigidbody;
    
    // Animasyon oynarken çizgiyi dondurmak ve üst üste atışı engellemek için
    private bool isThrowing = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        trajectoryPredictor = GetComponent<TrajectoryPredictor>();
        
        if (ballPrefab != null)
        {
            ballRigidbody = ballPrefab.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        // Karakter fırlatma animasyonu oynatmıyorsa nişan alabilir
        if (!isThrowing)
        {
            UpdateTargetPoint();
            PredictTrajectory();

            // Sol tıka basıldığında
            if (Input.GetMouseButtonDown(0))
            {
                // 1. Durumu fırlatmaya geçir, fare tıklamasını engelle
                isThrowing = true;
                
                // 2. Animasyonu başlat
                animator.SetTrigger("Throw");
                
                // Çizgiyi ve Hit Marker'ı fırlatma sırasında gizle
                trajectoryPredictor.SetTrajectoryVisible(false); 
            }
        }
    }

    private void UpdateTargetPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 hitPoint = hit.point;
            
            // Karakterden hedefe olan yatay yönü bul
            Vector3 directionToHitFlat = new Vector3(hitPoint.x - transform.position.x, 0f, hitPoint.z - transform.position.z);
            Vector3 forwardFlat = new Vector3(transform.forward.x, 0f, transform.forward.z);

            if (directionToHitFlat.sqrMagnitude > 0.001f && forwardFlat.sqrMagnitude > 0.001f)
            {
                // Karakterin baktığı yön ile hedefin yönü arasındaki açıyı bul
                float angle = Vector3.SignedAngle(forwardFlat, directionToHitFlat, Vector3.up);

                // Eğer açı sınırların dışındaysa
                if (Mathf.Abs(angle) > maxThrowAngle)
                {
                    // Açıyı maxThrowAngle ile sınırla (örneğin -45 ile +45 arası)
                    float clampedAngle = Mathf.Clamp(angle, -maxThrowAngle, maxThrowAngle);
                    
                    // İleri vektörünü sınırlandırılmış açı kadar döndür
                    Quaternion rotation = Quaternion.AngleAxis(clampedAngle, Vector3.up);
                    Vector3 clampedDirection = rotation * forwardFlat;

                    // Orijinal yatay uzaklığı koru
                    float distance = directionToHitFlat.magnitude;
                    targetPoint = transform.position + clampedDirection.normalized * distance;
                    targetPoint.y = hitPoint.y; // Orijinal yüksekliği koru
                }
                else
                {
                    targetPoint = hitPoint;
                }
            }
            else
            {
                targetPoint = hitPoint;
            }
        }
    }

    private void PredictTrajectory()
    {
        if (ballRigidbody == null || predictedReleasePoint == null) return;

        ProjectileProperties properties = new ProjectileProperties();
        
        // ÇİZGİ YÖNÜ: Hedef Nokta - TAHMİNİ Sabit Çıkış Noktası
        Vector3 forceDirection = (targetPoint - predictedReleasePoint.position).normalized;
        forceDirection = (forceDirection + (Vector3.up * upwardArc)).normalized;

        properties.direction = forceDirection;
        properties.initialPosition = predictedReleasePoint.position;
        properties.initialSpeed = throwForce;
        properties.mass = ballRigidbody.mass;
        properties.linearDamping = ballRigidbody.linearDamping; 

        trajectoryPredictor.SetTrajectoryVisible(true);
        trajectoryPredictor.PredictTrajectory(properties);
    }

    // --- ANİMASYON EVENTİ ---
    public void FirlatmayiTetikle()
    {
        // Topu GERÇEK hareketli el pozisyonundan yarat
        GameObject newBall = Instantiate(ballPrefab, throwSpawnPoint.position, throwSpawnPoint.rotation);
        
        // Çarpma anında topun yok olması için yazdığımız componenti ekle
        newBall.AddComponent<BallCollision>();

        // ÖNEMLİ: Çizilen tahmin çizgisinin (Raycast) havada uçan topa çarpıp HitMarker'ı 
        // topun üzerine koymasını engellemek için topu "Ignore Raycast" katmanına (Layer 2) alıyoruz.
        newBall.layer = 2;

        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            // FIRLATMA YÖNÜ: Kilitlenen Sabit Hedef - GERÇEK El Pozisyonu
            Vector3 forceDirection = (targetPoint - throwSpawnPoint.position).normalized;
            forceDirection = (forceDirection + (Vector3.up * upwardArc)).normalized;
            
            rb.AddForce(forceDirection * throwForce, ForceMode.Impulse);
        }
        
        // Fırlatma bitti, tekrar nişan almaya dönebiliriz
        isThrowing = false;
    }
}
