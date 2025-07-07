using UnityEngine;
using System.Collections.Generic;

public class ModularComboBuffer : MonoBehaviour
{
    [SerializeField] private int maxComboLength = 3;
    private Queue<ModularWeaponInput> inputBuffer = new();

    public void RegisterInput(ModularWeaponInput input)
    {
        inputBuffer.Enqueue(input);

        while (inputBuffer.Count > maxComboLength)
        {
            inputBuffer.Dequeue();
        }
    }

    public ModularWeaponInput[] GetCombo()
    {
        ModularWeaponInput[] combo = new ModularWeaponInput[maxComboLength];
        for (int i = 0; i < maxComboLength; i++)
        {
            combo[i] = ModularWeaponInput.None; // Default padding
        }

        ModularWeaponInput[] bufferArray = inputBuffer.ToArray();
        int offset = maxComboLength - bufferArray.Length;

        for (int i = 0; i < bufferArray.Length; i++)
        {
            combo[i + offset] = bufferArray[i]; // Pad from the back
        }

        return combo;
    }

    public void ClearBuffer()
    {
        inputBuffer.Clear();
    }
}
