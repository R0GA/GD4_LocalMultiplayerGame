using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Rendering;
using UnityEngine.Windows;

public class ControllerSetter : MonoBehaviour
{
    [SerializeField] private PlayerInput p1Input;
    [SerializeField] private PlayerInput p2Input;

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        ReassignControllers();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad) return;

        if (change == InputDeviceChange.Added ||
            change == InputDeviceChange.Removed ||
            change == InputDeviceChange.Disconnected ||
            change == InputDeviceChange.Reconnected)
        {
            Debug.Log($"Device change: {device.displayName} - {change}");
            ReassignControllers();
        }
    }

    private void ReassignControllers()
    {
        List<Gamepad> activeGamepads = new List<Gamepad>();

        foreach (var pad in Gamepad.all)
        {
            if (pad.added && pad.enabled)
                activeGamepads.Add(pad);
        }

        Debug.Log($"Active controllers found: {activeGamepads.Count}");

        p1Input.DeactivateInput();
        p2Input.DeactivateInput();

        p1Input.user.UnpairDevices();
        p2Input.user.UnpairDevices();

        if (activeGamepads.Count > 0)
        {
            InputUser.PerformPairingWithDevice(activeGamepads[0], p1Input.user);
            p1Input.SwitchCurrentControlScheme("Gamepad", activeGamepads[0]);
            p1Input.ActivateInput();
            Debug.Log($"Assigned {activeGamepads[0].displayName} to Player 1");
        }

        if (activeGamepads.Count > 1)
        {
            InputUser.PerformPairingWithDevice(activeGamepads[1], p2Input.user);
            p2Input.SwitchCurrentControlScheme("Gamepad", activeGamepads[1]);
            p2Input.ActivateInput();
            Debug.Log($"Assigned {activeGamepads[1].displayName} to Player 2");
        }
    }
}