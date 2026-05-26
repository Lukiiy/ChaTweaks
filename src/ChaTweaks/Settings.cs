using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using BepInEx.Configuration;

namespace ChaTweaks;

[HarmonyPatch]
public static class IngameSettings
{
    private static readonly List<string> boolSetting = ["On", "Off"];
    private static readonly ConfigEntry<bool>[] Toggleables = [Plugin.persistencyToggle, Plugin.profanityFilterToggle, Plugin.scamCollapseToggle];

    [HarmonyPatch(typeof(SettingsMenu), "Start")]
    [HarmonyPostfix]
    private static void Inject(SettingsMenu __instance) // injects the client toggles into the settings menu
    {
        DropdownOption template = __instance.muteChat;
        if (template == null) return;

        foreach (var setting in Toggleables) AddToggleThing(template, template.transform.parent, __instance.generalTooltip, setting);
    }

    private static void AddToggleThing(DropdownOption template, Transform parent, UiTooltip tooltip, ConfigEntry<bool> setting)
    {
        Component clone = UnityEngine.Object.Instantiate(template, parent);

        clone.name = $"CT_{setting.Definition.Key}";
        foreach (var localizComp in clone.GetComponentsInChildren<LocalizeStringEvent>(true)) localizComp.enabled = false; // disable localization components

        DropdownOption dropdown = clone.GetComponent<DropdownOption>();
        if (dropdown == null) return;

        dropdown.SetOptions(boolSetting);
        dropdown.Initialize(() => setting.Value = dropdown.value == 0, setting.Value ? 0 : 1);

        foreach (TMP_Text meshText in clone.GetComponentsInChildren<TMP_Text>(true))
        {
            if (meshText.GetComponentInParent<TMP_Dropdown>() != null) continue;

            meshText.text = setting.Definition.Key;
            meshText.ForceMeshUpdate();

            break;
        }

        tooltip.RegisterTooltip(clone.GetComponent<RectTransform>(), setting.Description.Description);
    }
}