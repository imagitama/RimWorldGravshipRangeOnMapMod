using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;
using RimWorldGravshipRangeOnMapMod;

namespace RimWorldGravshipRangeOnMapMod
{
    [HarmonyPatch(typeof(WorldRenderer), "DrawWorldLayers")]
    public static class WorldRendererPatch
    {
        private static readonly MethodInfo getMaxLaunchDistanceMI =
            AccessTools.Method(typeof(CompPilotConsole), "GetMaxLaunchDistance");

        private static bool? WasGravshipFound;
        private static bool? WasConsoleFound;
        private static float? lastRadius;
        private static float? lastMaxDistance;

        public static void Postfix()
        {
            // if the backdrop of a space map
            if (WorldRendererUtility.WorldBackgroundNow)
                return;

            // avoid lag from double render
            if (GravshipLaunchTracker.IsChoosingDestination)
                return;

            var engine = Find.Maps
                .SelectMany(m => m.listerThings.AllThings)
                .OfType<Building_GravEngine>()
                .FirstOrDefault(b => b.Faction == Faction.OfPlayer);

            if (engine == null)
            {
                if (WasGravshipFound == null || WasGravshipFound == true)
                {
                    WasGravshipFound = false;
                    Logger.LogMessage("No player gravship engine found on any map");
                }
                return;
            }

            if (WasGravshipFound == null || WasGravshipFound == false)
            {
                WasGravshipFound = true;
                Logger.LogMessage($"Found gravship engine '{engine.Label}' on map '{engine.Map?.Parent?.Label ?? "Unknown"}'");
            }

            CompPilotConsole console = engine.Map.listerThings.AllThings
                .Select(t => t.TryGetComp<CompPilotConsole>())
                .FirstOrDefault(c => c != null && c.engine == engine);

            if (console == null)
            {
                if (WasConsoleFound == null || WasConsoleFound == true)
                {
                    WasConsoleFound = false;
                    Logger.LogMessage("No pilot console linked to this engine was found");
                }
                return;
            }


            if (WasGravshipFound == null || WasGravshipFound == false)
            {
                WasConsoleFound = true;
                Logger.LogMessage($"Found console '{console.ToString()}'");
            }

            try
            {
                object result = getMaxLaunchDistanceMI.Invoke(console, new object[] { PlanetLayer.Selected });
                int maxDistance = (int)result;

                int radius = GravshipUtility.MaxDistForFuel(engine.TotalFuel,
                    engine.Map.Tile.Layer,
                    PlanetLayer.Selected,
                    fuelFactor: engine.FuelUseageFactor);


                PlanetTile cachedClosestLayerTile = PlanetTile.Invalid;
                PlanetTile curTile = engine.Map.Tile;
                PlanetTile planetTile = curTile;

                // if in "space" view
                if (curTile.Layer != Find.WorldSelector.SelectedLayer)
                {
                    if (cachedClosestLayerTile.Layer != Find.WorldSelector.SelectedLayer || !cachedClosestLayerTile.Valid)
                        cachedClosestLayerTile = Find.WorldSelector.SelectedLayer.GetClosestTile(curTile);
                    planetTile = cachedClosestLayerTile;
                }

                GenDraw.DrawWorldRadiusRing(planetTile, maxDistance, CompPilotConsole.GetThrusterRadiusMat(planetTile));

                if (radius < maxDistance)
                {
                    GenDraw.DrawWorldRadiusRing(planetTile, radius, CompPilotConsole.GetFuelRadiusMat(planetTile));
                }

                if (lastMaxDistance != maxDistance)
                {
                    Logger.LogMessage($"Max launch distance: {maxDistance}");
                }

                if (lastRadius != radius)
                {
                    Logger.LogMessage($"Fuel-limited distance: {radius}");
                }

                lastMaxDistance = maxDistance;
                lastRadius = radius;
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error: {ex}");
            }
        }
    }
}