using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace NewResolutionDialog.Scripts.Controller
{
    public class GraphicSettingsHandler : MonoBehaviour
    {
        #region Fields and Stuff
        [SerializeField] Dropdown resolution;
        [SerializeField] Dropdown refreshRate;
        [SerializeField] Dropdown fullScreenMode;
        [SerializeField] Dropdown vSync;
        [SerializeField] Text vSyncNote;
        [SerializeField] Dropdown quality;
        [SerializeField] Dropdown display;
        [SerializeField] Text displayNote;

        static readonly string hzSuffix = " Hz";
        static readonly string prefsKey_RefreshRate = "NewResolutionDialog_RefreshRate";
        static readonly string prefsKey_VSyncCount = "NewResolutionDialog_vSyncCount";

        Dictionary<string, List<string>> refreshRates = new Dictionary<string, List<string>>();
        bool updatingDialog = true;
        #endregion

        #region Dialog Getters
        string GetSelectedResolution()
        {
            return resolution.options[resolution.value].text;
        }
        string GetResolutionString()
        {
            return GetResolutionString(Screen.width, Screen.height);
        }
        string GetResolutionString(int w, int h)
        {
            return string.Format("{0}x{1}", w, h);
        }

        string GetSelectedRefreshRate()
        {
            return refreshRate.options[refreshRate.value].text.Replace(hzSuffix, "");
        }

        FullScreenMode GetSelectedFullScreenMode()
        {
            var mode = (FullScreenMode)fullScreenMode.value;

            // In the dropdown "MaximizedWindow" (#2 in enum) was removed because it ain't working, it is now "Windowed" (#3 in enum), hence this fix
            // see: https://issuetracker.unity3d.com/issues/fullscreen-mode-maximized-window-functionality-is-broken-and-any-built-player-changes-to-non-window-mode-when-maximizing
            if (mode == FullScreenMode.MaximizedWindow)
                mode = FullScreenMode.Windowed;

            return mode;
        }
        #endregion

        #region Debug
        void LogRefreshRates()
        {
            var sb = new StringBuilder("\n");
            foreach (var kvp in refreshRates)
            {
                sb.AppendFormat("{0} => ", kvp.Key);
                foreach (var hz in kvp.Value)
                {
                    sb.AppendFormat("{0}, ", hz);
                }
                sb.Append("\n");
            }

            sb.Append("\n");
            Debug.Log(sb);
        }
        #endregion

        #region Unity Startup
        void Awake()
        {
            displayNote.gameObject.SetActive(false);
            vSyncNote.gameObject.SetActive(false);

            var hz = PlayerPrefs.GetInt(prefsKey_RefreshRate, 0);
            if (hz != 0)
            {
                if (hz != Screen.currentResolution.refreshRate)
                {
                    SetResolution(Screen.width, Screen.height, Screen.fullScreenMode, PlayerPrefs.GetInt(prefsKey_RefreshRate, 0));
                    UpdateDialogAfterEndOfFrame();
                }
            }

            var vSyncCount = PlayerPrefs.GetInt(prefsKey_VSyncCount, QualitySettings.vSyncCount);
            if (vSyncCount != QualitySettings.vSyncCount)
                QualitySettings.vSyncCount = PlayerPrefs.GetInt(prefsKey_VSyncCount, QualitySettings.vSyncCount);
        }

        void OnEnable()
        {
            PopulateDropdowns();
            ApplyCurrentSettingsToDialog();
            UpdateDialogInteractability();
        }
        #endregion

        #region PopulateDropdowns
        void PopulateDropdowns()
        {
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
            var resolutions = Screen.resolutions;
            var isWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;
            var isFullScreenWindow = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
            var systemWidth = Display.main.systemWidth;
            var systemHeight = Display.main.systemHeight;

            foreach (var res in resolutions)
            {
                var resParts = res.ToString().Split(new char[] { 'x', '@', 'H' });
                var width = int.Parse(resParts[0].Trim());
                var height = int.Parse(resParts[1].Trim());

                // skip resolutions that won't fit in windowed modes
                if (isWindowed && (width >= systemWidth || height >= systemHeight))
                    continue;
                if (isFullScreenWindow && (width > systemWidth || height > systemHeight))
                    continue;

                // resolution
                var resString = GetResolutionString(width, height);
                if (refreshRates.ContainsKey(resString) == false)
                {
                    refreshRates.Add(resString, new List<string>());
                    var option = new Dropdown.OptionData(resString);
                    options.Add(option);
                }

                // refresh rates without 'Hz' suffix
                var hz = resParts[2].Trim();
                refreshRates[resString].Add(hz);
            }

            resolution.AddOptions(options);
        }
        void PopulateRefresRateDropdown()
        {
            refreshRate.ClearOptions();
            var options = new List<Dropdown.OptionData>();

            var selectedRes = GetResolutionString();

            if (refreshRates.ContainsKey(selectedRes))
            {
                var refreshRatesForCurrentRes = refreshRates[selectedRes];

                foreach (var hz in refreshRatesForCurrentRes)
                {
                    var option = new Dropdown.OptionData(hz + hzSuffix);
                    options.Add(option);
                }

                refreshRate.AddOptions(options);
            }
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

                var displayString = "Diplay " + (i + 1) + " (" + GetResolutionString(display.systemWidth, display.systemHeight) + ")";
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

            SelectCurrentResolutionDropdownItem();
            SelectCurrentRefreshRateDropdownItem();
            SelectCurrentFullScreenModeDropdownItem();
            SelectCurrentVSyncCountDropdownItem();
            SelectCurrentQualityLevelDropdownItem();
            SelectCurrentDisplayDropdownItem();

            updatingDialog = false;
        }

        void SelectCurrentResolutionDropdownItem()
        {
            // select highest by default
            resolution.value = resolution.options.Count - 1;

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

        void SelectCurrentRefreshRateDropdownItem()
        {
            // try to find closest Hz
            var hz = Screen.currentResolution.refreshRate;

            var closestDiff = int.MaxValue;
            var closestOptionIndex = refreshRate.options.Count - 1;
            for (int i = 0; i < refreshRate.options.Count; i++)
            {
                var optionHz = int.Parse(refreshRate.options[i].text.Replace(hzSuffix, ""));
                var diff = Mathf.Abs(hz - optionHz);
                if (diff < closestDiff)
                {
                    closestDiff = diff;
                    closestOptionIndex = i;
                    if (closestDiff == 0)
                        break;
                }
            }

            refreshRate.value = closestOptionIndex;
        }

        void SelectCurrentFullScreenModeDropdownItem()
        {
            var mode = Screen.fullScreenMode;
            if (Screen.fullScreenMode == FullScreenMode.MaximizedWindow)
                mode = FullScreenMode.FullScreenWindow;

            fullScreenMode.value = (int)mode;
        }

        void SelectCurrentVSyncCountDropdownItem()
        {
            vSync.value = QualitySettings.vSyncCount;

            // push a fair warning to users disabling vsync ignoring freesync/gsync users for whom this setting makes sense
            // in the majority of use cases, particularly on battery-powered devices, turning vsync off is detrimental to battery life and comfort (excess heat, noise)
            vSyncNote.gameObject.SetActive(QualitySettings.vSyncCount == 0);
        }

        void SelectCurrentQualityLevelDropdownItem()
        {
            quality.value = QualitySettings.GetQualityLevel();
        }
        void SelectCurrentDisplayDropdownItem()
        {
            // take the first active display
            for (int i = 0; i < Display.displays.Length; i++)
            {
                if (Display.displays[i].active)
                {
                    display.value = i;
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
                // in editor mode these settings are not applicable, some can be changed through the game view's settings (ie resolution)
                resolution.interactable = false; // change this through game view
                SetRefreshRateInteractable(false); // not applicable, always refresh rate of desktop
                fullScreenMode.interactable = false; // not applicable, always "windowed"
                vSync.interactable = false; // not applicable, vsync has no effect in editor mode
                quality.interactable = quality.options.Count > 1; // interactable if there is more than one quality level to select from
                display.interactable = false; // not applicable, same display as editor runs on unless game view is detached
            }
            else
            {
                resolution.interactable = true; // always interactable
                SetRefreshRateInteractable(Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen); // only in fullscreen mode, the window modes use desktop refresh rate
                fullScreenMode.interactable = true; // always interactable
                vSync.interactable = true; // always interactable
                quality.interactable = quality.options.Count > 1; // interactable if there is more than one quality level to select from
                display.interactable = display.options.Count > 1; // only interactable if there are multiple displays
            }
        }

        void SetRefreshRateInteractable(bool interactable)
        {
            if (!interactable)
            {
                refreshRate.ClearOptions();
                refreshRate.AddOptions(new List<string>() { "N/A" });
            }

            // only allow interactable if there are multiple refresh rates to select from
            refreshRate.interactable = interactable && refreshRates[GetResolutionString()].Count > 1;
        }
        #endregion

        #region Event Handlers
        public void OnResolutionChanged()
        {
            if (updatingDialog)
                return;

            ApplySelectedResolution();
            UpdateDialogAfterEndOfFrame();
        }

        public void OnRefreshRateChanged()
        {
            if (updatingDialog)
                return;

            int hz = 0;
            int.TryParse(GetSelectedRefreshRate(), out hz);
            PlayerPrefs.SetInt(prefsKey_RefreshRate, hz);

            ApplySelectedResolution();
            UpdateDialogAfterEndOfFrame();
        }

        public void OnFullScreenModeChanged()
        {
            if (updatingDialog)
                return;

            var wasWindowed = Screen.fullScreenMode == FullScreenMode.Windowed;

            var mode = GetSelectedFullScreenMode();
            Screen.fullScreenMode = mode;

            if (mode == FullScreenMode.Windowed)
            {
                var selectedRes = GetSelectedResolution();
                var resolution = selectedRes.Split(new char[] { 'x' });
                var width = int.Parse(resolution[0]);
                var height = int.Parse(resolution[1]);
                var screenWidth = Display.main.systemWidth;
                var screenHeight = Display.main.systemHeight;
                //Debug.LogError("cur w/h: " + width + "x" + height + ", max w/h: " + screenWidth + "x" + screenHeight + ", Scr w/h: " + Screen.width + "x" + Screen.height + 
                //    ", DspR w/h: " + Display.main.renderingWidth + "x" + Display.main.renderingHeight + ", DspS w/h: " + Display.main.systemWidth + "x" + Display.main.systemHeight);

                if (width >= screenWidth || height >= screenHeight)
                {
                    var closestWidth = screenWidth;
                    var closestHeight = screenHeight;
                    foreach (var res in Screen.resolutions)
                    {
                        if (res.width < screenWidth && res.height < screenHeight)
                        {
                            closestWidth = res.width;
                            closestHeight = res.height;
                        }
                    }

                    // set to resolution closest to desktop, just one below desktop res
                    SetResolution(closestWidth, closestHeight, mode, 0);
                }
                else
                {
                    ApplySelectedResolution();
                }
            }
            /*
        else if (wasWindowed)
        {
            // reset to native/desktop resolution
            SetResolution(Display.main.systemWidth, Display.main.systemHeight, mode, 0);
        }
        */
            else
            {
                ApplySelectedResolution();
            }

            UpdateDialogAfterEndOfFrame();
        }

        public void OnVSyncChanged()
        {
            if (updatingDialog)
                return;

            QualitySettings.vSyncCount = vSync.value;
            PlayerPrefs.SetInt(prefsKey_VSyncCount, vSync.value);
            UpdateDialogAfterEndOfFrame();
        }

        public void OnQualityLevelChanged()
        {
            if (updatingDialog)
                return;

            var selectedText = quality.options[quality.value].text;
            QualitySettings.SetQualityLevel(new List<string>(QualitySettings.names).IndexOf(selectedText), true);
            QualitySettings.vSyncCount = vSync.value; // reset vsync setting as it may be affected by quality level

            UpdateDialogAfterEndOfFrame();
        }

        public void OnMonitorChanged()
        {
            if (updatingDialog)
                return;

            // currently does not attempt to auto-restart application, it merely removes the Play button (via Inspector events) thus forcing user to restart
            // for one, restarting would have to be coded separately for each platform
            // secondly, "ForceSingleInstance" would prevent a clean restart from within the running app (ie requires external app or batch file to control restart)
            displayNote.gameObject.SetActive(false);

            UpdateDialogAfterEndOfFrame();
        }
        #endregion

        #region Apply Changes
        void ApplySelectedResolution()
        {
            // in case resolution changed, we need to check whether the Hz selection still applies for the new resolution
            // if not we opt to go with the default '0' Hz
            var selectedRes = GetSelectedResolution();
            var availableRefreshRates = refreshRates[selectedRes];
            var selectedHz = GetSelectedRefreshRate();
            if (selectedHz.Equals("N/A") || availableRefreshRates.Contains(selectedHz) == false)
                selectedHz = "0";

            var resolution = selectedRes.Split(new char[] { 'x' });
            var width = int.Parse(resolution[0]);
            var height = int.Parse(resolution[1]);
            var hz = int.Parse(selectedHz);
            SetResolution(width, height, GetSelectedFullScreenMode(), hz);
        }

        void SetResolution(int width, int height, FullScreenMode mode, int hz)
        {
            // prevent setting resolution multiple times when dialog is updated in the next frame
            //Debug.LogError("DESIRED res: " + GetResolutionString(width, height) + " @ " + hz + " Hz in " + mode);
            Screen.SetResolution(width, height, mode, hz);
            QualitySettings.vSyncCount = vSync.value; // reset vsync setting as it may be affected by resolution
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

            //Debug.LogError("NEW Screen: " + Screen.width+ "x" + Screen.height + " (curRes: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height +
            //    ", sysRes: " + Display.main.systemWidth + "x" + Display.main.systemHeight + ") @ " + Screen.currentResolution.refreshRate + " Hz in " + Screen.fullScreenMode);
            PopulateDropdowns();
            ApplyCurrentSettingsToDialog();
            UpdateDialogInteractability();
        }
        #endregion
    }
}
