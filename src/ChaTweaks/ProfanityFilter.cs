using HarmonyLib;

namespace ChaTweaks;

[HarmonyPatch]
public class ProfanityFilter
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.FilterProfanity))]
    [HarmonyPrefix]
    internal static bool Prefix(string verifyString, ref string filteredString, ref bool __result)
    {
        if (!Plugin.profanityFilterToggle.Value) return true; // original

        filteredString = verifyString;
        __result = false;

        return false;
    }
}