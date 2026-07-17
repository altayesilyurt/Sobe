using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Hedef")]
    [Tooltip("Kameranın takip edeceği oyuncu (Player) objesi")]
    public Transform target;

    [Header("Top-Down Takip Ayarları")]
    [Tooltip("Kameranın oyuncuya olan uzaklığı ve yüksekliği (Kuşbakışı için tam tepeye aldık)")]
    public Vector3 offset = new Vector3(0f, 15f, 0f);
    
    [Tooltip("Kameranın hedefe ne kadar yumuşak gideceği (Düşük değer = daha yumuşak)")]
    public float smoothTime = 0.125f;

    [Tooltip("Oyun başlarken kameranın yönü otomatik olarak tam aşağı (90 derece) baksın mı?")]
    public bool lockRotationToTopDown = true;

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Kuşbakışı görünüm için kameranın X eksenini 90 derece yaparak tam aşağı bakmasını sağlıyoruz
        if (lockRotationToTopDown)
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Kamera için bir hedef (Target) belirlenmedi! Lütfen Inspector'dan oyuncuyu sürükleyin.");
            return;
        }

        // Kameranın gitmesi gereken ideal pozisyon
        Vector3 desiredPosition = target.position + offset;

        // SmoothDamp kullanarak kamerayı pürüzsüz bir şekilde hareket ettir
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        
        // Not: Tam tepeden bakarken (0, Y, 0) LookAt kullanmak kamera yönünün sapmasına sebep olur.
        // Bu yüzden 2D/Top-Down oyunlarda sadece pozisyon takip edilir, dönüş (rotation) sabit bırakılır.
    }
}
