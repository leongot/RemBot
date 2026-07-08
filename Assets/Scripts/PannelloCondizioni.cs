using UnityEngine;

public class PannelloCondizioni : MonoBehaviour
{
    public static PannelloCondizioni Instance;

    private void Awake()
    {
        Instance = this;
        
        gameObject.SetActive(false);
    }

    
    public void Mostra()
    {
        gameObject.SetActive(true);
    }

    
    public void Nascondi()
    {
        gameObject.SetActive(false);
    }
}
