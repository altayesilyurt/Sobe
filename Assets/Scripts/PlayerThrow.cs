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
                
                // (İsteğe bağlı) Çizgiyi fırlatma sırasında gizlemek istersen şu satırı açabilirsin:
                // trajectoryPredictor.SetTrajectoryVisible(false); 
            }
        }
    }

    private void UpdateTargetPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
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
        Rigidbody rb = newBall.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            // FIRLATMA YÖNÜ: Kilitlenen Sabit Hedef - GERÇEK El Pozisyonu
            Vector3 forceDirection = (targetPoint - throwSpawnPoint.position).normalized;
            forceDirection = (forceDirection + (Vector3.up * upwardArc)).normalized;
            
            rb.AddForce(forceDirection * throwForce, ForceMode.Impulse);
        }
        
        Destroy(newBall, 10f);

        // Fırlatma bitti, tekrar nişan almaya dönebiliriz
        isThrowing = false;
    }
}
