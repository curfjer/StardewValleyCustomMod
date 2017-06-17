using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using CustomFarmBuildings.CustomBlueprints;

namespace CustomFarmBuildings.Menus
{
    class DebugLogger
    {
        internal static IMonitor Logger;

        public DebugLogger()
        {
            Logger = CustomFarmBuildings.Logger;
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
            Logger.Log($"FileName: {blu.FileName}");
            Logger.Log($"Name: {blu.BuildingName}");
            Logger.Log($"Name of Building to Upgrade: {blu.NameOfBuildingToUpgrade}");

            Logger.Log($"Tile Dimensions: {blu.TilesWidth} x {blu.TilesHeight}");
            Logger.Log($"HumanDoorCoord: ({blu.HumanDoorTileCoord.X}, {blu.HumanDoorTileCoord.Y})");
            Logger.Log($"AnimalDoorCoord: ({blu.AnimalDoorTileCoord.X}, {blu.AnimalDoorTileCoord.Y})");
            Logger.Log($"Max Occupants: {blu.MaxOccupants}");

            Logger.Log($"Description: {blu.Description}");
            Logger.Log($"Magical: {blu.Magical}");
            Logger.Log($"Seasonal: {blu.Seasonal}");

            Logger.Log($"Money Required: {blu.MoneyRequired}");
            Logger.Log($"Resources Required: NEED TO DO THIS"); // TODO log resources required

            Logger.Log($"Wood Required: {blu.woodRequired}");
            Logger.Log($"Stone Required: {blu.stoneRequired}");
            Logger.Log($"Copper Required: {blu.copperRequired}");
            Logger.Log($"Iron Required: {blu.IronRequired}");
            Logger.Log($"Gold Required: {blu.GoldRequired}");
            Logger.Log($"Iridium Required: {blu.IridiumRequired}");
            
            Logger.Log($"Blueprint Type: {blu.BlueprintType}");
            Logger.Log($"Action Behavior: {blu.ActionBehavior}");
            

            // Interiors
            Logger.Log("Interiors:");
            int count = 0;
            foreach (CustomInterior interior in blu.Interiors)
            {
                Logger.Log($"Interior {count} - Name - {interior.Name}");
                Logger.Log($"FileName: {interior.FileName}");
                Logger.Log($"Type: {interior.Type}");
                Logger.Log($"Farmable: {interior.Farmable}");
                Logger.Log($"Outdoor: {interior.Outdoor}");
                Logger.Log($"Seasonal: {interior.Seasonal}");
                count++;
            }
        }

        public void DebugGame1LocationsList(List<GameLocation> locations)
        {
            Logger.Log("Locations in Game1:");
            foreach (GameLocation location in locations)
            {
                Logger.Log($"{location.name} - {location.uniqueName}");
            }
        }

        public void DebugLocation(GameLocation location)
        {
            Logger.Log($"{location.name} - map data:");
            xTile.Map map = location.map;
            Logger.Log($"Description: {map.Description}");
            Logger.Log($"Height: {map.DisplayHeight}");
            Logger.Log($"Width: {map.DisplayWidth}");
            Logger.Log($"Size: {map.DisplaySize}");
            Logger.Log($"Time: {map.ElapsedTime}");
            Logger.Log($"ID: {map.Id}");
            foreach (xTile.Layers.Layer layer in map.Layers)
            {
                //Logger.Log($"Layers: {layer.}");
            }

            Logger.Log($"Properties: {map.Properties}");

            Logger.Log($"TileSheets: {map.TileSheets.ToString()}");
        }
    }
}
