using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static event Action OnMainMenuOpened;

    public static event Action<bool> OnCutsceneRunning;

    public static event Action<bool> OnPauseMenuIsOpen;

    public static event Action OnItemUsed;

    public static void MainMenuOpened() => OnMainMenuOpened?.Invoke();

    public static void CutsceneRunning(bool isRunning) => OnCutsceneRunning?.Invoke(isRunning);

    public static void PauseMenuIsOpen(bool isOpen) => OnPauseMenuIsOpen?.Invoke(isOpen);

    public static void ItemUsed() => OnItemUsed?.Invoke();
}
