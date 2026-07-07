using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private bool goalRaggiunto = false;

    private void OnTriggerEnter(Collider other)
    {
        if (goalRaggiunto)
            return;

        bool isPlayer =
            other.CompareTag("Player") ||
            other.transform.root.CompareTag("Player");

        if (!isPlayer)
            return;

        goalRaggiunto = true;

        if (GestoreBlocchi.Instance != null)
            GestoreBlocchi.Instance.CompletaLivelloDaGoal();

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.MostraLevelComplete();

        Debug.Log("[GoalTrigger] Player arrivato al goal.");
    }
}