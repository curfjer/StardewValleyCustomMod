/*
 *TODO 
 * More buildings - Bath House (upgrades to increase stamina faster?)?, Arcade?, Greenhouse(upgrades? more room, diff specialties?), Aquarium (display caught fish), ???
 * Add computer object, interact for menu, select farm buildings, customBuildingsMenu is opened
 * Add config option for keypress and which key opens the menu
 * fix keypress so it works more than just once - odd....it works all the time wtf
 * Ability to have option to choose different interiors? Winery for example
 * ^^^ Exteriors as well?
 * Add Multiple Exterior bool - how? seasonal not enough?
 * Add Multiple Interior bool
 * Make list for ^^^
 * Clean code
 * Birthday/Event Related textures bool???
 * add ability to adjust days of construction
 * Give custom buildings a skill level the farmer needs? ex. to build winery - farmer lvl 10, etc.
 * 
 * Make it so users can make their own mod packs, they put their folder in "CustomBuildings" and they have a folder for Buildings and BuildingInteriors???
 */

using System;

using StardewValley;
using StardewValley.Menus;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using Entoarox.Framework;
using Entoarox.Framework.Events;

using StardewValleyCustomMod.Menus;
using StardewValleyCustomMod.CustomBlueprints;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValleyCustomMod
{
    class StardewValleyCustomMod : Mod
    {
        public bool shopReplaced;
        public bool menuOpen;
        internal static Config Config;
        internal static IMonitor Logger;
        internal static IContentRegistry ContentRegistry = EntoFramework.GetContentRegistry();
        internal static string ModPath;
        internal static DebugLogger Debug;
        internal static LocalizedContentManager Content; // TODO framework has a localizer, can you use that instead?
        internal static Texture2D CustomTiles;

        public override void Entry(IModHelper helper)
        {
            Initialize(helper);

            MoreEvents.BeforeSaving += Events.Save; // Remove custom buildings from the farm map and save them
            SaveEvents.AfterSave += Events.Load; // Load custom buildings from the save file
            SaveEvents.AfterLoad += Events.Load; // Load custom buildings from the save file

            // NPC Custom Building Menu Access
            MenuEvents.MenuChanged += OnMenuChanged;
            MenuEvents.MenuClosed += OnMenuClosed;

            // Keypress Custom Building Menu Access
            ControlEvents.KeyPressed += this.ReceiveKeyPress;
        }

        private void Initialize(IModHelper helper)
        {
            ModPath = helper.DirectoryPath;
            Logger = Monitor;
            Content = new LocalizedContentManager(Game1.content.ServiceProvider, ModPath + "\\CustomBuildings");
            menuOpen = false;

            Logger.Log("Loading Config...");
            Config = helper.ReadConfig<Config>();
            if (Config == null)
            {
                Logger.Log("No config file found. Creating a config file.");
                Config = new Config();
                helper.WriteConfig<Config>(Config);
            }

            if (Config.Debug)
                Debug = new DebugLogger(); // What is this? TEST

            try
            {
                Logger = Monitor;
            }
            catch (Exception err)
            {
                Logger.ExitGameImmediately("err",err);
            }

            foreach (CustomBuildingBlueprint blu in Config.BlueprintList)
                Logger.Log($"{blu.name} added.");

            try
            {
                CustomTiles = Content.Load<Texture2D>(ModPath + "\\customFarmBuildingTilesheet");
            }
            catch (Exception err)
            {
                Logger.ExitGameImmediately("Unable to load custom tiles for the custom building menu!",err);
            }
        }

        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            if(Config.Debug)
                Logger.Log($"{e.KeyPressed.ToString()} was pressed.");
            if (e.KeyPressed.ToString() == Config.ShopAccessKey && Config.ShopAccessViaKeyPress)
            {
                Logger.Log("Opening Custom Building Menu...");
                if (Game1.activeClickableMenu is null)
                {
                    Game1.activeClickableMenu = (IClickableMenu)new CustomBuildingsMenu();
                }
                else
                {
                    Game1.activeClickableMenu.exitThisMenu();
                }
            }
        }

        // Work on this more TODO
        public void OnMenuChanged(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && !shopReplaced)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                if (shop.portraitPerson.name.Equals(Config.ShopNPCName))
                {
                    shopReplaced = true;
                    Logger.Log("Updating NPC Menu...");
                    Game1.activeClickableMenu = (IClickableMenu)new CustomBuildingsMenu();
                }
            }
        }

        public void OnMenuClosed(object sender, EventArgs e)
        {
            shopReplaced = false;
        }
    }

    public class Config
    {
        public bool Debug { get; set; } = false;
        public bool ShopAccessViaKeyPress { get; set; } = false;
        public string ShopAccessKey { get; set; } = "P"; // Captial only? TEST
        public string ShopNPCName { get; set; }
        public CustomBuildingBlueprint[] BlueprintList { get; set; }
        

        public Config()
        {
            BlueprintList = new CustomBuildingBlueprint[] { new CustomBuildingBlueprint() };
        }

        public CustomBuildingBlueprint GetCustomBuildingBlueprint(String name)
        {
            foreach(CustomBuildingBlueprint blu in BlueprintList)
            {
                if (blu.name.Equals(name))
                    return blu;
            }

            StardewValleyCustomMod.Logger.Log($"Did not find custom blueprint for {name}");
            return null;
        }
    }
}
