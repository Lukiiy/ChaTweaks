using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace ChaTweaks;

[HarmonyPatch]
public static class ModSettings
{
    private static readonly List<string> boolSetting = ["On", "Off"];

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
        Component clone = UnityEngine.Object.Instantiate(template, parent);

        clone.name = $"CT_{label}";
        foreach (var localizComp in clone.GetComponentsInChildren<LocalizeStringEvent>(true)) localizComp.enabled = false; // disable localization components

        DropdownOption dropdown = clone.GetComponent<DropdownOption>();
        if (dropdown == null) return;

        dropdown.SetOptions(boolSetting);
        dropdown.Initialize(() => set(dropdown.value == 0), get() ? 0 : 1);

        foreach (var meshText in clone.GetComponentsInChildren<TMP_Text>(true))
        {
            if (meshText.GetComponentInParent<TMP_Dropdown>() != null) continue;

            meshText.text = label; // set row title
            meshText.ForceMeshUpdate();

            break;
        }

        tooltip.RegisterTooltip(clone.GetComponent<RectTransform>(), description);
    }
}