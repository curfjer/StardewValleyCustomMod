﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewValleyCustomMod
{
    internal static class Events
    {
        // Load Custom Buildings
        internal static void Load(object s, EventArgs e)
        {

        }

        // Save and Remove Custom Buildings
        internal static void Save(object s, EventArgs e)
        {

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
    }
}