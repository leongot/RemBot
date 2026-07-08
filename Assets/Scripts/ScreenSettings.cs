using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    
    private List<Resolution> filteredResolutions = new List<Resolution>();

    private void Start()
    {
        if (!PlayerPrefs.HasKey("HasSetInitialResolution"))
        {
            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, FullScreenMode.FullScreenWindow);
            PlayerPrefs.SetInt("HasSetInitialResolution", 1);
            PlayerPrefs.Save();
        }

        Resolution[] allResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        List<string> uniqueResolutionStrings = new List<string>();

        for (int i = 0; i < allResolutions.Length; i++)
        {
            string option = allResolutions[i].width + " x " + allResolutions[i].height;
            
            if (!uniqueResolutionStrings.Contains(option))
            {
                uniqueResolutionStrings.Add(option);
                options.Add(option);
                
                
                filteredResolutions.Add(allResolutions[i]);
            }

            
            if (allResolutions[i].width == Screen.width && allResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = options.Count - 1;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        
        if (resolutionIndex < 0 || resolutionIndex >= filteredResolutions.Count) return;

        
        Resolution selectedRes = filteredResolutions[resolutionIndex];
        Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreenMode);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreen = isFullscreen;
    }

    public void RefreshResolutionOptions()
    {
       
        Resolution[] allResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        filteredResolutions.Clear();
        List<string> uniqueResolutionStrings = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            string option = allResolutions[i].width + " x " + allResolutions[i].height;
            
            if (!uniqueResolutionStrings.Contains(option))
            {
                uniqueResolutionStrings.Add(option);
                options.Add(option);
                filteredResolutions.Add(allResolutions[i]);
            }

          
            if (allResolutions[i].width == Screen.width && allResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = options.Count - 1;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

}