using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewValleyCustomMod.Menus;
using Entoarox.Framework;
using Entoarox.Framework.Events;
using xTile.Dimensions;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace StardewValleyCustomMod
{
    class StardewValleyCustomMod : Mod
    {
        public bool shopReplaced;
        public Config config;
        internal static IMonitor Logger;
        internal static IContentRegistry ContentRegistry = EntoFramework.GetContentRegistry();
        internal static string ModPath;

        public override void Entry(IModHelper helper)
        {
            ModPath = helper.DirectoryPath;
            MenuEvents.MenuChanged += OnMenuChanged;
            MenuEvents.MenuClosed += OnMenuClosed;
            config = helper.ReadConfig<Config>();
            if (config == null)
            {
                config = new Config();
                helper.WriteConfig<Config>(config);
            }
            Logger = Monitor;
            LocationEvents.CurrentLocationChanged += this.OnMapLoad;
            MoreEvents.WorldReady += MoreEvents_WorldReady;
        }

        public void OnMenuChanged(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && !shopReplaced)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                if (shop.portraitPerson.name.Equals("Jackie"))
                {
                    shopReplaced = true;
                    this.Monitor.Log("Displaying Updated Shop Inventory");
                    //Game1.activeClickableMenu = new ShopMenu(shopList, 0, "Marlon");
                    Game1.activeClickableMenu = (IClickableMenu)new CustomBuildingsMenu(this.Monitor);
                }
            }
        }

        public void OnMenuClosed(object sender, EventArgs e)
        {
            shopReplaced = false;
        }

        public void OnMapLoad(object sender, EventArgs e)
        {
            this.Monitor.Log("Loading Building Interior222");
            ApplyLocation();
            this.Monitor.Log("Building Interior successfully loaded!222");
        }

        public static void ApplyLocation()
        {

            //AdvancedLocationLoaderMod.Logger.Log(location.ToString(), LogLevel.Trace);
            string wineryPath = Path.Combine(ModPath, "CustomBuildings");
            wineryPath = Path.Combine(wineryPath, "BuildingInterior");
            wineryPath = Path.Combine(wineryPath, "Winery");
            Logger.Log("Winery File Path: " + wineryPath);
            try
            {
                GameLocation loc;
                ContentRegistry.RegisterXnb(wineryPath, wineryPath);
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
                loc = new StardewValley.Locations.Cellar(map, "Winery");
                loc.objects = new SerializableDictionary<Microsoft.Xna.Framework.Vector2, StardewValley.Object>();
                loc.isOutdoors = false;
                loc.isFarm = false;
                Game1.locations.Add(loc);
                Logger.Log("Adding Winery Tilesheet...");
                /*string fakepath = Path.Combine(Path.GetDirectoryName(tilesheet.FileName), "all_sheet_paths_objects", tilesheet.SheetId, Path.GetFileName(tilesheet.FileName));
                if (tilesheet.Seasonal)
                    fakepath = fakepath.Replace("all_sheet_paths_objects", Path.Combine("all_sheet_paths_objects", Game1.currentSeason));
                stage++; // 3*/
                string fakepath = ModPath;
                ContentRegistry.RegisterXnb(fakepath, "Winery");
                Logger.Log("Winery Tilesheet Added!");
                //Game1.addNewFarmBuildingMaps();
            }
            catch (Exception err)
            {
                Logger.ExitGameImmediately("Unable to add custom location, a unexpected error occured: " + "Winery" + err);
            }
        }

        internal static void TimeCheck(object s, EventArgs e)
        {
            ApplyLocation();
        }
        internal static void MoreEvents_WorldReady(object s, EventArgs e)
        {
            //if (Configs.Compound.DynamicTiles.Count > 0 || Configs.Compound.DynamicProperties.Count > 0 || Configs.Compound.DynamicWarps.Count > 0)
                TimeEvents.DayOfMonthChanged += TimeCheck;
            //if (Configs.Compound.SeasonalTilesheets.Count > 0)
              //  TimeEvents.SeasonOfYearChanged += TimeEvents_SeasonOfYearChanged;
        }
    }

    public class Config
    {
        public bool keepDefaultsIfNotOverwritten { get; set; }
        public bool removeTopazRing { get; set; }
        public ItemForSale[] weaponList { get; set; }
        public ItemForSale[] bootList { get; set; }
        public ItemForSale[] ringList { get; set; }
        public ItemForSale[] hatList { get; set; }

        public Config()
        {
            keepDefaultsIfNotOverwritten = true;
            removeTopazRing = true;
            weaponList = new ItemForSale[] { new ItemForSale() };
            bootList = new ItemForSale[] { new ItemForSale() };
            ringList = new ItemForSale[] { new ItemForSale() };
            hatList = new ItemForSale[] { new ItemForSale() };
        }
    }

    public class ItemForSale
    {
        public int itemID { get; set; } = -1;
        public bool isUnique { get; set; } = false;
        public int salePrice { get; set; } = -1;
        public int mineLevelReached { get; set; } = -1;
        public string mailReceived { get; set; } = "";
        public int requiredKillCount { get; set; } = 0;
        public string[] requiredKillTypes { get; set; } = new string[] { "" };
        public bool requiresAllDonations { get; set; } = false;
        public int[] donatedItems { get; set; } = new int[] { -1 };
    }
}
