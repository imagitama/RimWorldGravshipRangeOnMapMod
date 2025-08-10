using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;

namespace RimWorldGravshipRangeOnMapMod
{
    [HarmonyPatch(typeof(WorldRenderer), "DrawWorldLayers")]
    public static class WorldRendererPatch
    {
        private static readonly MethodInfo getMaxLaunchDistanceMI =
            AccessTools.Method(typeof(CompPilotConsole), "GetMaxLaunchDistance");

        public static void Postfix()
        {
            Logger.LogMessage("Postfix started");

            var engine = Find.Maps
                .SelectMany(m => m.listerThings.AllThings)
                .OfType<Building_GravEngine>()
                .FirstOrDefault(b => b.Faction == Faction.OfPlayer);

            if (engine == null)
            {
                Log.Warning("No player gravship engine found on any map");
                return;
            }

            Logger.LogMessage($"Found gravship engine '{engine.Label}' on map '{engine.Map?.Parent?.Label ?? "Unknown"}'.");

            // Find the CompPilotConsole that's linked to this engine
            var console = engine.Map.listerThings.AllThings
                .Select(t => t.TryGetComp<CompPilotConsole>())
                .FirstOrDefault(c => c != null && c.engine == engine);

            if (console == null)
            {
                Log.Warning("No pilot console linked to this engine was found");
                return;
            }

            try
            {
                object result = getMaxLaunchDistanceMI.Invoke(console, new object[] { PlanetLayer.Selected });
                int maxDistance = (int)result;
                Logger.LogMessage($"Max launch distance from GetMaxLaunchDistance: {maxDistance}");

                int radius = GravshipUtility.MaxDistForFuel(engine.TotalFuel,
                    engine.Map.Tile.Layer,
                    PlanetLayer.Selected,
                    fuelFactor: engine.FuelUseageFactor);
                Logger.LogMessage($"Fuel-limited distance: {radius}");

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
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error calling GetMaxLaunchDistance: {ex}");
            }
        }
    }
}