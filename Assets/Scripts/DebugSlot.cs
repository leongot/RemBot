using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Script TEMPORANEO di debug. Aggiungilo alla casella gialla
/// per capire se i click arrivano. Rimuovilo dopo aver fixato il problema.
/// </summary>
public class DebugSlot : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[DEBUG] Click ricevuto su: " + gameObject.name);
        Debug.Log("[DEBUG] GestoreBlocchi.Instance: " + (GestoreBlocchi.Instance != null ? "OK" : "NULL!"));
        Debug.Log("[DEBUG] PannelloCondizioni.Instance: " + (PannelloCondizioni.Instance != null ? "OK" : "NULL!"));
        Debug.Log("[DEBUG] slotAttivoCondizione: " + (GestoreBlocchi.Instance?.slotAttivoCondizione != null ? GestoreBlocchi.Instance.slotAttivoCondizione.name : "NULL"));
    }
}
