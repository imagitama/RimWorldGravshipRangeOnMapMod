using HarmonyLib;
using Verse;

namespace RimWorldGravshipRangeOnMapMod
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            Logger.LogMessage("Starting up...");

            var harmony = new Harmony("imagitama.rimworldgravshiprangeonmap");
            harmony.PatchAll();
        }
    }
}
