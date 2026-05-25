using HarmonyLib;
using UnityEngine;

namespace ChaTweaks;

[HarmonyPatch]
internal static class UIPatches
{
    // Forces the chat to render while the scoreboard hides it
    [HarmonyPatch(typeof(UiVisibilityController), "OnUiVisibilityModeChanged")]
    [HarmonyPostfix]
    private static void UIVisibilityChange(UiVisibilityController __instance)
    {
        if (__instance == null || !IsChatController(__instance) || (GameManager.HiddenUiGroups & UiHidingGroup.ScoreboardOpen) == 0) return;

        ForceRender(__instance);
    }

    // Keeps chat rendered above other UI when opened
    [HarmonyPatch(typeof(TextChatUi), "SetEnabledInternal")]
    [HarmonyPostfix]
    private static void Enabled(TextChatUi __instance, bool enabled)
    {
        if (!enabled || __instance == null) return;

        BringToTop(__instance.transform);
    }

    // Chat draw priority whenever the scoreboard visibility changes
    [HarmonyPatch(typeof(Scoreboard), "UpdateVisibility")]
    [HarmonyPostfix]
    private static void BoardVisibilityChange()
    {
        TextChatUi chat = SingletonBehaviour<TextChatUi>.HasInstance ? SingletonBehaviour<TextChatUi>.Instance : UnityEngine.Object.FindFirstObjectByType<TextChatUi>();

        if (chat != null) BringToTop(chat.transform);
    }

    // Checks whether this is TextChatUI's visibility controller
    private static bool IsChatController(UiVisibilityController controller) => controller.GetComponent<TextChatUi>() != null || controller.GetComponentInParent<TextChatUi>() != null || controller.GetComponentInChildren<TextChatUi>(true) != null;

    private static void ForceRender(UiVisibilityController controller)
    {
        if (controller.TryGetComponent(out Canvas canvas)) canvas.enabled = true;

        CanvasGroup canvasGroup = controller.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private static void BringToTop(Transform root)
    {
        root.SetAsLastSibling();

        foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 20;
        }
    }
}