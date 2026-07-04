using UnityEngine;

/// <summary>
/// Metti questo script su un pannello UI (es. "Pannello_Condizioni") 
/// che contiene i 3 bottoni LIBERO, MURO, NEMICO.
///
/// Il pannello si mostra automaticamente quando l'utente clicca 
/// su una casella gialla (SlotCondizione), e si nasconde dopo
/// che l'utente ha scelto una condizione.
/// </summary>
public class PannelloCondizioni : MonoBehaviour
{
    public static PannelloCondizioni Instance;

    private void Awake()
    {
        Instance = this;
        // Il pannello parte nascosto
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Chiamato da SlotCondizione quando l'utente clicca su una casella gialla.
    /// </summary>
    public void Mostra()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Chiamato da GestoreBlocchi dopo che l'utente ha scelto una condizione.
    /// </summary>
    public void Nascondi()
    {
        gameObject.SetActive(false);
    }
}
