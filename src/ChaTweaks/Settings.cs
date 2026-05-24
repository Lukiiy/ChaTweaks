using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ChaTweaks;

[HarmonyPatch]
public static class ModSettings
{
    private static List<string> boolSetting = ["On", "Off"];

    private static readonly (string label, string description, Func<bool> get, Action<bool> set)[] Settings = [
        ("Persistent Text Chat", "Toggle message persistency for the text chat", () => History.persistencyToggle, v => History.persistencyToggle = v),
        ("Profanity Filter", "Toggle profanity filter for the text chat", () => ProfanityFilter.toggled, v => ProfanityFilter.toggled = v)
    ];

    [HarmonyPatch(typeof(SettingsMenu), "Start")]
    [HarmonyPostfix]
    private static void Inject(SettingsMenu __instance) // injects the client toggles into the settings menu
    {
        DropdownOption template = __instance.muteChat;
        if (template == null) return;

        foreach (var (label, desc, get, set) in Settings) AddToggleThing(template, template.transform.parent, __instance.generalTooltip, label, desc, get, set);
    }

    private static void AddToggleThing(DropdownOption template, Transform parent, UiTooltip tooltip, string label, string description, Func<bool> get, Action<bool> set) // spawns a toggle setting using a given template and values
    {
        Component clone = UnityEngine.Object.Instantiate((Component) template, parent);

        SetLabel(clone, label);

        DropdownOption dropdown = clone.GetComponent<DropdownOption>();

        dropdown.SetOptions(boolSetting);
        dropdown.Initialize(() => set(dropdown.value == 0), get() ? 0 : 1);
        tooltip.RegisterTooltip(clone.GetComponent<RectTransform>(), description);
    }

    private static void SetLabel(Component root, string text)
    {
        foreach (var MeshText in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (MeshText.GetComponentInParent<TMP_Dropdown>() != null) continue;

            MeshText.text = text;
            return;
        }
    }
}