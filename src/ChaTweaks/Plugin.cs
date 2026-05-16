using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ChaTweaks;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony harmony;

    private void Awake()
    {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loaded!");

        harmony = new Harmony("me.lukiiy.ChaTweaks");
        harmony.PatchAll();
    }
}
