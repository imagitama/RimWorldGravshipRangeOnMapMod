using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;

namespace RimWorldGravshipRangeOnMapMod {
    public static class GravshipLaunchTracker
    {
        // store this to avoid lag from double render
        public static bool IsChoosingDestination = false;
    }

    [HarmonyPatch(typeof(CompPilotConsole), nameof(CompPilotConsole.StartChoosingDestination))]
    public static class Patch_StartChoosingDestination
    {
        public static void Prefix()
        {
            Logger.LogMessage("Started choosing destination");
            GravshipLaunchTracker.IsChoosingDestination = true;
        }
    }

    [HarmonyPatch(typeof(TilePicker), nameof(TilePicker.StopTargeting))]
    public static class Patch_StopTargeting
    {
        public static void Postfix()
        {
            if (GravshipLaunchTracker.IsChoosingDestination)
            {
                Logger.LogMessage("Stopped choosing destination");
                GravshipLaunchTracker.IsChoosingDestination = false;
            }
        }
    }
}