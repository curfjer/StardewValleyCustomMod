using System;

using StardewValley;
using StardewValley.Menus;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using Entoarox.Framework;
using Entoarox.Framework.Events;

using StardewValleyCustomMod.Menus;
using StardewValleyCustomMod.CustomBlueprints;

namespace StardewValleyCustomMod
{
    class StardewValleyCustomMod : Mod
    {
        public bool shopReplaced;
        internal static Config Config;
        internal static IMonitor Logger;
        internal static IContentRegistry ContentRegistry = EntoFramework.GetContentRegistry();
        internal static string ModPath;
        internal static DebugLogger Debug;

        public override void Entry(IModHelper helper)
        {
            Initialize(helper);

            foreach(CustomBuildingBlueprint blu in Config.blueprintList)
                Logger.Log($"{blu.name} added.");
            
            LocationEvents.CurrentLocationChanged += this.OnMapLoad;
            MoreEvents.WorldReady += MoreEvents_WorldReady;
            GameEvents.GameLoaded += MoreEvents_WorldReady;

            MenuEvents.MenuChanged += OnMenuChanged;
            MenuEvents.MenuClosed += OnMenuClosed;

            ControlEvents.KeyPressed += this.ReceiveKeyPress;

            // IDEA:
            SaveEvents.BeforeSave += Events.Save; // Remove custom buildings from the farm map and save them
            SaveEvents.AfterSave += Events.Load; // Load custom buildings from the save file
            SaveEvents.AfterLoad += Events.Load; // Load custom buildings from the save file
        }

        private void Initialize(IModHelper helper)
        {
            ModPath = helper.DirectoryPath;
            Logger = Monitor;

            Logger.Log("Loading Config...");
            Config = helper.ReadConfig<Config>();
            if (Config == null)
            {
                Logger.Log("No config file found. Creating a config file.");
                Config = new Config();
                helper.WriteConfig<Config>(Config);
            }

            if (Config.debug)
                Debug = new DebugLogger();

            try
            {
                Logger = Monitor;
            }
            catch (Exception err)
            {
                Logger.ExitGameImmediately("err",err);
            }
        }

        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed.ToString() == "P")
            {
                Logger.Log("Opening MENU");
                if (Game1.activeClickableMenu is null)
                {
                    Game1.activeClickableMenu = (IClickableMenu)new CustomBuildingsMenu();
                }
            }
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
                    Game1.activeClickableMenu = (IClickableMenu)new CustomBuildingsMenu();
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
            Events.ApplyLocation();
            this.Monitor.Log("Building Interior successfully loaded!222");
        }

        public static void ApplyPatches()
        {
            Events.ApplyLocation();
            Events.ApplyTilesheet();
        }

        internal static void TimeCheck(object s, EventArgs e)
        {
            Events.ApplyLocation();
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
        public bool debug { get; set; } = false;
        public string shopNPCName { get; set; }
        public CustomBuildingBlueprint[] blueprintList { get; set; }

        public Config()
        {
            shopNPCName = "Jackie";
            blueprintList = new CustomBuildingBlueprint[] { new CustomBuildingBlueprint() };
        }
    }

    /*
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
    }*/
}
