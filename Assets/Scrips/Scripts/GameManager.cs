using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    private Vector3 player1Start;
    private Vector3 player2Start;

    private int player1Score;
    private int player2Score;

    private void Awake()
    {
        Instance = this;

        player1Start = player1.position;
        player2Start = player2.position;
    }

    public void PlayerOut(PlayerController player)
    {
        if (player.IsPlayer2())
        {
            player1Score++;
        }
        else
        {
            player2Score++;
        }

        Debug.Log(
            "MARCADOR | PLAYER 1: " +
            player1Score +
            " - PLAYER 2: " +
            player2Score
        );

        player1.position = player1Start;
        player2.position = player2Start;
    }
}