using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GlobalAlarmLightEffect : MonoBehaviour
{
    public static GlobalAlarmLightEffect Instance;

    [Header("Luce Globale Allarme")]
    [SerializeField] private Light alarmLight;

    [Header("Effetto Lampeggio")]
    //[SerializeField] private Color coloreAllarme = Color.red;
    [SerializeField] private float durataEffetto = 3f;
    [SerializeField] private float intensitaMassima = 2.5f;
    [SerializeField] private float velocitaLampeggio = 8f;

    [Header("Ambiente Rosso")]
    [SerializeField] private bool modificaLuceAmbientale = true;
    [SerializeField] private float forzaAmbienteRosso = 0.4f;

    private Coroutine alarmCoroutine;

    private Color coloreOriginaleLuce;
    private float intensitaOriginaleLuce;
    private bool luceAttivaOriginale;

    private AmbientMode ambientModeOriginale;
    private Color ambientLightOriginale;

    private void Awake()
    {
        Instance = this;

        if (alarmLight != null)
        {
            coloreOriginaleLuce = alarmLight.color;
            intensitaOriginaleLuce = alarmLight.intensity;
            luceAttivaOriginale = alarmLight.enabled;

            //alarmLight.color = coloreAllarme;
            alarmLight.intensity = 0f;
            alarmLight.enabled = true;
        }

        ambientModeOriginale = RenderSettings.ambientMode;
        ambientLightOriginale = RenderSettings.ambientLight;
    }

    public void PlayAlarm()
    {
        if (alarmCoroutine != null)
        {
            StopCoroutine(alarmCoroutine);
            RipristinaLuce();
        }

        alarmCoroutine = StartCoroutine(AlarmRoutine());
    }

    private IEnumerator AlarmRoutine()
    {
        float tempo = 0f;

        while (tempo < durataEffetto)
        {
            tempo += Time.deltaTime;

            float lampeggio = Mathf.Abs(Mathf.Sin(Time.time * velocitaLampeggio));

            if (alarmLight != null)
            {
                alarmLight.enabled = true;
                //alarmLight.color = coloreAllarme;
                alarmLight.intensity = lampeggio * intensitaMassima;
            }

            if (modificaLuceAmbientale)
            {
                RenderSettings.ambientMode = AmbientMode.Flat;

                RenderSettings.ambientLight = Color.Lerp(
                    ambientLightOriginale,
                    alarmLight.color,
                    lampeggio * forzaAmbienteRosso
                );
            }

            yield return null;
        }

        RipristinaLuce();
        alarmCoroutine = null;
    }

    private void RipristinaLuce()
    {
        if (alarmLight != null)
        {
            alarmLight.color = coloreOriginaleLuce;
            alarmLight.intensity = intensitaOriginaleLuce;
            alarmLight.enabled = luceAttivaOriginale;
        }

        if (modificaLuceAmbientale)
        {
            RenderSettings.ambientMode = ambientModeOriginale;
            RenderSettings.ambientLight = ambientLightOriginale;
        }
    }
}   