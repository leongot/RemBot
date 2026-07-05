using UnityEngine;

public class GridBordersGenerator : MonoBehaviour
{
    [Header("Dimensioni Griglia")]
    public int righe = 5;
    public int colonne = 5;

    [Header("Dimensione Casella")]
    public float dimensioneCella = 12f;

    [Header("Aspetto Linee")]
    public float spessoreLinea = 0.12f;
    public float altezzaLinea = 0.05f;
    public Material materialeLinea;

    [Header("Opzioni")]
    public bool centraGriglia = true;

    [ContextMenu("Genera Confini Griglia")]
    public void GeneraConfini()
    {
        CancellaConfini();

        float larghezzaTotale = colonne * dimensioneCella;
        float profonditaTotale = righe * dimensioneCella;

        float offsetX = 6f;
        float offsetZ = 6f;

        if (centraGriglia)
        {
            offsetX = -larghezzaTotale * 0.5f;
            offsetZ = -profonditaTotale * 0.5f;
        }

        // Linee verticali, parallele all'asse Z
        for (int x = 0; x <= colonne; x++)
        {
            float posX = offsetX + x * dimensioneCella;

            CreaLinea(
                "Grid_Line_Z_" + x,
                new Vector3(posX, altezzaLinea, offsetZ + profonditaTotale * 0.5f),
                new Vector3(spessoreLinea, spessoreLinea, profonditaTotale)
            );
        }

        // Linee orizzontali, parallele all'asse X
        for (int z = 0; z <= righe; z++)
        {
            float posZ = offsetZ + z * dimensioneCella;

            CreaLinea(
                "Grid_Line_X_" + z,
                new Vector3(offsetX + larghezzaTotale * 0.5f, altezzaLinea, posZ),
                new Vector3(larghezzaTotale, spessoreLinea, spessoreLinea)
            );
        }

        Debug.Log("[GridBordersGenerator] Confini griglia generati.");
    }

    [ContextMenu("Cancella Confini Griglia")]
    public void CancellaConfini()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (child.name.StartsWith("Grid_Line_"))
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }

    private void CreaLinea(string nome, Vector3 posizioneLocale, Vector3 scalaLocale)
    {
        GameObject linea = GameObject.CreatePrimitive(PrimitiveType.Cube);

        linea.name = nome;
        linea.transform.SetParent(transform);
        linea.transform.localPosition = posizioneLocale;
        linea.transform.localRotation = Quaternion.identity;
        linea.transform.localScale = scalaLocale;

        Collider col = linea.GetComponent<Collider>();

        if (col != null)
        {
            DestroyImmediate(col);
        }

        MeshRenderer renderer = linea.GetComponent<MeshRenderer>();

        if (renderer != null && materialeLinea != null)
        {
            renderer.sharedMaterial = materialeLinea;
        }
    }
}