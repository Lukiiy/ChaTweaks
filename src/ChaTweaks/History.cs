using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChaTweaks;

[HarmonyPatch]
public class History
{
    public static bool persistencyToggle = true;
    public static bool recallToggle = true;

    private static readonly List<string> sent = [];
    private static readonly List<string> saved = [];
    private static int recallIdx = -1;

    private static readonly FieldInfo queue = AccessTools.Field(typeof(TextChatUi), "historyQueue"); // lol reflection minecraft mod moment

    // Save messages
    [HarmonyPatch(typeof(TextChatUi), "OnDestroy")]
    [HarmonyPrefix]
    private static void SaveMessages_Prefix(TextChatUi __instance)
    {
        if (!persistencyToggle) return;

        saved.Clear();

        var queue = (Queue<TextChatMessageUi>)History.queue.GetValue(__instance);
        foreach (var msg in queue)
        {
            var tmp = msg.GetComponentInChildren<TMP_Text>();
            if (tmp != null) saved.Add(tmp.text);
        }
    }

    // Restore saved messages
    [HarmonyPatch(typeof(TextChatUi), "Awake")]
    [HarmonyPostfix]
    private static void RestoreMessages_Postfix()
    {
        if (!persistencyToggle || saved.Count == 0) return;

        foreach (var msg in saved) TextChatUi.ShowMessage(msg);

        saved.Clear();
    }

    // Save sent messages for recall
    [HarmonyPatch(typeof(TextChatManager), "UserCode_CmdSendMessageInternal__String__NetworkConnectionToClient")]
    [HarmonyPrefix]
    private static void RecordSentMessage_Prefix(string message)
    {
        if (!recallToggle || message.Length == 0) return;

        sent.Remove(message);
        sent.Add(message);

        recallIdx = -1;
    }

    // The actual recall functionality ingame
    [HarmonyPatch(typeof(TextChatUi), "Update")]
    [HarmonyPostfix]
    private static void ArrowRecall_Postfix(TextChatUi __instance)
    {
        if (!recallToggle || !TextChatUi.IsOpen || sent.Count == 0) return;
        var keyboard = Keyboard.current;

        if (keyboard == null || (!keyboard.upArrowKey.wasPressedThisFrame && !keyboard.downArrowKey.wasPressedThisFrame)) return;

        if (keyboard.upArrowKey.wasPressedThisFrame) recallIdx = Mathf.Clamp(recallIdx + 1, 0, sent.Count - 1); else recallIdx = Mathf.Clamp(recallIdx - 1, -1, sent.Count - 1);

        __instance.messageField.text = recallIdx == -1 ? string.Empty : sent[sent.Count - 1 - recallIdx];
        __instance.messageField.MoveToEndOfLine(false, false);
    }
}