using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Metti questo script sulla casella gialla di IF/WHILE.
/// Blocca la propagazione verso la pancia (SlotComandi padre)
/// che altrimenti azzererebbe slotAttivoCondizione.
/// </summary>
public class SlotCondizione : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        GestoreBlocchi.Instance.ImpostaSlotCondizione(this.transform);
        PannelloCondizioni.Instance.Mostra();

        // CRITICO: blocca la propagazione verso il padre (SlotComandi)
        // che chiamerebbe ImpostaSlotComandi e azzererebbe slotAttivoCondizione
        eventData.Use();

        Debug.Log("[SlotCondizione] Condizione impostata, propagazione bloccata.");
    }
}
