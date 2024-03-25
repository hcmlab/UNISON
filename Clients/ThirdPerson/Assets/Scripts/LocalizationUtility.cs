using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;

public static class LocalizationUtility
{
    private const string DefaultStringTable = "String Table Collection";

    /// <summary>
    ///     This dictionary supports the abbreviation of most used keys. There are problems with non-US keyboard layouts.
    ///     https://answers.unity.com/questions/1365520/is-it-possible-to-have-hotkeys-for-characters-like.html
    /// </summary>
    private static readonly Dictionary<KeyCode, string> CleanKeyCodes = new Dictionary<KeyCode, string>
    {
        { KeyCode.UpArrow, "▲" },
        { KeyCode.DownArrow, "▼" },
        { KeyCode.LeftArrow, "◄" },
        { KeyCode.RightArrow, "►" },
        { KeyCode.Alpha0, "0" },
        { KeyCode.Alpha1, "1" },
        { KeyCode.Alpha2, "2" },
        { KeyCode.Alpha3, "3" },
        { KeyCode.Alpha4, "4" },
        { KeyCode.Alpha5, "5" },
        { KeyCode.Alpha6, "6" },
        { KeyCode.Alpha7, "7" },
        { KeyCode.Alpha8, "8" },
        { KeyCode.Alpha9, "9" },
        { KeyCode.Backslash, "\\" },
        { KeyCode.Slash, "/" },
        { KeyCode.Comma, "," },
        { KeyCode.Colon, ":" },
        { KeyCode.Keypad0, "Keypad 0" },
        { KeyCode.Keypad1, "Keypad 1" },
        { KeyCode.Keypad2, "Keypad 2" },
        { KeyCode.Keypad3, "Keypad 3" },
        { KeyCode.Keypad4, "Keypad 4" },
        { KeyCode.Keypad5, "Keypad 5" },
        { KeyCode.Keypad6, "Keypad 6" },
        { KeyCode.Keypad7, "Keypad 7" },
        { KeyCode.Keypad8, "Keypad 8" },
        { KeyCode.Keypad9, "Keypad 9" },
        { KeyCode.Semicolon, ";" },
        { KeyCode.Escape, "Esc" },
        { KeyCode.Tab, "Tab" },
        { KeyCode.CapsLock, "CapsLock" },
        { KeyCode.AltGr, "Alt Gr" },
        { KeyCode.LeftBracket, "[" },
        { KeyCode.RightBracket, "]" },
        { KeyCode.LeftParen, "(" },
        { KeyCode.RightParen, ")" },
        { KeyCode.KeypadDivide, "Num /" },
        { KeyCode.KeypadMinus, "Num -" },
        { KeyCode.KeypadMultiply, "Num *" },
        { KeyCode.KeypadPlus, "Num +" },
        { KeyCode.KeypadPeriod, "Num ." }
    };

    private static readonly Dictionary<KeyCode, string> KeyCodesWithTranslations = new Dictionary<KeyCode, string>
    {
        { KeyCode.LeftShift, "KeyLeftShift" },
        { KeyCode.RightShift, "KeyRightShift" },
        { KeyCode.LeftControl, "KeyLeftCtrl" },
        { KeyCode.RightControl, "KeyRightCtrl" },
        { KeyCode.LeftAlt, "KeyLeftAlt" },
        { KeyCode.RightAlt, "KeyRightAlt" },
        { KeyCode.PageUp, "KeyPageUp" },
        { KeyCode.PageDown, "KeyPageDown" },
        { KeyCode.LeftWindows, "KeyLeftWindows" },
        { KeyCode.RightWindows, "KeyRightWindows" },
        { KeyCode.LeftCommand, "KeyLeftCommand" },
        { KeyCode.RightCommand, "KeyRightCommand" },
        { KeyCode.Return, "KeyReturn" },
        { KeyCode.Numlock, "KeyNumlock" },
        { KeyCode.Backspace, "KeyBackspace" },
        { KeyCode.KeypadEnter, "KeyKeypadEnter" }
    };

    public static string GetLocalizedString(params string[] stringKeys)
    {
        if (stringKeys.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stringKeys), stringKeys, "At least one string key!");
        }

        var localizedStrings = stringKeys.Select(key =>
            LocalizationSettings.StringDatabase.GetLocalizedString(DefaultStringTable, key));
        return string.Join(" ", localizedStrings);
    }

    public static string GetLocalizedString(string stringKey, string[] arguments)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(DefaultStringTable, stringKey, null,
            FallbackBehavior.UseProjectSettings, arguments.Cast<object>().ToArray());
    }

    public static string GetLocalizedString(string stringKey, int[] arguments)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(DefaultStringTable, stringKey, null,
            FallbackBehavior.UseProjectSettings, arguments.Cast<object>().ToArray());
    }

    public static string GetLocalizedString(string stringKey, float[] arguments)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(DefaultStringTable, stringKey, null,
            FallbackBehavior.UseProjectSettings, arguments.Cast<object>().ToArray());
    }

    public static string GetLocalizedString(string stringKey, double[] arguments)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(DefaultStringTable, stringKey, null,
            FallbackBehavior.UseProjectSettings, arguments.Cast<object>().ToArray());
    }

    public static string AddLocalizedKeyCode(KeyCode keyCode, string message)
    {
        return $"[{GetTextForKeyCode(keyCode)}] {message}";
    }

    public static string GetTextForKeyCode(KeyCode keyCode)
    {
        if (keyCode == KeyCode.None)
        {
            return "";
        }

        if (KeyCodesWithTranslations.TryGetValue(keyCode, out var translatedKeyCode))
        {
            return GetLocalizedString(translatedKeyCode);
        }

        if (CleanKeyCodes.TryGetValue(keyCode, out var cleanKeyCode))
        {
            return cleanKeyCode;
        }

        return keyCode.ToString();
    }
}