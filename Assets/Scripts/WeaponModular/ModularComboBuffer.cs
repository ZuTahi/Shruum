using System.Collections.Generic;
using UnityEngine;

public class ModularComboBuffer : MonoBehaviour
{
    private struct ComboInput
    {
        public ModularWeaponInput input;
        public float time;

        public ComboInput(ModularWeaponInput input, float time)
        {
            this.input = input;
            this.time = time;
        }
    }

    private List<ComboInput> buffer = new();
    public float inputTimeout = 1.5f; // maximum allowed time between first and last input

    public void RegisterInput(ModularWeaponInput input)
    {
        buffer.Add(new ComboInput(input, Time.time));

        if (buffer.Count > 3)
            buffer.RemoveAt(0);
    }

    public ModularWeaponInput[] GetCombo()
    {
        return buffer.ConvertAll(c => c.input).ToArray();
    }

    public bool HasValidCombo()
    {
        if (buffer.Count < 3) return false;

        float firstTime = buffer[0].time;
        float lastTime = buffer[buffer.Count - 1].time;

        return (lastTime - firstTime) <= inputTimeout;
    }

    public void ClearBuffer()
    {
        buffer.Clear();
    }
}
