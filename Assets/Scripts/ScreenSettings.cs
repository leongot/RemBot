using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    // CHANGE: Store the actual filtered resolutions so the indices match the dropdown 1:1
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
                
                // Keep track of the actual Resolution struct matching this UI slot
                filteredResolutions.Add(allResolutions[i]);
            }

            // Track if this matches the machine's current active screen resolution
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
        // Safety guard clause
        if (resolutionIndex < 0 || resolutionIndex >= filteredResolutions.Count) return;

        // FIX: Pull from our cleanly matching filtered list instead of the raw array!
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
        // Re-read the physical monitor capabilities in case they plugged in a new screen
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

            // Match against the current window width/height dynamically
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