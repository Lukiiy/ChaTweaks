using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChaTweaks;

[HarmonyPatch]
public class History
{
    private static readonly List<string> sent = [];
    private static readonly List<string> saved = [];
    private static int recallIdx = -1;
    private static string? lastMsg;
    private static int currentMsgRepeat;

    private static readonly FieldInfo queue = AccessTools.Field(typeof(TextChatUi), "historyQueue"); // lol reflection minecraft mod moment
    private static readonly FieldInfo? newMsgQueue = AccessTools.Field(typeof(TextChatUi), "newMessagesQueue");

    // Save messages
    [HarmonyPatch(typeof(TextChatUi), "OnDestroy")]
    [HarmonyPrefix]
    private static void Save(TextChatUi __instance)
    {
        if (!Plugin.persistencyToggle.Value) return;

        saved.Clear();

        var queue = (Queue<TextChatMessageUi>) History.queue.GetValue(__instance);
        foreach (var msg in queue)
        {
            TMP_Text meshText = msg.GetComponentInChildren<TMP_Text>();

            if (meshText != null) saved.Add(meshText.text);
        }
    }

    // Restore saved messages
    [HarmonyPatch(typeof(TextChatUi), "Awake")]
    [HarmonyPostfix]
    private static void Restore()
    {
        if (!Plugin.persistencyToggle.Value || saved.Count == 0) return;

        foreach (string msg in saved) TextChatUi.ShowMessage(msg);

        saved.Clear();
    }

    // Save sent messages for recall
    [HarmonyPatch(typeof(TextChatManager), "UserCode_CmdSendMessageInternal__String__NetworkConnectionToClient")]
    [HarmonyPrefix]
    private static void SaveSentMsgs(string message)
    {
        if (message.Length == 0) return;

        sent.Remove(message);
        sent.Add(message);

        recallIdx = -1;
    }

    // The actual recall functionality ingame
    [HarmonyPatch(typeof(TextChatUi), "Update")]
    [HarmonyPostfix]
    private static void Recall(TextChatUi __instance)
    {
        if (!TextChatUi.IsOpen || sent.Count == 0) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || (!keyboard.upArrowKey.wasPressedThisFrame && !keyboard.downArrowKey.wasPressedThisFrame)) return;

        if (keyboard.upArrowKey.wasPressedThisFrame) recallIdx = Mathf.Clamp(recallIdx + 1, 0, sent.Count - 1); else recallIdx = Mathf.Clamp(recallIdx - 1, -1, sent.Count - 1);

        __instance.messageField.text = recallIdx == -1 ? string.Empty : sent[sent.Count - 1 - recallIdx];
        __instance.messageField.MoveToEndOfLine(false, false);
    }

    // Collapses repeated messages into a single message with a repeat count
    [HarmonyPatch(typeof(TextChatUi), "ShowMessage")]
    [HarmonyPrefix]
    private static bool ShowMessage(string message)
    {
        if (!Plugin.scamCollapseToggle.Value || string.IsNullOrEmpty(message) || !SingletonBehaviour<TextChatUi>.HasInstance)
        {
            lastMsg = null;
            currentMsgRepeat = 0;

            return true;
        }

        if (message != lastMsg)
        {
            lastMsg = message;
            currentMsgRepeat = 1;

            return true;
        }

        UpdateLastMessage($"{message} <color=#9a9a9a>(x{++currentMsgRepeat})</color>");

        return false;
    }

    // Updates the last message in the queue to the given text
    private static void UpdateLastMessage(string text)
    {
        TextChatUi ui = SingletonBehaviour<TextChatUi>.Instance;

        SetQueueTail(ui, newMsgQueue, text);
        SetQueueTail(ui, queue, text);
    }

    // Sets the tail of the queue to the given text
    private static void SetQueueTail(TextChatUi ui, FieldInfo? field, string text)
    {
        if (field?.GetValue(ui) is not Queue<TextChatMessageUi> queue || queue.Count == 0) return;

        TextChatMessageUi? last = null;
        foreach (TextChatMessageUi item in queue) last = item;

        if (last?.messageText == null) return;

        last.messageText.text = text;
        last.messageText.ForceMeshUpdate();
    }
}