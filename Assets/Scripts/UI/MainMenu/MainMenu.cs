using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    Button startButton;

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        startButton = uiDoc.rootVisualElement.Q<Button>("startbutton");
        startButton.RegisterCallback<ClickEvent>(ClickMessage);
    }

    void ClickMessage(ClickEvent e)
    {
        WorldGeneratorEngine.Instance.CreateNewWorld("WorldName");
        startButton.SetEnabled(false);
        startButton.style.visibility = Visibility.Hidden;
    }
}
