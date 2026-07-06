using UnityEngine;
using UnityEngine.EventSystems;

public class SlotCondizione : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GestoreBlocchi.Instance == null)
            return;

        if (GestoreBlocchi.Instance.ProgrammaBloccato)
        {
            eventData.Use();
            return;
        }

        GestoreBlocchi.Instance.ImpostaSlotCondizione(this.transform);

        if (PannelloCondizioni.Instance != null)
            PannelloCondizioni.Instance.Mostra();

        eventData.Use();

        Debug.Log("[SlotCondizione] Condizione impostata, propagazione bloccata.");
    }
}
