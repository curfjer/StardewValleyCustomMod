using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace StardewValleyCustomMod.Menus
{
    class DebugLogger
    {
        internal static IMonitor Logger;

        public DebugLogger()
        {
            Logger = StardewValleyCustomMod.Logger;
        }

        public void DebugBlueprintDetails(BluePrint blueprint, GameLocation blueprintIndoorLocation)
        {
            Logger.Log($"Blueprint details for '{blueprint.name}'");
            Logger.Log($"Name: {blueprint.name}");
            Logger.Log($"mapToWarpTo: {blueprint.mapToWarpTo}");
            Logger.Log($"X: {(int)Vector2.Zero.X}");
            Logger.Log($"Y: {(int)Vector2.Zero.Y}");
            Logger.Log($"tilesWidth: {blueprint.tilesWidth}");
            Logger.Log($"tilesHeight: {blueprint.tilesHeight}");
            Logger.Log($"humanDoor: {blueprint.humanDoor}");
            Logger.Log($"animalDoor: {blueprint.animalDoor}");
            Logger.Log($"blueprintIndoorLocation: {blueprintIndoorLocation}");
            Logger.Log($"texture: {blueprint.texture}");
            Logger.Log($"magical: {blueprint.magical}");
        }
    }
}
