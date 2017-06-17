using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Entoarox.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using CustomFarmBuildings.CustomBlueprints;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using CustomFarmBuildings.CustomBuildings;

namespace CustomFarmBuildings
{
    [Serializable]
    public class PlayerSave
    {
        public CustomBuilding[] buildings;
        public AnimalBuilding[] animalBuildings;
        public HarvesterBuilding[] harvesterBuildings;
    }

    internal static class Events
    {
        // Load Custom Buildings
        // TODO Need to create Save folder if dne
        internal static void Load(object s, EventArgs e)
        {
            CustomFarmBuildings.Logger.Log("Loading custom buildings...");

            CustomFarmBuildings.Logger.Log($"Season: {Game1.currentSeason}");

            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSave));
            // TODO check these unknowns and understand them
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            string saveGameName = Game1.player.name + "_" + Game1.uniqueIDForThisGame;
            string fileName = Path.Combine(CustomFarmBuildings.ModPath, "Saves", saveGameName + ".xml");
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            int count = 0;
            /*
            // If save exists load save
            if (fs.Length > 0)
            {
                PlayerSave playerBuildingsList = (PlayerSave)serializer.Deserialize(fs);;

                BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");
                foreach (CustomBuilding building in playerBuildingsList.buildings)
                {
                    building.load();//needed or no?
                    farm.buildings.Add((Building)building);
                    CustomFarmBuildings.FarmBuildings.CustomBuildings.Add(building);
                    if (CustomFarmBuildings.Config.Debug)
                        CustomFarmBuildings.Logger.Log($"Loaded {building.buildingName} at {building.tileX}, {building.tileY}");
                    count++;
                }
                foreach (AnimalBuilding building in playerBuildingsList.animalBuildings)
                {
                    building.load();//needed or no?
                    farm.buildings.Add((Building)building);
                    CustomFarmBuildings.FarmBuildings.AnimalBuildings.Add(building);
                    if (CustomFarmBuildings.Config.Debug)
                        CustomFarmBuildings.Logger.Log($"Loaded {building.buildingName} at {building.tileX}, {building.tileY}");
                    count++;
                }
                foreach (HarvesterBuilding building in playerBuildingsList.harvesterBuildings)
                {
                    building.load();//needed or no?
                    farm.buildings.Add((Building)building);
                    CustomFarmBuildings.FarmBuildings.HarvesterBuildings.Add(building);
                    if (CustomFarmBuildings.Config.Debug)
                        CustomFarmBuildings.Logger.Log($"Loaded {building.buildingName} at {building.tileX}, {building.tileY}");
                    count++;
                }

                CustomFarmBuildings.Logger.Log($"{count} of {playerBuildingsList.buildings.Length} Custom Buildings Loaded!");
            }
            // Save does not exist, load nothing
            else
                CustomFarmBuildings.Logger.Log($"No custom buildings found for {saveGameName}.");
                */

            BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");
            List<Building> buildings = farm.buildings;
            foreach (Building building in buildings)
            {
                if (building.buildingType.StartsWith("MOD"))
                {
                    String[] modBuildingInfo = building.buildingType.Split('_');
                    CustomBuildingBlueprint blu = CustomFarmBuildings.Config.GetCustomBuildingBlueprint(modBuildingInfo[1], modBuildingInfo[2]);
                    if (blu != null)
                    {
                        if (blu.BlueprintType.Equals("Building"))
                        {
                            CustomBuilding customBuilding = new CustomBuilding(blu, new Vector2(building.tileX, building.tileY));
                            customBuilding.ConvertBuildingToCustomBuilding(building);
                            customBuilding.load();
                            farm.buildings.Remove(building);
                            farm.buildings.Add(customBuilding);
                        }
                        else if (blu.BlueprintType.Equals("Animal"))
                        {
                            AnimalBuilding animalBuilding = new AnimalBuilding(blu, new Vector2(building.tileX, building.tileY));
                            animalBuilding.ConvertBuildingToCustomBuilding(building);
                            animalBuilding.load();
                            farm.buildings.Remove(building);
                            farm.buildings.Add(animalBuilding);
                        }
                        else if (blu.BlueprintType.Equals("Harvester"))
                        {
                            HarvesterBuilding harvesterBuilding = new HarvesterBuilding(blu, new Vector2(building.tileX, building.tileY));
                            harvesterBuilding.ConvertBuildingToCustomBuilding(building);
                            harvesterBuilding.load();
                            farm.buildings.Remove(building);
                            farm.buildings.Add(harvesterBuilding);
                        }
                    }
                }
            }
        }

        // Save and Remove Custom Buildings
        internal static void Save(object s, EventArgs e)
        {
            PlayerSave playerBuildingsList = new PlayerSave();
            string fileName = Game1.player.name + "_" + Game1.uniqueIDForThisGame;
            List<Building> playerBuildings = new List<Building>();
            List<CustomBuilding> customBuildings = new List<CustomBuilding>();
            List<AnimalBuilding> animalBuildings = new List<AnimalBuilding>();
            List<HarvesterBuilding> harvesterBuildings = new List<HarvesterBuilding>();

            string filePath = Path.Combine(CustomFarmBuildings.ModPath, "Saves", fileName + ".xml");
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSave));
            TextWriter writer = new StreamWriter(filePath);

            CustomFarmBuildings.Logger.Log("Creating save file...");

            BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");

            // Check for custom buildings on farm
            foreach (Building building in farm.buildings)
            {
                if (building is CustomBuilding)
                {
                    if (building is AnimalBuilding)
                    {
                        animalBuildings.Add(building as AnimalBuilding);

                        if (CustomFarmBuildings.Config.Debug)
                            CustomFarmBuildings.Logger.Log($"Adding {(building as AnimalBuilding).buildingName} to the animal building list.");
                    }
                    else
                    {
                        customBuildings.Add(building as CustomBuilding);

                        if (CustomFarmBuildings.Config.Debug)
                            CustomFarmBuildings.Logger.Log($"Adding {(building as CustomBuilding).buildingName} to the custom building list.");
                    }
                    //StardewValleyCustomMod.Logger.Log($"AD-Height:{(building as CustomBuilding).animalDoorHeight}");

                    playerBuildings.Add(building);   
                }
                else if (building is HarvesterBuilding)
                {
                    harvesterBuildings.Add(building as HarvesterBuilding);

                    if (CustomFarmBuildings.Config.Debug)
                        CustomFarmBuildings.Logger.Log($"Adding {(building as HarvesterBuilding).buildingName} to the harvester building list.");

                    playerBuildings.Add(building);
                }
            }

            // Remove buildings from the games farm building list so it does not save and try to load
            foreach (CustomBuilding building in customBuildings)
            {
                if (CustomFarmBuildings.Config.Debug)
                    CustomFarmBuildings.Logger.Log($"Removing {building.buildingName} from the building list.");
                farm.buildings.Remove((Building) building);
                farm.buildings.Add(building.ConvertCustomBuildingToBuilding());
            }
            foreach (AnimalBuilding building in animalBuildings)
            {
                if (CustomFarmBuildings.Config.Debug)
                    CustomFarmBuildings.Logger.Log($"Removing {building.buildingName} from the building list.");
                farm.buildings.Remove((Building)building);
                farm.buildings.Add(building.ConvertCustomBuildingToBuilding());
            }
            foreach (HarvesterBuilding building in harvesterBuildings)
            {
                if (CustomFarmBuildings.Config.Debug)
                    CustomFarmBuildings.Logger.Log($"Removing {building.buildingName} from the building list.");
                farm.buildings.Remove((Building)building);
                farm.buildings.Add(building.ConvertCustomBuildingToBuilding());
            }

            // Save custom buildings to file
            playerBuildingsList.buildings = customBuildings.ToArray();
            playerBuildingsList.animalBuildings = animalBuildings.ToArray();
            playerBuildingsList.harvesterBuildings = harvesterBuildings.ToArray();
            serializer.Serialize(writer, playerBuildingsList);
            writer.Close();
            CustomFarmBuildings.Logger.Log($"Save File created for '{fileName}'.");
        }

        internal static void LoadMods()
        {
            try
            {
                CustomFarmBuildings.Logger.Log("Loading building mods...");
                string baseDir = Path.Combine(CustomFarmBuildings.ModPath, "buildingMods");
                int count = 0;
                Directory.CreateDirectory(baseDir);

                // Check each mod pack for manifest and load its contents
                foreach (string dir in Directory.EnumerateDirectories(baseDir))
                {
                    string file = Path.Combine(dir, "manifest.json");
                    if (File.Exists(file))
                    {
                        try
                        {
                            ParseMod(file);
                            count++;
                        }
                        catch (Exception err)
                        {
                            CustomFarmBuildings.Logger.Log("Unable to load manifest, json is invalid:" + file);
                            return;
                        }
                    }
                    else
                    {
                        CustomFarmBuildings.Logger.Log("Could not find a manifest.json in the " + dir + " directory, if this is intentional you can ignore this message", LogLevel.Warn);
                    }
                }
            }
            catch (Exception err)
            {
                CustomFarmBuildings.Logger.ExitGameImmediately("A unexpected error occured while loading custom building mod manifests", err);
            }
        }

        // Load the buildings from the modpack using its manifest
        internal static void ParseMod(string file)
        {
            try
            {
                BuildingManifest mani = JsonConvert.DeserializeObject<BuildingManifest>(File.ReadAllText(file));
                int count = 0;

                // Loading Buildings from file
                CustomFarmBuildings.Logger.Log($"Loading Buildings from {mani.ModName}...");
                foreach (CustomBuildingBlueprint blu in mani.CustomBuildingBlueprintList)
                {
                    blu.SetModName(mani.ModName);
                    blu.SetDefaults();
                    CustomFarmBuildings.Config.BlueprintList.Add(blu);
                    count++;

                    if (CustomFarmBuildings.Config.Debug)
                        CustomFarmBuildings.Logger.Log($"{blu.BuildingName} from {mani.ModName} added.");
                }
                CustomFarmBuildings.Logger.Log($"{count} buildings were added.");
            }
            catch (Exception err)
            {
                CustomFarmBuildings.Logger.Log(LogLevel.Error, "Unable to parse manifest.json from " + file, err);
            }
            
            
        }

        // Check if given building is from a mod
        // True: Returns the CustomBuildingBlueprint for the building
        // False: Returns null
        public static CustomBuildingBlueprint CustomBuildingCheck(Building building)
        {
            if (CustomFarmBuildings.Config.Debug)
                CustomFarmBuildings.Logger.Log($"Checking if {building.buildingType} is a custom building.");
            

            // Check the blueprint list for the building
            foreach (CustomBuildingBlueprint blu in CustomFarmBuildings.Config.BlueprintList)
            {
                if (building.buildingType.Equals(blu.ModName + "_" + blu.BuildingName))
                    return blu;
                if (building is CustomBuilding && (building as CustomBuilding).modName.Equals(blu.ModName))
                    return blu;
            }
            

            if (CustomFarmBuildings.Config.Debug)
                CustomFarmBuildings.Logger.Log($"{building.buildingType} is not a custom building.");
            return null;
        }

        private static void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            CustomFarmBuildings.Logger.Log($"Unknown Node: {e.Name}, {e.Text}");
        }

        private static void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            CustomFarmBuildings.Logger.Log($"Unknown Attribute: {attr.Name} - '{attr.Value}'");
        }
    }

    public class BuildingManifest
    {
        public String ModName;
        public List<CustomBuildingBlueprint> CustomBuildingBlueprintList;
    }
}
