using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPredictor : MonoBehaviour
{
    LineRenderer trajectoryLine;
    public int maxPoints = 50;
    public float increment = 0.025f;
    public float rayOverlap = 1.1f;
    public Transform hitMarker;

    [Header("Çizgi Görünümü")]
    public float lineWidth = 0.05f; // Çizgi kalınlığı
    public float dotSpacing = 2f;   // Nokta sıklığı (Tile modu için)

    private void Start()
    {
        trajectoryLine = GetComponent<LineRenderer>();

        // 1. Çizgiyi incelt
        trajectoryLine.startWidth = lineWidth;
        trajectoryLine.endWidth = lineWidth;

        // 2. Noktalı görünüm için texture'ın çizgi boyunca tekrar etmesini sağla
        trajectoryLine.textureMode = LineTextureMode.Tile;
        
        // 3. Kod üzerinden noktalı bir Material oluştur
        CreateDottedMaterial();
    }

    private void CreateDottedMaterial()
    {
        Texture2D dotTexture = new Texture2D(64, 64);
        dotTexture.wrapMode = TextureWrapMode.Repeat;
        dotTexture.filterMode = FilterMode.Bilinear;
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                // Daire şekli oluştur
                float u = (x / 63f) - 0.5f;
                float v = (y / 63f) - 0.5f;
                float dist = Mathf.Sqrt(u * u + v * v);
                
                // Yarıçapı ufak tutuyoruz ki arasında boşluk kalsın (noktalı görünsün)
                Color color = (dist < 0.25f) ? Color.white : Color.clear;
                dotTexture.SetPixel(x, y, color);
            }
        }
        dotTexture.Apply();

        // Şeffaflığı destekleyen temel bir shader kullan (Sprites/Default genelde her projede sorunsuz çalışır)
        Material dottedMaterial = new Material(Shader.Find("Sprites/Default"));
        dottedMaterial.mainTexture = dotTexture;
        
        // Tekrar sıklığı
        dottedMaterial.mainTextureScale = new Vector2(dotSpacing, 1f); 
        
        trajectoryLine.material = dottedMaterial;
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
                    MoveHitMarker(hit);
                }
                break;
            }

            if (hitMarker != null) hitMarker.gameObject.SetActive(false);
            position = nextPosition;
            trajectoryLine.SetPosition(i, position);
        }
    }

    private void MoveHitMarker(RaycastHit hit)
    {
        hitMarker.gameObject.SetActive(true);

        // Yüzeyden çok hafif dışarıda tut (içine girmemesi için)
        float offset = 0.025f;
        hitMarker.position = hit.point + hit.normal * offset;
        
        // Quad veya Sprite (2D) kullanıldığında yüzeye tam yapışık (düz) durması için:
        // Yüzü (ön kısmı) yüzeyden dışarı bakacak şekilde normalin tersine çeviriyoruz.
        hitMarker.forward = -hit.normal;
    }

    public void SetTrajectoryVisible(bool visible)
    {
        trajectoryLine.enabled = visible;
        if (hitMarker != null) hitMarker.gameObject.SetActive(visible);
    }
}
