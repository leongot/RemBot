using UnityEngine;
using UnityEngine.EventSystems;

public class SlotAreaPrincipale : MonoBehaviour, IPointerClickHandler
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

        GestoreBlocchi.Instance.ImpostaSlotComandi(this.transform);
        Debug.Log("Tornato all'area principale.");
    }
}
