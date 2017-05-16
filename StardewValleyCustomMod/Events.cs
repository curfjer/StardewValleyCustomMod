using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValleyCustomMod.CustomBlueprints;

namespace StardewValleyCustomMod
{
    [Serializable]
    public class PlayerSave
    {
        public string player;
        public CustomBuilding[] buildings;
    }

    internal static class Events
    {
        // Load Custom Buildings
        internal static void Load(object s, EventArgs e)
        {
            StardewValleyCustomMod.Logger.Log("Loading custom buildings...");

            StardewValleyCustomMod.Logger.Log($"Season: {Game1.currentSeason}");

            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSave));
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            string saveGameName = Game1.player.name + "_" + Game1.uniqueIDForThisGame;
            string fileName = Path.Combine(StardewValleyCustomMod.ModPath, saveGameName + ".xml");
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);

            PlayerSave playerBuildingsList = (PlayerSave)serializer.Deserialize(fs);

            
            //ApplyLocation();
            //ApplyTilesheet();

            BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");
            foreach (CustomBuilding building in playerBuildingsList.buildings)
            {
                StardewValleyCustomMod.Logger.Log("Loading building...");
                building.load();//needed or no?
                StardewValleyCustomMod.Logger.Log("Adding building to farm");
                farm.buildings.Add(building.ConvertCustomBuildingToBuilding());
                if (StardewValleyCustomMod.Config.Debug)
                    StardewValleyCustomMod.Logger.Log($"Loaded {building.buildingType} at {building.tileX}, {building.tileY}");
            }

            StardewValleyCustomMod.Logger.Log("Custom Buildings Loaded!");
        }

        // Save and Remove Custom Buildings
        internal static void Save(object s, EventArgs e)
        {
            PlayerSave playerBuildingsList = new PlayerSave();
            playerBuildingsList.player = Game1.player.name + "_" + Game1.uniqueIDForThisGame; // Use if just one save file for all different farms
            List<Building> playerBuildings = new List<Building>();
            List<CustomBuilding> buildings = new List<CustomBuilding>();

            string fileName = Path.Combine(StardewValleyCustomMod.ModPath, playerBuildingsList.player + ".xml");
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSave));
            TextWriter writer = new StreamWriter(fileName);

            StardewValleyCustomMod.Logger.Log("Creating save file...");

            BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");

            // Check for custom buildings on farm
            foreach (Building building in farm.buildings)
            {
                if (CustomBuildingCheck(building))
                {
                    buildings.Add(new CustomBuilding(building));
                    playerBuildings.Add(building);
                    if (StardewValleyCustomMod.Config.Debug)
                        StardewValleyCustomMod.Logger.Log($"Adding {building.buildingType} to the custom building list.");
                }
            }
            
            // Remove buildings from the games farm building list so it does not save and try to load
            foreach (Building building in playerBuildings)
            {
                if (StardewValleyCustomMod.Config.Debug)
                    StardewValleyCustomMod.Logger.Log($"Removing {building.buildingType} from the building list.");
                farm.buildings.Remove(building);
            }
            
            // Save custom buildings to file
            playerBuildingsList.buildings = buildings.ToArray();
            serializer.Serialize(writer, playerBuildingsList);
            writer.Close();
            StardewValleyCustomMod.Logger.Log($"Save File created for '{playerBuildingsList.player}'.");
        }

        public static bool CustomBuildingCheck(Building building)
        {
            if (StardewValleyCustomMod.Config.Debug)
                StardewValleyCustomMod.Logger.Log($"Checking if {building.buildingType} is a custom building.");

            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.BlueprintList)
                if (building.buildingType.Equals(blu.name))
                    return true;
            if (StardewValleyCustomMod.Config.Debug)
                StardewValleyCustomMod.Logger.Log("CustomBuildingCheck FAILED");
            return false;
        }

        private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            StardewValleyCustomMod.Logger.Log($"Unknown Node: {e.Name}, {e.Text}");
        }

        private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            StardewValleyCustomMod.Logger.Log($"Unknown Attribute: {attr.Name} - '{attr.Value}'");
        }
    }
}
