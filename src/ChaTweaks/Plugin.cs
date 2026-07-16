using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;

namespace ChaTweaks;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony harmony = null!;

    internal static ConfigEntry<bool> persistencyToggle = null!;
    internal static ConfigEntry<bool> profanityFilterToggle = null!;
    internal static ConfigEntry<bool> scamCollapseToggle = null!;

    private void Awake()
    {
        harmony = new Harmony(Info.Metadata.GUID);
        Log = Logger;

        persistencyToggle = Config.Bind("ChaTweaks", "Persistent Chat", true, "Allow text chat messages to persist");
        profanityFilterToggle = Config.Bind("ChaTweaks", "Profanity Filter", true, "Toggle the text profanity filter (Other players need this off to SEND messages without the profanity filter)");
        scamCollapseToggle = Config.Bind("ChaTweaks", "Spam Collapse", true, "Repeated messages from the same sender will be collapsed");

        Log.LogInfo($"Mod {Name} loaded!");
        harmony.PatchAll();
    }
}
