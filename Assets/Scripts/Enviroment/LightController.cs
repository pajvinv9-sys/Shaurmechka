using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [field: SerializeField] public List<Light> Lights = new();
    public bool IsActive => Lights.Count > 0 && Lights[0].enabled;
    [field:SerializeField] public TextMeshPro ActivityScreen;

    public void On()
    {
        if(ActivityScreen != null) ActivityScreen.text = "вкл";
        foreach (Light light in Lights)
        {
            light.enabled = true;
        }
    }

    public void Off()
    {
        if (ActivityScreen != null) ActivityScreen.text = "выкл";
        foreach (Light light in Lights)
        {
            light.enabled = false;
        }
    }

    public void Switch()
    {
        foreach (Light light in Lights)
        {
            light.enabled = !light.enabled;
        }
        if (ActivityScreen != null)
        {
            if (IsActive) ActivityScreen.text = "вкл";
            else ActivityScreen.text = "выкл";
        }
    }
}
