﻿using System;
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
using StardewValleyCustomMod.CustomBlueprints;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StardewModdingAPI;
using Microsoft.Xna.Framework;

namespace StardewValleyCustomMod
{
    [Serializable]
    public class PlayerSave
    {
        public CustomBuilding[] buildings;
    }

    internal static class Events
    {
        // Load Custom Buildings
        // TODO Need to create Save folder if dne
        internal static void Load(object s, EventArgs e)
        {
            StardewValleyCustomMod.Logger.Log("Loading custom buildings...");

            StardewValleyCustomMod.Logger.Log($"Season: {Game1.currentSeason}");

            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSave));
            // TODO check these unknowns and understand them
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            string saveGameName = Game1.player.name + "_" + Game1.uniqueIDForThisGame;
            string fileName = Path.Combine(StardewValleyCustomMod.ModPath, "Saves", saveGameName + ".xml");
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            int count = 0;

            // If save exists load save
            if (fs.Length > 0)
            {
                PlayerSave playerBuildingsList = (PlayerSave)serializer.Deserialize(fs);;

                BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");
                foreach (CustomBuilding building in playerBuildingsList.buildings)
                {
                    building.load();//needed or no?
                    //farm.buildings.Add(building.ConvertCustomBuildingToBuilding());
                    farm.buildings.Add((Building)building);
                    StardewValleyCustomMod.FarmBuildings.Add(building);
                    if (StardewValleyCustomMod.Config.Debug)
                        StardewValleyCustomMod.Logger.Log($"Loaded {building.buildingType} at {building.tileX}, {building.tileY}");
                    count++;
                }

                StardewValleyCustomMod.Logger.Log($"{count} of {playerBuildingsList.buildings.Length} Custom Buildings Loaded!");
            }
            // Save does not exist, load nothing
            else
                StardewValleyCustomMod.Logger.Log($"No custom buildings found for {saveGameName}.");
        }

        // Save and Remove Custom Buildings
        internal static void Save(object s, EventArgs e)
        {
            PlayerSave playerBuildingsList = new PlayerSave();
            string fileName = Game1.player.name + "_" + Game1.uniqueIDForThisGame;
            List<Building> playerBuildings = new List<Building>();
            List<CustomBuilding> buildings = new List<CustomBuilding>();

            string filePath = Path.Combine(StardewValleyCustomMod.ModPath, "Saves", fileName + ".xml");
            XmlSerializer serializer = new XmlSerializer(typeof(PlayerSave));
            TextWriter writer = new StreamWriter(filePath);

            StardewValleyCustomMod.Logger.Log("Creating save file...");

            BuildableGameLocation farm = (BuildableGameLocation)Game1.getLocationFromName("Farm");

            // Check for custom buildings on farm
            foreach (Building building in farm.buildings)
            {
                if (building is CustomBuilding)
                {
                    buildings.Add(building as CustomBuilding);
                    playerBuildings.Add(building);

                    if (StardewValleyCustomMod.Config.Debug)
                        StardewValleyCustomMod.Logger.Log($"Adding {building.buildingType} to the custom building list.");
                }
            }
            
            // Remove buildings from the games farm building list so it does not save and try to load
            foreach (CustomBuilding building in buildings)
            {
                if (StardewValleyCustomMod.Config.Debug)
                    StardewValleyCustomMod.Logger.Log($"Removing {building.buildingType} from the building list.");
                farm.buildings.Remove((Building) building);
            }
            
            // Save custom buildings to file
            playerBuildingsList.buildings = buildings.ToArray();
            serializer.Serialize(writer, playerBuildingsList);
            writer.Close();
            StardewValleyCustomMod.Logger.Log($"Save File created for '{fileName}'.");
        }

        internal static void LoadMods()
        {
            try
            {
                StardewValleyCustomMod.Logger.Log("Loading building mods...");
                string baseDir = Path.Combine(StardewValleyCustomMod.ModPath, "buildingMods");
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
                            StardewValleyCustomMod.Logger.Log("Unable to load manifest, json is invalid:" + file);
                            return;
                        }
                    }
                    else
                    {
                        StardewValleyCustomMod.Logger.Log("Could not find a manifest.json in the " + dir + " directory, if this is intentional you can ignore this message", LogLevel.Warn);
                    }
                }
            }
            catch (Exception err)
            {
                StardewValleyCustomMod.Logger.ExitGameImmediately("A unexpected error occured while loading custom building mod manifests", err);
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
                StardewValleyCustomMod.Logger.Log($"Loading Buildings from {mani.ModName}...");
                foreach (CustomBuildingBlueprint blu in mani.CustomBuildingBlueprintList)
                {
                    blu.SetModName(mani.ModName);
                    blu.SetDefaults();
                    StardewValleyCustomMod.Config.BlueprintList.Add(blu);
                    count++;

                    if (StardewValleyCustomMod.Config.Debug)
                        StardewValleyCustomMod.Logger.Log($"{blu.BuildingName} from {mani.ModName} added.");
                }
                StardewValleyCustomMod.Logger.Log($"{count} buildings were added.");
            }
            catch (Exception err)
            {
                StardewValleyCustomMod.Logger.Log(LogLevel.Error, "Unable to parse manifest.json from " + file, err);
            }
            
            
        }

        // Check if given building is from a mod
        // True: Returns the CustomBuildingBlueprint for the building
        // False: Returns null
        public static CustomBuildingBlueprint CustomBuildingCheck(Building building)
        {
            if (StardewValleyCustomMod.Config.Debug)
                StardewValleyCustomMod.Logger.Log($"Checking if {building.buildingType} is a custom building.");
            

            // Check the blueprint list for the building
            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.BlueprintList)
            {
                if (building.buildingType.Equals(blu.ModName + "_" + blu.BuildingName))
                    return blu;
                if (building is CustomBuilding && (building as CustomBuilding).modName.Equals(blu.ModName))
                    return blu;
            }
            

            if (StardewValleyCustomMod.Config.Debug)
                StardewValleyCustomMod.Logger.Log($"{building.buildingType} is not a custom building.");
            return null;
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

        public static void DayOfMonthChanged(object s, EventArgs e)
        {
            foreach (CustomBuilding building in StardewValleyCustomMod.FarmBuildings)
            {
                building.dayUpdate(Game1.dayOfMonth);
            }
        }

        public static void OnPostRenderEvent(object s, EventArgs e)
        {
            if (Game1.currentLocation != null && Game1.currentLocation.name.Equals("Farm"))
            {
                StardewValleyCustomMod.Logger.Log($"Drawing Farm buildings...");
                foreach (CustomBuilding building in StardewValleyCustomMod.FarmBuildings)
                {
                    StardewValleyCustomMod.Logger.Log($"Drawing {building.buildingType}.");
                    building.draw(Game1.spriteBatch);
                }
            }
        }
    }

    public class BuildingManifest
    {
        public String ModName;
        public List<CustomBuildingBlueprint> CustomBuildingBlueprintList;
    }
}
