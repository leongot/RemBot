using System.Collections;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    Player player;

    [SerializeField]
    Transform spawnPoint;

    [SerializeField]
    TextMeshPro textUI;

    void Awake()
    {
        player.StartNewGame(spawnPoint.localPosition, spawnPoint.rotation);    
    }
}
