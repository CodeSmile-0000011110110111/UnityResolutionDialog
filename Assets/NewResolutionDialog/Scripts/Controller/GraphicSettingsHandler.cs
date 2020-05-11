using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#pragma warning disable 0649

public class GraphicSettingsHandler : MonoBehaviour
{
    [SerializeField] Settings settings;
    [SerializeField] Dropdown resolution;
    [SerializeField] Dropdown refreshRate;
    [SerializeField] Dropdown fullScreenMode;
    [SerializeField] Dropdown vSync;
    [SerializeField] Text vSyncNote;
    [SerializeField] Dropdown quality;
    [SerializeField] Dropdown display;
    [SerializeField] Text displayNote;

    Dictionary<string, List<string>> refreshRates = new Dictionary<string, List<string>>();
    FullScreenMode launchFullScreenMode = FullScreenMode.Windowed;
    bool updatingDialog = true;

    bool IsActivationKeyHeldDown()
    {
        // this doesn't work since keys are only available in Update(), and then only from the 3rd frame onward
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    void Awake()
    {
        displayNote.gameObject.SetActive(false);
        vSyncNote.gameObject.SetActive(false);

        //if (settings.HiddenByDefault == false || IsActivationKeyHeldDown())
        {
            // ensure dialog always launches in window mode
            launchFullScreenMode = Screen.fullScreenMode;
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
        /*
        else
        {
            // load next scene immediately
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            this.enabled = false;
        }
        */
    }

    void Start()
    {
        updatingDialog = true;
        UpdateSettings();
        updatingDialog = false;
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
    }


    int frameCount = 0;
    bool firstKeyPressed = false;
    private void Update()
    {
        frameCount++;
        if (firstKeyPressed == false)
        {
            if (IsActivationKeyHeldDown())
            {
                Debug.LogError("Update #" + frameCount + " - KEY DOWN!");
                firstKeyPressed = true;
            }
        }
    }

    void UpdateDialogAfterEndOfFrame()
    {
        if (updatingDialog == false)
        {
            updatingDialog = true; // ensure only one coroutine updates
            StartCoroutine(UpdateSettingsAfterEndOfFrame());
        }
    }

    IEnumerator UpdateSettingsAfterEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        UpdateSettings();
        yield return new WaitForEndOfFrame();
        updatingDialog = false;
    }

    void UpdateSettings()
    {
        // TODO: split into populating and selecting an item, with or without applying the change
        PopulateResolutionsDropdown();
        PopulateRefresRateDropdown();
        InitFullScreenMode();
        InitVSync();
        PopulateQualityDropdown();
        PopulateMonitorDropdown();
        ApplyResolution();
        SetRefreshRateEnabled(Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen);
    }

    void PopulateResolutionsDropdown()
    {
        refreshRates.Clear();
        resolution.ClearOptions();
        var options = new List<Dropdown.OptionData>();

        Dropdown.OptionData currentOption = null;
        var currentRes = Screen.currentResolution;
        var resolutions = Screen.resolutions;
        foreach (var res in resolutions)
        {
            var resParts = res.ToString().Split(new char[] { 'x', '@', 'H' });
            var width = resParts[0];
            var height = resParts[1];
            var hz = resParts[2];

            var resString = width + "x" + height;

            // resolution
            if (refreshRates.ContainsKey(resString) == false)
            {
                refreshRates.Add(resString, new List<string>());
                var option = new Dropdown.OptionData(resString);
                options.Add(option);

                // remember initial resolution
                if (res.width == currentRes.width && res.height == currentRes.height)
                    currentOption = option;
            }

            // refresh rate
            refreshRates[resString].Add(hz);
        }

        resolution.AddOptions(options);
        resolution.value = resolution.options.IndexOf(currentOption); // select current resolution
        resolution.interactable = !Application.isEditor;
    }

    void PopulateRefresRateDropdown()
    {
        refreshRate.ClearOptions();
        var options = new List<Dropdown.OptionData>();

        var selectedRes = resolution.options[resolution.value].text;
        var refreshRatesForCurrentRes = refreshRates[selectedRes];

        foreach (var hz in refreshRatesForCurrentRes)
        {
            var option = new Dropdown.OptionData(hz + " Hz");
            options.Add(option);
        }

        refreshRate.AddOptions(options);
        refreshRate.value = refreshRate.options.Count - 1; // select highest refresh rate
        refreshRate.interactable = refreshRatesForCurrentRes.Count > 1 && Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
    }

    void SelectRefreshRate(string hz)
    {
        for (int i = 0; i < this.refreshRate.options.Count; i++)
        {
            var refreshRate = this.refreshRate.options[i];
            if (refreshRate.text == hz)
            {
                this.refreshRate.value = i;
                break;
            }
        }
    }

    void SetRefreshRateEnabled(bool enabled)
    {
        if (!enabled)
        {
            refreshRate.ClearOptions();
            refreshRate.AddOptions(new List<string>() { "N/A" });
        }

        refreshRate.interactable = enabled;
    }

    string GetSelectedResolution()
    {
        return resolution.options[resolution.value].text;
    }

    string GetSelectedRefreshRate()
    {
        return refreshRate.options[refreshRate.value].text;
    }

    public void OnResolutionChanged()
    {
        //var selectedHz = GetSelectedRefreshRate();

        ApplyResolution();

        /*
        // update refresh rates and try to select the same refresh rate as before, if available
        PopulateRefresRateDropdown();
        SelectRefreshRate(selectedHz);
        selectedHz = GetSelectedRefreshRate(); // Hz may have changed if previous Hz isn't available
        */

        UpdateDialogAfterEndOfFrame();
    }

    public void OnRefreshRateChanged()
    {
        ApplyResolution();
        UpdateDialogAfterEndOfFrame();
    }

    void ApplyResolution()
    {
        var selectedHz = GetSelectedRefreshRate();
        var selectedRes = GetSelectedResolution();
        var resolution = selectedRes.Split(new char[] { 'x' });

        var width = int.Parse(resolution[0]);
        var height = int.Parse(resolution[1]);
        var hz = selectedHz.Equals("N/A") ? 0 : int.Parse(selectedHz.Replace("Hz", ""));
        var mode = (FullScreenMode)fullScreenMode.value;
        SetResolution(width, height, mode, hz);
    }

    void SetResolution(int width, int height, FullScreenMode mode, int hz)
    {
        // prevent setting resolution multiple times when dialog is updated in the next frame
        Debug.LogError("Changing resolution to: " + width + "x" + height + " @ " + hz + " Hz in " + mode);
        Screen.SetResolution(width, height, mode, hz);
    }

    void PopulateQualityDropdown()
    {
        var options = new List<Dropdown.OptionData>();

        Dropdown.OptionData currentOption = null;
        var currentLevel = QualitySettings.GetQualityLevel();
        var qualityLevels = QualitySettings.names;
        foreach (var quality in qualityLevels)
        {
            var option = new Dropdown.OptionData(quality);
            options.Add(option);

            // remember initial quality level
            if (quality == qualityLevels[currentLevel])
                currentOption = option;
        }

        quality.ClearOptions();
        quality.AddOptions(options);
        quality.value = quality.options.IndexOf(currentOption); // select current quality level
        quality.interactable = qualityLevels.Length > 1;
    }

    public void OnQualityLevelChanged()
    {
        var selectedText = quality.options[quality.value].text;
        QualitySettings.SetQualityLevel(new List<string>(QualitySettings.names).IndexOf(selectedText), true);

        // update vsync settings as it may be affected by quality level
        vSync.value = QualitySettings.vSyncCount;

        UpdateDialogAfterEndOfFrame();
    }

    void InitFullScreenMode()
    {
        Screen.fullScreenMode = launchFullScreenMode;
        fullScreenMode.value = (int)Screen.fullScreenMode;

        // we can't switch fullscreen modes in editor
        if (Application.isEditor)
            fullScreenMode.interactable = false;
    }
    public void OnFullScreenModeChanged()
    {
        var mode = (FullScreenMode)fullScreenMode.value;
        Screen.fullScreenMode = mode;
        SetRefreshRateEnabled(Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen);

        // update dropdown selection in case we couldn't switch modes for some reason
        if (mode != Screen.fullScreenMode)
            fullScreenMode.value = (int)Screen.fullScreenMode;

        UpdateDialogAfterEndOfFrame();
    }

    void InitVSync()
    {
        vSync.value = QualitySettings.vSyncCount;
        vSyncNote.gameObject.SetActive(QualitySettings.vSyncCount == 0);
        vSync.interactable = !Application.isEditor;
    }

    public void OnVSyncChanged()
    {
        QualitySettings.vSyncCount = vSync.value;

        UpdateDialogAfterEndOfFrame();
    }

    void PopulateMonitorDropdown()
    {
        display.ClearOptions();
        var options = new List<Dropdown.OptionData>();

        Dropdown.OptionData currentOption = null;
        for (int i = 0; i < Display.displays.Length; i++)
        {
            var display = Display.displays[i];

            var displayString = "Diplay " + (i + 1) + " (" + display.renderingWidth + "x" + display.renderingHeight + ")";
            var option = new Dropdown.OptionData(displayString);
            options.Add(option);

            // select active display
            if (display.active)
                currentOption = option;
        }

        display.AddOptions(options);
        display.value = display.options.IndexOf(currentOption);
        display.interactable = Display.displays.Length > 1;
    }

    public void OnMonitorChanged()
    {
        // currently does not attempt to auto-restart application, it merely removes the Play button thus forcing user to restart
        // for one, restarting would have to be coded separately for each platform (Windows, Mac, Linux, possibly won't work with UWP)
        // secondly, the "ForceSingleInstance" flag would prevent a restart without additional do-hickery (external app or batch file)

        UpdateDialogAfterEndOfFrame();
    }
}
