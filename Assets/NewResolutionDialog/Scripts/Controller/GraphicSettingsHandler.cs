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
    [SerializeField] Dropdown resolution;
    [SerializeField] Dropdown refreshRate;
    [SerializeField] Dropdown fullScreenMode;
    [SerializeField] Dropdown vSync;
    [SerializeField] Text vSyncNote;
    [SerializeField] Dropdown quality;
    [SerializeField] Dropdown display;
    [SerializeField] Text displayNote;

    Dictionary<string, List<string>> refreshRates = new Dictionary<string, List<string>>();
    bool updatingDialog = true;

    static readonly string hzSuffix = " Hz";


    #region Dialog Getters
    string GetSelectedResolution()
    {
        return resolution.options[resolution.value].text;
    }
    string GetResolutionString()
    {
        return GetResolutionString(Screen.currentResolution.width, Screen.currentResolution.height);
    }
    string GetResolutionString(int w, int h)
    {
        return string.Format("{0}x{1}", w, h);
    }

    string GetSelectedRefreshRate()
    {
        return refreshRate.options[refreshRate.value].text;
    }

    bool IsActivationKeyHeldDown()
    {
        // Note: keys are only available within Update() and there only from the 2nd or 3rd frame onward
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
    #endregion

    #region Unity Startup
    void Awake()
    {
        displayNote.gameObject.SetActive(false);
        vSyncNote.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        PopulateDropdowns();
        ApplyCurrentSettingsToDialog();
        UpdateDialogInteractability();
    }
    #endregion

    #region Update
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
    #endregion

    #region PopulateDropdowns
    void PopulateDropdowns()
    {
        // TODO: split into populating and selecting an item, with or without applying the change
        PopulateResolutionsDropdown();
        PopulateRefresRateDropdown();
        PopulateQualityDropdown();
        PopulateMonitorDropdown();
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
            var width = resParts[0].Trim();
            var height = resParts[1].Trim();
            var hz = resParts[2].Trim();

            var resString = GetResolutionString(int.Parse(width), int.Parse(height));

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
    }
    void PopulateRefresRateDropdown()
    {
        refreshRate.ClearOptions();
        var options = new List<Dropdown.OptionData>();

        var selectedRes = resolution.options[resolution.value].text;
        var refreshRatesForCurrentRes = refreshRates[selectedRes];

        foreach (var hz in refreshRatesForCurrentRes)
        {
            var option = new Dropdown.OptionData(hz + hzSuffix);
            options.Add(option);
        }

        refreshRate.AddOptions(options);
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
    }

    void PopulateMonitorDropdown()
    {
        display.ClearOptions();
        var options = new List<Dropdown.OptionData>();

        Dropdown.OptionData currentOption = null;
        for (int i = 0; i < Display.displays.Length; i++)
        {
            var display = Display.displays[i];

            var displayString = "Diplay " + (i + 1) + " (" + GetResolutionString(display.renderingWidth, display.renderingHeight) + ")";
            var option = new Dropdown.OptionData(displayString);
            options.Add(option);

            // select active display
            if (display.active)
                currentOption = option;
        }

        display.AddOptions(options);
    }
    #endregion

    #region UpdateDialogWithCurrentSettings
    void ApplyCurrentSettingsToDialog()
    {
        updatingDialog = true;

        SelectCurrentResolution();
        SelectCurrentRefreshRate();
        /*
        resolution.interactable = !Application.isEditor;

        refreshRate.value = refreshRate.options.Count - 1; // select highest refresh rate
        refreshRate.interactable = refreshRatesForCurrentRes.Count > 1 && Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;

        quality.value = quality.options.IndexOf(currentOption); // select current quality level
        quality.interactable = qualityLevels.Length > 1;
    
        Screen.fullScreenMode = launchFullScreenMode;
        fullScreenMode.value = (int)Screen.fullScreenMode;
        // we can't switch fullscreen modes in editor
        if (Application.isEditor)
            fullScreenMode.interactable = false;

        vSync.value = QualitySettings.vSyncCount;
        vSyncNote.gameObject.SetActive(QualitySettings.vSyncCount == 0);
        vSync.interactable = !Application.isEditor;

        display.value = display.options.IndexOf(currentOption);
        display.interactable = Display.displays.Length > 1;

         * */

        //displayNote.gameObject.SetActive(false);
        //vSyncNote.gameObject.SetActive(false);

        updatingDialog = false;
    }

    void SelectCurrentResolution()
    {
        // select lowest by default
        resolution.value = 0;

        var res = GetResolutionString();
        for (int i = 0; i < resolution.options.Count; i++)
        {
            if (resolution.options[i].text == res)
            {
                resolution.value = i;
                break;
            }
        }
    }

    void SelectCurrentRefreshRate()
    {
        // select highest by default
        refreshRate.value = refreshRate.options.Count - 1;

        string hz = Screen.currentResolution.refreshRate + hzSuffix;
        for (int i = 0; i < refreshRate.options.Count; i++)
        {
            if (refreshRate.options[i].text == hz)
            {
                refreshRate.value = i;
                break;
            }
        }
    }
    #endregion

    #region UpdateInteractability
    void UpdateDialogInteractability()
    {
        if (Application.isEditor)
        {
            // in editor mode we can only change quality level, everything else is not applicable or has to be changed through game view settings
            resolution.interactable = false;
            SetRefreshRateInteractable(false);
            fullScreenMode.interactable = false;
            vSync.interactable = false;
            resolution.interactable = false;
            quality.interactable = true;
            display.interactable = false;
        }
        else
        {
            var res = GetResolutionString();
            
            resolution.interactable = true;
            SetRefreshRateInteractable(refreshRates[res].Count > 1 && Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen);
            fullScreenMode.interactable = true;
            vSync.interactable = false;
            resolution.interactable = false;
            quality.interactable = true;
            display.interactable = false;
        }
    }

    void SetRefreshRateInteractable(bool interactable)
    {
        if (!interactable)
        {
            refreshRate.ClearOptions();
            refreshRate.AddOptions(new List<string>() { "N/A" });
        }

        refreshRate.interactable = interactable;
    }
    #endregion

    #region Event Handlers
    public void OnResolutionChanged()
    {
        if (updatingDialog)
            return;

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
        if (updatingDialog)
            return;

        ApplyResolution();
        UpdateDialogAfterEndOfFrame();
    }

    public void OnQualityLevelChanged()
    {
        if (updatingDialog)
            return;

        var selectedText = quality.options[quality.value].text;
        QualitySettings.SetQualityLevel(new List<string>(QualitySettings.names).IndexOf(selectedText), true);

        // update vsync settings as it may be affected by quality level
        vSync.value = QualitySettings.vSyncCount;

        UpdateDialogAfterEndOfFrame();
    }

    public void OnFullScreenModeChanged()
    {
        if (updatingDialog)
            return;

        var mode = (FullScreenMode)fullScreenMode.value;
        Screen.fullScreenMode = mode;
        SetRefreshRateInteractable(Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen);

        // update dropdown selection in case we couldn't switch modes for some reason
        if (mode != Screen.fullScreenMode)
            fullScreenMode.value = (int)Screen.fullScreenMode;

        UpdateDialogAfterEndOfFrame();
    }

    public void OnVSyncChanged()
    {
        if (updatingDialog)
            return;

        QualitySettings.vSyncCount = vSync.value;

        UpdateDialogAfterEndOfFrame();
    }


    public void OnMonitorChanged()
    {
        if (updatingDialog)
            return;

        // currently does not attempt to auto-restart application, it merely removes the Play button thus forcing user to restart
        // for one, restarting would have to be coded separately for each platform (Windows, Mac, Linux, possibly won't work with UWP)
        // secondly, the "ForceSingleInstance" flag would prevent a restart without additional do-hickery (external app or batch file)

        UpdateDialogAfterEndOfFrame();
    }
    #endregion

    #region Apply Changes
    void ApplyResolution()
    {
        var selectedHz = GetSelectedRefreshRate();
        var selectedRes = GetSelectedResolution();
        var resolution = selectedRes.Split(new char[] { 'x' });

        var width = int.Parse(resolution[0]);
        var height = int.Parse(resolution[1]);
        var hz = selectedHz.Equals("N/A") ? 0 : int.Parse(selectedHz.Replace(hzSuffix, ""));
        var mode = (FullScreenMode)fullScreenMode.value;
        SetResolution(width, height, mode, hz);
    }

    void SetResolution(int width, int height, FullScreenMode mode, int hz)
    {
        // prevent setting resolution multiple times when dialog is updated in the next frame
        Debug.LogError("Changing resolution to: " + GetResolutionString(width, height) + " @ " + hz + " Hz in " + mode);
        Screen.SetResolution(width, height, mode, hz);
    }
    #endregion

    #region Update After Frame 
    void UpdateDialogAfterEndOfFrame()
    {
        StartCoroutine(UpdateDialogAfterEndOfFrameCoroutine());
    }

    IEnumerator UpdateDialogAfterEndOfFrameCoroutine()
    {
        // must wait for end of this AND next frame for the new resolution to be applied
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Debug.LogError("NEW Resolution: " + Screen.currentResolution);
        PopulateDropdowns();
        ApplyCurrentSettingsToDialog();
        UpdateDialogInteractability();
    }
    #endregion
}
