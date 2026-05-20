using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ChaTweaks;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony harmony = null!;

    private void Awake()
    {
        harmony = new Harmony(Info.Metadata.GUID);
        Log = Logger;

        Log.LogInfo($"Mod {Name} loaded!");
        harmony.PatchAll();
    }
}
