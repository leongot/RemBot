using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Metti questo script su Contenuto_Codice.
/// Cliccando sull'area vuota si torna a inserire nell'area principale.
/// 
/// IMPORTANTE: aggiungi anche una Image su Contenuto_Codice con:
///   - Color: qualsiasi, Alpha anche 0 (trasparente)
///   - Raycast Target: TRUE  ← obbligatorio, altrimenti i click passano nel vuoto
/// </summary>
public class SlotAreaPrincipale : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        GestoreBlocchi.Instance.ImpostaSlotComandi(this.transform);
        Debug.Log("Tornato all'area principale.");
    }
}
