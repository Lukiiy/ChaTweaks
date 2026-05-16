using HarmonyLib;

namespace ChaTweaks;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.FilterProfanity))]
internal static class DisableProfanityFilter
{
    static bool Prefix(string verifyString, ref string filteredString, ref bool __result)
    {
        filteredString = verifyString;
        __result = false;

        return false;
    }
}