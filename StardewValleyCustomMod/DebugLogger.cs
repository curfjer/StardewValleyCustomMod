using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValleyCustomMod.CustomBlueprints;

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

        public void DebugCustomBlueprintValues(CustomBuildingBlueprint blu)
        {
            StardewValleyCustomMod.Logger.Log($"Name: {blu.name}");
            StardewValleyCustomMod.Logger.Log($"Wood Required: {blu.woodRequired}");
            StardewValleyCustomMod.Logger.Log($"Stone Required: {blu.stoneRequired}");
            StardewValleyCustomMod.Logger.Log($"Copper Required: {blu.copperRequired}");
            StardewValleyCustomMod.Logger.Log($"Iron Required: {blu.IronRequired}");
            StardewValleyCustomMod.Logger.Log($"Gold Required: {blu.GoldRequired}");
            StardewValleyCustomMod.Logger.Log($"Iridium Required: {blu.IridiumRequired}");
            StardewValleyCustomMod.Logger.Log($"Tiles Width: {blu.tilesWidth}");
            StardewValleyCustomMod.Logger.Log($"Tiles Height: {blu.tilesHeight}");
            StardewValleyCustomMod.Logger.Log($"Max Occupants: {blu.maxOccupants}");
            StardewValleyCustomMod.Logger.Log($"Money Required: {blu.moneyRequired}");
            StardewValleyCustomMod.Logger.Log($"Map To Warp To: {blu.mapToWarpTo}");
            StardewValleyCustomMod.Logger.Log($"Description: {blu.description}");
            StardewValleyCustomMod.Logger.Log($"Blueprint Type: {blu.blueprintType}");
            StardewValleyCustomMod.Logger.Log($"Name of Building to Upgrade: {blu.nameOfBuildingToUpgrade}");
            StardewValleyCustomMod.Logger.Log($"Action Behavior: {blu.actionBehavior}");
            StardewValleyCustomMod.Logger.Log($"Magical: {blu.magical}");
        }
    }
}
