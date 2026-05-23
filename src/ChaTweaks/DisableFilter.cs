using HarmonyLib;

namespace ChaTweaks;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.FilterProfanity))]
public class DisableProfanityFilter
{
    public static bool toggled = true;

    static bool Prefix(string verifyString, ref string filteredString, ref bool __result)
    {
        if (!toggled) return true; // original

        filteredString = verifyString;
        __result = false;

        return false;
    }
}