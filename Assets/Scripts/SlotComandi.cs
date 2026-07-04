using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Metti questo script sulla "pancia bianca" interna agli IF/WHILE.
/// Cliccandoci, quella diventa l'area attiva per i comandi.
/// </summary>
public class SlotComandi : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        GestoreBlocchi.Instance.ImpostaSlotComandi(this.transform);
    }
}
