using UnityEngine;

public class BallCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Eğer top fırlatıldığı anda karakterin kendisine (Player) çarparsa silinmesini engelle
        if (collision.gameObject.CompareTag("Player")) 
        {
            return;
        }

        // Top herhangi başka bir objeye çarptığında kendini yok et
        Destroy(gameObject);
    }
}
