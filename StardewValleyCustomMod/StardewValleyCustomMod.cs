/*Custom Farm Buildings
 * 
 * Current Features:
 * * Custom Building Menu
 * * * Inherits all of the features of the base games carpentry menu
 * * * Tabs to display different information about the  building
 * * * 
 * 
 * * Saves and loads building from its own xml file
 * * * Users can simply remove the mod files to uninstall the mod and any farms used with this mod will still work regardless
 * * * 
 * 
 * * Add own custom buildings
 * * * Simply make a building in your image editor, convert to xnb and make your own interior for it in tIDE.
 * * * Then put them in the corresponding folders and edit the config file to include them and any special properties you'd like them to have that the mod supports
 * *
 * 
 *TODO 
 * More buildings - Bath House (upgrades to increase stamina faster?)?, Arcade?, Greenhouse(upgrades? more room, diff specialties?), Aquarium (display caught fish), ???
 * Outhouse upgrade to Bath House?
 *  upgrade available if quantity is 1 or more, hit button to show upgraded building details? then hit select building to select which to up?
 * Forge(blacksmith building) - process geodes upgrade stuff etc. max mining?
 * Cave? Drill? Leads to mining?
 * Chicken Coop
 * Dog House? - is that a thing already or no?
 * - Upgrade button on building in menu, click displays upgraded building info, cost tab shows you need the prior building, red if 0? etc? how does base game do upgrades?
 * Add computer object, interact for menu, select farm buildings, customBuildingsMenu is opened
 *  black screen when not in use, what should it be when in use? changes based on what you select? if in building menu then mini version of the building menu is on the screen?
 *  get from mail from parent?
 *  - use a bookcase? player interacts with bookcase and get blueprint menu?
 *
 * Ability to have option to choose different exteriors? (not the same as seasonal?)
 * Clean code
 * Birthday/Event Related textures bool???
 * Notification when a building finished? - on next day load, or mail or npc talks to you when you walk out or something?
 * 
 * Custom Building texture for when building the building, day 1 just the floor?, day 2 add walls?, day 3 roof? day 4 done? etc
 * Default is games building
 * Should I have a day 0 construction texture?
 * Upgrade construction will be constructing around the prior building and turn into the upgrade
 * - the last day paint, is being applied, all the exterior decor is being added?
 * - brick buildings, bricks stacked up slowly being built
 * 
 * 
 * Custom TouchActions?
 * 
 * Next mod, can you fish on any tile (this way we can have custom water tiles)?
 * 
 * Save game does not work for multiplayer? it never syncs? maybe put a dummy building in that the mod will recognize as a key and uses it to replace with a building? will coords work as key? then you can use any building as a placeholder
 * - parse buildingType with more information that the mod can decode and load the correct building?
 * - objects are saved in the main file?
 * 
 * multiple floors
 *  - might already work if I just load the map
 *  - can add up and down arrows on interior preview to cycle through floors
 *  - building where you can go on roof or something, so you'd be on the farm but the roof of the building? building has layers maybe?
 *  
 *  add prior building into cost tab for the upgrade building
 *  
 *  info icon - make the circle bigger, but keep i the same size
 *  already built icon - maybe stagger houses on top of each other?
 *  
 *  I think I did this (UNTESTED) vvvv
 *  Is there a way to set default values after retrieve information from manifest
 *   -ex. user only sets filename, then default sets buildingname, folder name to the same as filename
 *   
 *   different textures for night - mainly for different lighting purposes? default might be good enough
 *   
 *   blue tiles for upgrade - needs to have a door and animal door version because if the upgrade tile is the same spot as the door, the door is not displayed until built
 *   
 *   manifest changes are not applied to building already built
 *   - need to have a check for current buildings and the blueprints on load (so start of every day) and change any values that are different from blueprint
 *   
 *   Support different texture sizes
 *    - 16 (base)(*4), 32(*2), 64(*1)
 *    
 *    When checking for items required, check for id numbers and/or item names
 *    
 *    scroll - arrows and bar
 *    
 *    Sorting - small menu opens with list, click, update menu, close this menu
 *    ex.  | Sort by | ... |
 *    filters - Buttons for each one, highlighted when toggled
 *    ex.  b1B2b3   b2 is on
 *    
 *    items required - sorting and filters
 *      sorting - alphabetical, type, price (weight applied to items?)
 *      filters - All, type, items you need, items you have,
 *    
 *    menu - sorting and filters - types, upgrades, 
 *      sorting - alphabetical, smart? (upgrades next to their corresponding building), price/rarity? hmmm
 *      filters - All, Upgrades, animal houses, regular buildings, magic, ???
 *      Modpack sort and/or filter?
 *      
 *    Icons for types of building
 *    - animal, magical, decoratable? etc
 *    
 *    Special Properties
 *    - Warp
 *    - Prevents debris from appearing on your farm. Keeps fences from decaying.
 *    - Harvesters
 *    - ???
 *      
 *    controller support to access menu, variable for what button to press
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
using System.Collections.Generic;
using StardewValleyCustomMod.CustomBuildings;

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
        internal static Buildings FarmBuildings;

        public override void Entry(IModHelper helper)
        {
            this.Initialize(helper);

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
            FarmBuildings = new Buildings();

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

            Events.LoadMods();

            try
            {
                Logger = Monitor;
            }
            catch (Exception err)
            {
                Logger.ExitGameImmediately("err",err);
            }

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
        //public CustomBuildingBlueprint[] BlueprintList { get; set; }
        public List<CustomBuildingBlueprint> BlueprintList;


        public Config()
        {
            //BlueprintList = new CustomBuildingBlueprint[] { new CustomBuildingBlueprint() };
            BlueprintList = new List<CustomBuildingBlueprint>();
        }

        public CustomBuildingBlueprint GetCustomBuildingBlueprint(String name)
        {
            StardewValleyCustomMod.Logger.Log($"Looking for {name}");
            foreach (CustomBuildingBlueprint blu in BlueprintList)
            {
                StardewValleyCustomMod.Logger.Log($"Checking {blu.BuildingName}");
                if (blu.BuildingName.Equals(name))
                    return blu;
            }

            StardewValleyCustomMod.Logger.Log($"Did not find custom blueprint for {name}");
            return null;
        }
    }

    public class Buildings
    {
        public List<CustomBuilding> CustomBuildings;
        public List<AnimalBuilding> AnimalBuildings;
        public List<HarvesterBuilding> HarvesterBuildings;

        public Buildings()
        {
            CustomBuildings = new List<CustomBuilding>();
            AnimalBuildings = new List<AnimalBuilding>();
            HarvesterBuildings = new List<HarvesterBuilding>();
        }
    }
}
