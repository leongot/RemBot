using UnityEngine;
public enum TipoBlocco
{
    Avanti, Sinistra, Destra, Indietro, Spara, // Comandi
    If, While,                                 // Controlli
    Libero, Muro, Nemico, Sempre               // Condizioni
}

public class InfoBlocco : MonoBehaviour
{
    public TipoBlocco tipo;
}