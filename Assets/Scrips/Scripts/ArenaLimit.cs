using UnityEngine;

public class ArenaLimit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player =
            other.GetComponent<PlayerController>();

        if (player != null)
        {
            Debug.Log(other.gameObject.name + " salió de la arena.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerOut(player);
            }
            else
            {
                Debug.Log("ERROR: GameManager.Instance es null.");
            }
        }
    }
}