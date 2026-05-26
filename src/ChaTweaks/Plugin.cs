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

    private void Awake()
    {
        harmony = new Harmony(Info.Metadata.GUID);
        Log = Logger;

        persistencyToggle = Config.Bind("ChaTweaks", "Persistent Text Chat", true, "Toggle message persistency for the text chat");
        profanityFilterToggle = Config.Bind("ChaTweaks", "Profanity Filter", true, "Toggle profanity filter for the text chat");

        Log.LogInfo($"Mod {Name} loaded!");
        harmony.PatchAll();
    }
}
