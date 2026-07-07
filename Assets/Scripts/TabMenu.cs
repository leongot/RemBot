using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// 1. Force the script to execute lifecycle methods in Editor Mode
[ExecuteAlways]
public class TabMenu : MonoBehaviour
{
    [Header("Current Index")]
    [SerializeField] private int pageIndex = 0;

    [Header("Components")]
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private List<Toggle> tabs = new List<Toggle>();
    [SerializeField] private List<CanvasGroup> pages = new List<CanvasGroup>();

    [Header("Events to call")]
    public UnityEvent<int> OnPageIndexChanged;

    private void Initialize()
    {
        if (toggleGroup == null)
        {
            toggleGroup = GetComponentInChildren<ToggleGroup>();
        }

        if (tabs.Count == 0)
        {
            tabs.Clear();
            if (toggleGroup != null)
            {
                tabs.AddRange(toggleGroup.GetComponentsInChildren<Toggle>());
            }
        }

        if (pages.Count == 0)
        {
            pages.Clear();
            foreach (var group in GetComponentsInChildren<CanvasGroup>(true))
            {
                if (group.transform == transform) continue;
                if (toggleGroup != null && group.transform.IsChildOf(toggleGroup.transform)) continue;
                pages.Add(group);
            }
        }
    }

    private void Reset()
    {
        Initialize();
    }

    private void OnValidate()
    {
        Initialize();
        EnsureIndexIsInRange(pageIndex);

        if (tabs.Count > 0 && pageIndex < tabs.Count && tabs[pageIndex] != null)
        {
            tabs[pageIndex].SetIsOnWithoutNotify(true);
        }

        RefreshPageState();
    }

    private void Awake()
    {
        Initialize();

        // Only register runtime events if we are actually playing the game
        if (Application.isPlaying)
        {
            foreach (var toggle in tabs)
            {
                if (toggle == null) continue;

                toggle.onValueChanged.AddListener(CheckForTab);
                if (toggleGroup != null)
                {
                    toggle.group = toggleGroup;
                }
            }
        }

        RefreshPageState();
    }

    private void Start()
    {
        if (tabs.Count > 0)
        {
            EnsureIndexIsInRange(pageIndex);
            if (tabs[pageIndex] != null)
            {
                tabs[pageIndex].SetIsOnWithoutNotify(true);
            }
            OpenPage(pageIndex);
        }
    }

    private void OnDestroy()
    {
        foreach (var toggle in tabs)
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(CheckForTab);
        }
    }

    private void CheckForTab(bool isOn)
    {
        if (!isOn) return;

        for (int i = 0; i < tabs.Count; i++)
        {
            if (!tabs[i].isOn) continue;
            pageIndex = i;
            OpenPage(pageIndex);
            return;
        }
    }

    private void OpenPage(int index)
    {
        EnsureIndexIsInRange(index);
        ApplyPageState();
    }

    private void RefreshPageState()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall -= ApplyPageState;
            UnityEditor.EditorApplication.delayCall += ApplyPageState;
            return;
        }
#endif
        ApplyPageState();
    }

    private void ApplyPageState()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] == null) continue; // Safety guard clause

            bool isActivePage = (i == pageIndex);
            pages[i].alpha = isActivePage ? 1.0f : 0.0f;
            pages[i].interactable = isActivePage;
            pages[i].blocksRaycasts = isActivePage;
        }

        if (Application.isPlaying)
        {
            OnPageIndexChanged?.Invoke(pageIndex);
        }
    }

    private void EnsureIndexIsInRange(int index)
    {
        if (tabs.Count == 0 || pages.Count == 0)
        {
            return;
        }

        pageIndex = Mathf.Clamp(index, 0, pages.Count - 1);
    }

    public void JumpToPage(int index)
    {
        EnsureIndexIsInRange(index);
        tabs[pageIndex].isOn = true;
    }
}