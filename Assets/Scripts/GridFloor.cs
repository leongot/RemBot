using UnityEngine;

public class GridFloor : MonoBehaviour
{
    [Header("Prefab Mattonella")]
    public GameObject tilePrefab;

    [Header("Dimensioni Griglia")]
    public int righe = 5;
    public int colonne = 5;

    [Header("Dimensione Casella")]
    public float dimensioneCella = 12f;

    [Header("Opzioni")]
    public bool centraGriglia = true;

    [ContextMenu("Genera Griglia")]
    public void GeneraGriglia()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("[GridFloor] Tile prefab non assegnato.");
            return;
        }

        CancellaGriglia();

        for (int x = 0; x < colonne; x++)
        {
            for (int z = 0; z < righe; z++)
            {
                Vector3 posizione = new Vector3(
                    x * dimensioneCella,
                    0f,
                    z * dimensioneCella
                );

                if (centraGriglia)
                {
                    posizione.x -= (colonne - 1) * dimensioneCella * 0.5f;
                    posizione.z -= (righe - 1) * dimensioneCella * 0.5f;
                }

                GameObject tile = Instantiate(tilePrefab, transform);
                tile.name = "Tile_" + x + "_" + z;
                tile.transform.localPosition = posizione;
                tile.transform.localRotation = Quaternion.identity;
            }
        }

        Debug.Log("[GridFloor] Griglia generata.");
    }

    [ContextMenu("Cancella Griglia")]
    public void CancellaGriglia()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}