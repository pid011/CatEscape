using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatEscape.UI.MainMenu
{
    public class GraphicSettingsComponent : MonoBehaviour
    {
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Dropdown _resolutionDropdown;

        private readonly List<Dropdown.OptionData> _resolutions = new List<Dropdown.OptionData>
        {
            new Dropdown.OptionData("1280 x 720"),
            new Dropdown.OptionData("1600 x 900"),
            new Dropdown.OptionData("1920 x 1080"),
            new Dropdown.OptionData("2560 x 1440")
        };

        private void Awake()
        {
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);

            _fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
            _resolutionDropdown.ClearOptions();
            _resolutionDropdown.options = _resolutions;
        }

        public void OnFullScreenOptionValueChanged(bool value)
        {
            Screen.fullScreenMode = value
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;
        }

        public void OnResolutionDropdownValueChanged(int idx)
        {
            var resolutionString = _resolutions[idx].text;

            var (w, h) = ParseResolution(resolutionString);
            Screen.SetResolution(w, h, _fullscreenToggle.isOn);
        }

        private static (int, int) ParseResolution(string text)
        {
            var result = text.Split(new[] { " x " }, StringSplitOptions.None);

            return (int.Parse(result[0]), int.Parse(result[1]));
        }
    }
}
