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

            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSave));
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            string saveGameName = Game1.player.name + "_" + Game1.uniqueIDForThisGame;
            string fileName = Path.Combine(StardewValleyCustomMod.ModPath, saveGameName + ".xml");
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);

            PlayerSave playerBuildingsList = (PlayerSave)serializer.Deserialize(fs);

            ApplyLocation();
            ApplyTilesheet();

            BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");
            foreach (CustomBuilding building in playerBuildingsList.buildings)
            {
                StardewValleyCustomMod.Logger.Log("Adding building to farm");
                farm.buildings.Add(building.ConvertCustomBuildingToBuilding());
                StardewValleyCustomMod.Logger.Log("Loading building...");
                building.load();//needed or no?
                if (StardewValleyCustomMod.Config.debug)
                    StardewValleyCustomMod.Logger.Log($"Loaded {building.buildingType} at {building.tileX}, {building.tileY}");
            }

            StardewValleyCustomMod.Logger.Log("Custom Buildings Loaded!");
        }

        // Save and Remove Custom Buildings
        internal static void Save(object s, EventArgs e)
        {
            PlayerSave playerBuildingsList = new PlayerSave();
            playerBuildingsList.player = Game1.player.name + "_" + Game1.uniqueIDForThisGame; // Use if just one save file for all different farms
            //playerBuildingsList.buildings = new List<Building>();
            List<Building> playerBuildings = new List<Building>();
            List<CustomBuilding> buildings = new List<CustomBuilding>();

            string fileName = Path.Combine(StardewValleyCustomMod.ModPath, playerBuildingsList.player + ".xml");
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSave));
            TextWriter writer = new StreamWriter(fileName);
            //Dictionary<string, List<Building>> playerBuildingsList = new Dictionary<string, List<Building>>();

            StardewValleyCustomMod.Logger.Log("Creating save file...");

            BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");
            foreach (Building building in farm.buildings)
            {
                if (CustomBuildingCheck(building))
                {
                    // Get building coordinates
                    //playerBuildingsList.buildings.Add(building);
                    buildings.Add(new CustomBuilding(building));
                    playerBuildings.Add(building);
                    if (StardewValleyCustomMod.Config.debug)
                        StardewValleyCustomMod.Logger.Log($"Adding {building.buildingType} to the custom building list.");
                    //StardewValleyCustomMod.Logger.Log("Removing building from farm");
                    //farm.buildings.Remove(building);//might need to get building by coordinates and pass it that
                    // Save interior state
                    // Remove building
                }
            }
            
            // Remove buildings from the games farm building list so it does not save and try to load
            foreach (Building building in playerBuildings)
            {
                if (StardewValleyCustomMod.Config.debug)
                    StardewValleyCustomMod.Logger.Log($"Removing {building.buildingType} from the building list.");
                farm.buildings.Remove(building);
            }

            //playerBuildingsList.Add("Test",buildings);
            playerBuildingsList.buildings = buildings.ToArray();
            serializer.Serialize(writer, playerBuildingsList);
            writer.Close();
            StardewValleyCustomMod.Logger.Log($"Save File created for '{playerBuildingsList.player}'.");
        }

        public static bool CustomBuildingCheck(Building building)
        {
            if (StardewValleyCustomMod.Config.debug)
                StardewValleyCustomMod.Logger.Log($"Checking if {building.buildingType} is a custom building.");

            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.blueprintList)
                if (building.buildingType.Equals(blu.name))
                    return true;
            if (StardewValleyCustomMod.Config.debug)
                StardewValleyCustomMod.Logger.Log("CustomBuildingCheck FAILED");
            return false;
        }

        public static void ApplyLocation()
        {

            //AdvancedLocationLoaderMod.Logger.Log(location.ToString(), LogLevel.Trace);
            string wineryPath = Path.Combine(StardewValleyCustomMod.ModPath, "CustomBuildings");
            wineryPath = Path.Combine(wineryPath, "BuildingInterior");
            wineryPath = Path.Combine(wineryPath, "WineryInterior");
            StardewValleyCustomMod.Logger.Log("Winery File Path: " + wineryPath);
            try
            {
                GameLocation loc;
                StardewValleyCustomMod.ContentRegistry.RegisterXnb(wineryPath, wineryPath);
                xTile.Map map = Game1.content.Load<xTile.Map>(wineryPath);
                /*switch (location.Type)
                {
                    case "Cellar":
                        loc = new StardewValley.Locations.Cellar(map, location.MapName);
                        loc.objects = new SerializableDictionary<Microsoft.Xna.Framework.Vector2, StardewValley.Object>();
                        break;
                    case "BathHousePool":
                        loc = new StardewValley.Locations.BathHousePool(map, location.MapName);
                        break;
                    case "Decoratable":
                        loc = new Locations.DecoratableLocation(map, location.MapName);
                        break;
                    case "Desert":
                        loc = new Locations.Desert(map, location.MapName);
                        break;
                    case "Greenhouse":
                        loc = new Locations.Greenhouse(map, location.MapName);
                        break;
                    case "Sewer":
                        loc = new Locations.Sewer(map, location.MapName);
                        break;
                    default:
                        loc = new GameLocation(map, location.MapName);
                        break;
                }*/
                //loc.isOutdoors = location.Outdoor;
                //loc.isFarm = location.Farmable;
                loc = new StardewValley.Locations.Cellar(map, "WineryInterior");
                loc.objects = new SerializableDictionary<Microsoft.Xna.Framework.Vector2, StardewValley.Object>();
                loc.isOutdoors = false;
                loc.isFarm = false;
                Game1.locations.Add(loc);
                StardewValleyCustomMod.Logger.Log("Adding Winery Tilesheet...");
                /*string fakepath = Path.Combine(Path.GetDirectoryName(tilesheet.FileName), "all_sheet_paths_objects", tilesheet.SheetId, Path.GetFileName(tilesheet.FileName));
                if (tilesheet.Seasonal)
                    fakepath = fakepath.Replace("all_sheet_paths_objects", Path.Combine("all_sheet_paths_objects", Game1.currentSeason));
                stage++; // 3*/
                
                //Game1.addNewFarmBuildingMaps();
            }
            catch (Exception err)
            {
                StardewValleyCustomMod.Logger.ExitGameImmediately("Unable to add custom location, a unexpected error occured: " + "Winery" + err);
            }
        }

        public static void ApplyTilesheet()
        {
            try
            {
                string fakepath = StardewValleyCustomMod.ModPath;
                StardewValleyCustomMod.ContentRegistry.RegisterXnb(fakepath, "Winery");
                StardewValleyCustomMod.Logger.Log("Winery Tilesheet Added!");
            }
            catch (Exception err)
            {
                StardewValleyCustomMod.Logger.ExitGameImmediately("Unable to add custom tilesheet, a unexpected error occured: " + "Winery" + err);
            }
            
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
