using UnityEngine;
using UnityEngine.EventSystems;

public class SlotComandi : MonoBehaviour, IPointerClickHandler
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
    }
}