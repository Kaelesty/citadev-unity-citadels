using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{

    public GameObject screenSelector;
    public GameObject camera;

    private Dictionary<string, int[]> screens = new Dictionary<string, int[]>();

    public void Start()
    {
        screens["Connecting"] = new int[2] { 0, 0 };
        screens["CharacterSelecting"] = new int[2] { 2000, 0 };

        var dropdown = screenSelector.GetComponent<Dropdown>();
        dropdown.ClearOptions();
        foreach (var screen in screens.Keys)
        {
            var option = new Dropdown.OptionData();
            option.text = screen;
            dropdown.options.Add(option);
        }
    }

    public void selectorSwitchScreen()
    {
        screenSelector.SetActive(true);
        var dropdown = screenSelector.GetComponent<Dropdown>();
        var screenPosition = screens[dropdown.options[dropdown.value].text];
        camera.transform.LeanMoveLocal(new Vector3(screenPosition[0], screenPosition[1], (float)-325.4922), 2).setEaseInCubic();
    }
}
