// Decompiled with JetBrains decompiler
// Type: StardewValley.Menus.CarpenterMenu
// Assembly: Stardew Valley, Version=1.0.6124.25603, Culture=neutral, PublicKeyToken=null
// MVID: 8735DDC4-C499-43F5-9D59-831F1FFC73CF
// Assembly location: E:\SteamLibrary\steamapps\common\Stardew Valley\Stardew Valley.exe

/* TODO
 * Buttons on the side:
 *  Description - scroll if description is too long? - add scroll ability to other tabs too if needed?
 *  Details: Height Width (Icons up down arrow, left right arrow?), Interior Selected (small blueprint icon), Time (Hourglass Icon), Animals it can hold, Already own?,  Animals currently on farm?
 *      Icon for seasonal? for magical?
 *  Cost: Money then resources listed
 *  Stats? - How many built on farm, if animals how many on farm?
 *  Select Interior
 *  Seasonal option if enabled? -> if multiple exteriors are allowed that are not seasonal? or should they be smart enough to replace what they want?
 *  
 * Interior options
 *  Seasonal Interiors?
 * 
 * Exterior options?
 * 
 * View interiors other rooms/floors???
 * - hover over warp zone, highlights tile, hover text of name of warp location, click it to load the location, clicking the farm warp does nothing
 * 
 * Check what sounds we have access to, might find some better ones
 *  - possible to get own sound files?
 *  
 *  buttons below image to show season image, event image?
 *  
 *  what are spriteeffects? all draws have spriteeffects.none
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using StardewValley;
using StardewValley.Menus;
using StardewValleyCustomMod.CustomBlueprints;
using StardewModdingAPI;
using xTile;
using StardewValley.TerrainFeatures;
using System;
using StardewValley.Tools;

namespace StardewValleyCustomMod.Menus
{
    public class CustomBuildingsMenu : IClickableMenu
    {
        public int maxWidthOfBuildingViewer = 7 * Game1.tileSize;
        public int maxHeightOfBuildingViewer = 8 * Game1.tileSize;
        public int maxWidthOfDescription = 6 * Game1.tileSize;
        private List<Item> ingredients = new List<Item>();
        private bool drawBG = true;
        private string hoverText = "";
        //private List<BluePrint> blueprints;
        private List<CustomBuildingBlueprint> blueprints;
        private int currentBlueprintIndex;
        private ClickableTextureComponent okButton;
        private ClickableTextureComponent cancelButton;
        private ClickableTextureComponent backButton;
        private ClickableTextureComponent forwardButton;
        private ClickableTextureComponent upgradeIcon;
        private ClickableTextureComponent demolishButton;
        private ClickableTextureComponent moveButton;
        private ClickableTextureComponent descriptionTab;
        private ClickableTextureComponent detailsTab;
        private ClickableTextureComponent costTab;
        private ClickableTextureComponent interiorButton;
        private ClickableTextureComponent zoomOut;
        private ClickableTextureComponent zoomIn;
        private CustomBuilding currentBuilding;
        private Building buildingToMove; // keep building or change to custombuilding? TODO
        private string buildingDescription;
        private string buildingName;
        private int price;
        private bool onFarm;
        private bool freeze;
        private bool upgrading;
        private bool demolishing;
        private bool moving;
        private bool magicalConstruction;
        //private BluePrint currentBlueprint; // Not used atm
        private LocalizedContentManager content;
        private GameLocation shopLocation;
        private IMonitor Logger;
        private bool debug;
        private int currentTab;
        private bool interior;
        private bool upgradeMode;
        private Building buildingToUpgrade;// keep building or change to custombuilding? TODO
        private int zoom;
        private static int MAXZOOM = 4;
        private DropDownMenu SorterMenu;
        private String sorter;

        public CustomBuildingsMenu()
        {
            Game1.player.forceCanMove();
            this.resetBounds();
            this.shopLocation = Game1.currentLocation;
            this.content = new LocalizedContentManager(Game1.content.ServiceProvider, "Mods\\StardewValleyCustomMod\\CustomBuildings");
            this.blueprints = new List<CustomBuildingBlueprint>();
            this.Logger = StardewValleyCustomMod.Logger;
            this.debug = StardewValleyCustomMod.Config.Debug;
            this.currentTab = 0;
            this.interior = false;
            this.upgradeMode = false;
            this.zoom = 4;
            this.sorter = "Alphabetical";

            this.Logger.Log("Loading Crafting Menu...");

            // TODO dont need this if I use CustomBuildingBlueprint since config has the list already
            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.BlueprintList)
            {
                //this.blueprints.Add(blu.convertCustomBlueprintToBluePrint());
                StardewValleyCustomMod.Debug.DebugCustomBlueprintValues(blu);
                blu.UpdateCurrentInterior(0);
                blu.SetSourceRect();
                this.blueprints.Add(blu);
                if (this.debug)
                    this.Logger.Log($"{blu.BuildingName} custom blueprint has been converted.");
            }

            this.magicalConstruction = false; //REMOVE
            this.setNewActiveBlueprint();
        }

        // List display info
        private void DrawCoords()
        {
            StardewValleyCustomMod.Logger.Log($"xPositionOnScreen: {this.xPositionOnScreen}");
            StardewValleyCustomMod.Logger.Log($"yPositionOnScreen: {this.yPositionOnScreen}");
            StardewValleyCustomMod.Logger.Log($"width: {this.width}");
            StardewValleyCustomMod.Logger.Log($"height: {this.height}");
            StardewValleyCustomMod.Logger.Log($"IClickableMenu.borderWidth: {IClickableMenu.borderWidth}");
            StardewValleyCustomMod.Logger.Log($"IClickableMenu.spaceToClearSideBorder: {IClickableMenu.spaceToClearSideBorder}");
            StardewValleyCustomMod.Logger.Log($"Game1.tilsSize: {Game1.tileSize}");
            StardewValleyCustomMod.Logger.Log($"maxHeightOfBuildingViewer: {this.maxHeightOfBuildingViewer}");
            StardewValleyCustomMod.Logger.Log($"Game1.pixelZoom: {Game1.pixelZoom}");

            StardewValleyCustomMod.Logger.Log("Buttons:");

            StardewValleyCustomMod.Logger.Log($"okButton - X: {this.okButton.bounds.X}, Y: {this.okButton.bounds.Y}, W: {this.okButton.bounds.Width} H: {this.okButton.bounds.Height}");
            StardewValleyCustomMod.Logger.Log($"cancelButton - X: {this.cancelButton.bounds.X}, Y: {this.cancelButton.bounds.Y}, W: {this.cancelButton.bounds.Width} H: {this.cancelButton.bounds.Height}");
            StardewValleyCustomMod.Logger.Log($"backButton - X: {this.backButton.bounds.X}, Y: {this.backButton.bounds.Y}, W: {this.backButton.bounds.Width} H: {this.backButton.bounds.Height}");
            StardewValleyCustomMod.Logger.Log($"forwardButton - X: {this.forwardButton.bounds.X}, Y: {this.forwardButton.bounds.Y}, W: {this.forwardButton.bounds.Width} H: {this.forwardButton.bounds.Height}");
            StardewValleyCustomMod.Logger.Log($"demolishButton - X: {this.demolishButton.bounds.X}, Y: {this.demolishButton.bounds.Y}, W: {this.demolishButton.bounds.Width} H: {this.demolishButton.bounds.Height}");
            StardewValleyCustomMod.Logger.Log($"upgradeIcon - X: {this.upgradeIcon.bounds.X}, Y: {this.upgradeIcon.bounds.Y}, W: {this.upgradeIcon.bounds.Width} H: {this.upgradeIcon.bounds.Height}");
            StardewValleyCustomMod.Logger.Log($"moveButton - X: {this.moveButton.bounds.X}, Y: {this.moveButton.bounds.Y}, W: {this.moveButton.bounds.Width} H: {this.moveButton.bounds.Height}");
        }

        private void resetBounds()
        {
            Texture2D customTiles = StardewValleyCustomMod.CustomTiles;

            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.maxHeightOfBuildingViewer / 2 - IClickableMenu.spaceToClearTopBorder + Game1.tileSize / 2;
            this.width = this.maxWidthOfBuildingViewer + this.maxWidthOfDescription + IClickableMenu.spaceToClearSideBorder * 2 + Game1.tileSize;
            this.height = this.maxHeightOfBuildingViewer + IClickableMenu.spaceToClearTopBorder;
            this.initialize(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, true);
            this.okButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - Game1.tileSize * 3 - Game1.pixelZoom * 3, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + Game1.tileSize, Game1.tileSize, Game1.tileSize), (string)null, (string)null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 373, 16, 16), (float)Game1.pixelZoom, false);
            this.cancelButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - Game1.tileSize, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + Game1.tileSize, Game1.tileSize, Game1.tileSize), (string)null, (string)null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false);
            this.backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + Game1.tileSize, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + Game1.tileSize, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.maxWidthOfBuildingViewer - Game1.tileSize * 4 + Game1.tileSize / 4, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + Game1.tileSize, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.demolishButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_Demolish"), new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - Game1.tileSize * 2 - Game1.pixelZoom * 2, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + Game1.tileSize - Game1.pixelZoom, Game1.tileSize, Game1.tileSize), (string)null, (string)null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(348, 372, 17, 17), (float)Game1.pixelZoom, false);
            this.upgradeIcon = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.maxWidthOfBuildingViewer - Game1.tileSize * 2 + Game1.tileSize / 2, this.yPositionOnScreen + Game1.pixelZoom * 2, 9 * Game1.pixelZoom, 13 * Game1.pixelZoom), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(402, 328, 9, 13), (float)Game1.pixelZoom, false);
            this.moveButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings"), new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - Game1.tileSize * 4 - Game1.pixelZoom * 5, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + Game1.tileSize, Game1.tileSize, Game1.tileSize), (string)null, (string)null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(257, 284, 16, 16), (float)Game1.pixelZoom, false);
            //this.yPositionOnScreen + Game1.tileSize * 5 / 4
            this.descriptionTab = new ClickableTextureComponent("Description", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + IClickableMenu.spaceToClearTopBorder, Game1.tileSize, Game1.tileSize), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(0, 16, 16, 16), (float)Game1.pixelZoom, false);
            this.detailsTab = new ClickableTextureComponent("Details", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder / 2, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + IClickableMenu.spaceToClearTopBorder + Game1.tileSize, Game1.tileSize, Game1.tileSize), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(16, 0, 16, 16), (float)Game1.pixelZoom, false);
            this.costTab = new ClickableTextureComponent("Cost", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder / 2, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.tileSize * 2 + IClickableMenu.spaceToClearTopBorder, Game1.tileSize, Game1.tileSize), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), (float)Game1.pixelZoom, false);
            this.interiorButton = new ClickableTextureComponent("Select Interior", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.tileSize * 3 + Game1.pixelZoom*4 + IClickableMenu.spaceToClearTopBorder, 25 * Game1.pixelZoom, 18 * Game1.pixelZoom), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(32, 0, 25, 18), (float)Game1.pixelZoom, false);

            this.zoomIn = new ClickableTextureComponent("Zoom In", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen - Game1.tileSize * 5 / 2 + 2 * Game1.pixelZoom, Game1.viewport.Height / 2 - 38 / 2 * Game1.pixelZoom - (8 + 1) * Game1.pixelZoom, Game1.tileSize, Game1.tileSize), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(152, 58, 7, 8), (float)Game1.pixelZoom, false);
            this.zoomOut = new ClickableTextureComponent("Zoom Out", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen - Game1.tileSize * 5 / 2 + 2 * Game1.pixelZoom, Game1.viewport.Height / 2 + 38 / 2 * Game1.pixelZoom + Game1.pixelZoom, Game1.tileSize, Game1.tileSize), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(152, 106, 7, 8), (float)Game1.pixelZoom, false);

            String[] sorterMenuOptions = { "Alphabetical", "test1", "test2" };
            this.SorterMenu = new DropDownMenu("Sort by: ", sorterMenuOptions, new Vector2(this.xPositionOnScreen, this.yPositionOnScreen - Game1.tileSize));
        }

        public void setNewActiveBlueprint()
        {
            this.Logger.Log($"CurrentBlueprintIndex: {this.currentBlueprintIndex}"); // DEBUG REMOVE
            //this.currentBuilding = new Building(this.blueprints[this.currentBlueprintIndex], Vector2.Zero);
            this.currentBuilding = new CustomBuilding(this.CurrentBlueprint, Vector2.Zero);
            this.price = this.blueprints[this.currentBlueprintIndex].MoneyRequired;
            this.ingredients.Clear();
            foreach (KeyValuePair<int, int> keyValuePair in this.blueprints[this.currentBlueprintIndex].ResourcesRequired)
                this.ingredients.Add((Item)new StardewValley.Object(keyValuePair.Key, keyValuePair.Value, false, -1, 0));
            this.buildingDescription = this.blueprints[this.currentBlueprintIndex].Description;
            this.buildingName = this.blueprints[this.currentBlueprintIndex].BuildingName;
            this.Logger.Log($"CurrentBuildingName: {this.buildingName}"); // DEBUG REMOVE
        }

        // TODO Hover over icons, text description of the icon
        // is there some sort of texturecomponent I can use for that ^^^???
        public override void performHoverAction(int x, int y)
        {
            this.cancelButton.tryHover(x, y, 0.1f);
            this.backButton.tryHover(x, y, 1f);
            this.forwardButton.tryHover(x, y, 1f);
            this.okButton.tryHover(x, y, 0.1f);
            base.performHoverAction(x, y);
            if (!this.onFarm && !this.interior)
            {
                // Hover Icon
                
                this.demolishButton.tryHover(x, y, 0.1f);
                this.moveButton.tryHover(x, y, 0.1f);
                //this.detailsButton.tryHover(x, y, 0.1f);
                //this.costButton.tryHover(x, y, 0.1f);
                this.interiorButton.tryHover(x, y, 0.1f);
                this.zoomIn.tryHover(x, y, 0.1f);
                this.zoomOut.tryHover(x, y, 0.1f);

                // Hover Text
                if (this.CurrentBlueprint.isUpgrade() && this.upgradeIcon.containsPoint(x, y))
                    this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Upgrade", (object)this.CurrentBlueprint.NameOfBuildingToUpgrade);
                else if (this.demolishButton.containsPoint(x, y))
                    this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Demolish");
                else if (this.moveButton.containsPoint(x, y))
                    this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings");
                else if (this.okButton.containsPoint(x, y) && this.CurrentBlueprint.doesFarmerHaveEnoughResourcesToBuild())
                    this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Build");
                else if (this.descriptionTab.containsPoint(x, y))
                    this.hoverText = this.descriptionTab.name;
                else if (this.detailsTab.containsPoint(x, y))
                    this.hoverText = this.detailsTab.name;
                else if (this.costTab.containsPoint(x, y))
                    this.hoverText = this.costTab.name;
                else if (this.interiorButton.containsPoint(x, y))
                    this.hoverText = this.interiorButton.name;
                else
                    this.hoverText = "";
            }
            // TODO Any hover info?
            // highlight warp tiles when hovering over them? - click to see location? click does not work on farm warp
            // do this in draw()?
            else if (this.interior)
            {

            }
            // TODO figure what this is doing
            else
            {
                if (!this.upgrading && !this.demolishing && !this.moving || this.freeze)
                    return;
                foreach (Building building in ((BuildableGameLocation)Game1.getLocationFromName("Farm")).buildings)
                    building.color = Color.White;
                Building building1 = ((BuildableGameLocation)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize))) ?? ((BuildableGameLocation)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY() + Game1.tileSize * 2) / Game1.tileSize))) ?? ((BuildableGameLocation)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY() + Game1.tileSize * 3) / Game1.tileSize)));
                if (this.upgrading)
                {
                    if (building1 != null && this.CurrentBlueprint.NameOfBuildingToUpgrade != null && this.CurrentBlueprint.NameOfBuildingToUpgrade.Equals(building1.buildingType))
                    {
                        building1.color = Color.Lime * 0.8f;
                    }
                    else
                    {
                        if (building1 == null)
                            return;
                        building1.color = Color.Red * 0.8f;
                    }
                }
                else if (this.demolishing)
                {
                    if (building1 == null)
                        return;
                    building1.color = Color.Red * 0.8f;
                }
                else
                {
                    if (!this.moving || building1 == null)
                        return;
                    building1.color = Color.Lime * 0.8f;
                }
            }
        }

        public override bool readyToClose()
        {
            if (base.readyToClose())
                return this.buildingToMove == null;
            return false;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (this.freeze)
                return;
            if (!this.onFarm)
                base.receiveKeyPress(key);
            if (Game1.globalFade || !this.onFarm)
                return;
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.returnToCarpentryMenu), 0.02f);
            else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                Game1.panScreen(0, 4);
            else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                Game1.panScreen(4, 0);
            else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
            {
                Game1.panScreen(0, -4);
            }
            else
            {
                if (!Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                    return;
                Game1.panScreen(-4, 0);
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (!this.onFarm && !this.interior || Game1.globalFade)
                return;
            int num1 = Game1.getOldMouseX() + Game1.viewport.X;
            int num2 = Game1.getOldMouseY() + Game1.viewport.Y;
            if (num1 - Game1.viewport.X < Game1.tileSize)
                Game1.panScreen(-8, 0);
            else if (num1 - (Game1.viewport.X + Game1.viewport.Width) >= -Game1.tileSize * 2)
                Game1.panScreen(8, 0);
            if (num2 - Game1.viewport.Y < Game1.tileSize)
                Game1.panScreen(0, -8);
            else if (num2 - (Game1.viewport.Y + Game1.viewport.Height) >= -Game1.tileSize)
                Game1.panScreen(0, 8);
            foreach (Keys pressedKey in Game1.oldKBState.GetPressedKeys())
                this.receiveKeyPress(pressedKey);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.freeze)
                return;
            if (!this.onFarm && !this.interior) // TODO do I need this for interior screen too; test
                base.receiveLeftClick(x, y, playSound);
            String sort = this.SorterMenu.OptionClicked(x, y);
            if (sort != null)
                this.sorter = sort;

            //
            // Check buttons and tabs:
            //
            // Cancel Button
            if (this.cancelButton.containsPoint(x, y))
            {
                if (!this.onFarm && !this.interior)
                {
                    this.exitThisMenu(true);
                    Game1.player.forceCanMove();
                    Game1.playSound("bigDeSelect");
                }
                else
                {
                    // TODO understand this better
                    if (this.moving && this.buildingToMove != null)
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                    else if (this.interior)
                    {
                        this.CurrentBlueprint.ResetCustomInterior();
                        this.interior = false;
                    }

                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.returnToCarpentryMenu), 0.02f);
                    Game1.playSound("smallSelect");
                    return;
                }
            }
            if (!this.onFarm)
            {
                // Back Button
                if (this.backButton.containsPoint(x, y))
                {
                    if (this.interior)
                    {
                        this.CurrentBlueprint.UpdateCurrentInterior(-1);
                        this.DisplayInterior();
                    }
                    else
                    {
                        if (this.currentBlueprintIndex == 0)
                            this.currentBlueprintIndex = this.blueprints.Count - 1;
                        else
                            this.currentBlueprintIndex -= 1;
                        this.setNewActiveBlueprint();
                    }
                    Game1.playSound("shwip");
                    this.backButton.scale = this.backButton.baseScale;
                }
                // Forward Button
                if (this.forwardButton.containsPoint(x, y))
                {
                    if (this.interior)
                    {
                        this.CurrentBlueprint.UpdateCurrentInterior(1);
                        this.DisplayInterior();
                    }
                    else
                    {
                        if (this.currentBlueprintIndex >= this.blueprints.Count - 1)
                            this.currentBlueprintIndex = 0;
                        else
                            this.currentBlueprintIndex += 1;
                        this.setNewActiveBlueprint();
                    }
                    Game1.playSound("shwip");
                    this.forwardButton.scale = this.forwardButton.baseScale;
                }
                if (!this.interior)
                {
                    // Demolish Button
                    if (this.demolishButton.containsPoint(x, y))
                    {
                        Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
                        Game1.playSound("smallSelect");
                        this.onFarm = true;
                        this.demolishing = true;
                    }
                    // Move Button
                    if (this.moveButton.containsPoint(x, y))
                    {
                        Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
                        Game1.playSound("smallSelect");
                        this.onFarm = true;
                        this.moving = true;
                    }
                }
                // OK Button
                if (this.okButton.containsPoint(x, y))
                {
                    if (!this.interior && (Game1.player.money >= this.price && this.blueprints[this.currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild()))
                    {
                        Game1.playSound("smallSelect");
                        Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
                        this.onFarm = true;
                    }
                    else if (this.interior)
                    {
                        Game1.playSound("smallSelect");
                        Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.returnToCarpentryMenu), 0.02f);
                        this.interior = false;
                    }
                }
                // Zoom Buttons
                if (this.zoomIn.containsPoint(x, y))
                {
                    Game1.playSound("smallSelect");
                    this.zoom++;
                    if (this.zoom >= CustomBuildingsMenu.MAXZOOM)
                        this.zoom = 4;
                }
                if (this.zoomOut.containsPoint(x, y))
                {
                    Game1.playSound("smallSelect");
                    this.zoom--;
                    if (this.zoom <= 0)
                        this.zoom = 1;
                }
                // Description Tab
                if (this.descriptionTab.containsPoint(x, y) && this.currentTab != 0)
                {
                    Game1.playSound("smallSelect");
                    this.currentTab = 0;
                    this.UpdateCurrentTabSelected();
                }
                // Details Tab
                if (this.detailsTab.containsPoint(x, y) && this.currentTab != 1)
                {
                    Game1.playSound("smallSelect");
                    this.currentTab = 1;
                    this.UpdateCurrentTabSelected();
                }
                // Cost Tab
                if (this.costTab.containsPoint(x, y) && this.currentTab != 2)
                {
                    Game1.playSound("smallSelect");
                    this.currentTab = 2;
                    this.UpdateCurrentTabSelected();
                }
                // Interior Button
                if (this.interiorButton.containsPoint(x, y))
                {
                    Game1.playSound("smallSelect");
                    this.DisplayInterior();
                    this.interior = true;
                }
            }
            // TODO Do not need this if once you make the methods for the if statements below
            else if (!this.freeze && !Game1.globalFade && !this.interior)
            {
                // Update current option selected:
                if (this.demolishing)
                {
                    this.DemoBuilding();
                }
                else if (this.upgrading)
                {
                    this.UpgradeBuilding();
                }
                else if (this.moving)
                {
                    this.MoveBuilding();
                }
                else if (this.tryToBuild())
                {
                    this.Logger.Log($"Successfully started building the {currentBuilding.buildingType}"); // DEBUG REMOVE
                    this.CurrentBlueprint.consumeResources();
                    DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(this.returnToCarpentryMenuAfterSuccessfulBuild), 2000);
                    this.freeze = true;
                }
                else
                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), Color.Red, 3500f));
            }
        }

        public void UpgradeBuilding()
        {
            if (!this.upgradeMode)
            {
                Building buildingAt = ((BuildableGameLocation)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize)));
                if (buildingAt != null && this.CurrentBlueprint.BuildingName != null && /*buildingAt.buildingType.Split('_').GetValue(1)*/buildingAt.buildingType.Equals(this.CurrentBlueprint.NameOfBuildingToUpgrade))
                {
                    this.upgradeMode = true;
                    this.buildingToUpgrade = buildingAt;
                }
                else
                {
                    if (buildingAt == null)
                        return;
                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType"), Color.Red, 3500f));
                }
            }
            else
            {
                Game1.getFarm().buildings.Remove(this.buildingToUpgrade);
                
                if (this.tryToBuild())
                {
                    this.upgradeMode = false;
                    this.CurrentBlueprint.consumeResources();
                    //buildingAt.daysUntilUpgrade = 2;
                    //buildingAt.showUpgradeAnimation((GameLocation)Game1.getFarm());
                    Game1.playSound("axe");
                    DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(this.returnToCarpentryMenuAfterSuccessfulBuild), 1500);
                    this.freeze = true;
                }
                else
                {
                    Game1.getFarm().buildings.Add(this.buildingToUpgrade);
                }
            }
            
        }

        public void MoveBuilding()
        {
            if (this.buildingToMove == null)
            {
                this.buildingToMove = ((BuildableGameLocation)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getMouseY()) / Game1.tileSize)));
                if (this.buildingToMove == null)
                    return;
                if (this.buildingToMove.daysOfConstructionLeft > 0)
                {
                    this.buildingToMove = (Building)null;
                }
                else
                {
                    ((BuildableGameLocation)Game1.getLocationFromName("Farm")).buildings.Remove(this.buildingToMove);
                    Game1.playSound("axchop");
                }
            }
            else if (((BuildableGameLocation)Game1.getLocationFromName("Farm")).buildStructure(this.buildingToMove, new Vector2((float)((Game1.viewport.X + Game1.getMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getMouseY()) / Game1.tileSize)), false, Game1.player))
            {
                this.buildingToMove = (Building)null;
                Game1.playSound("axchop");
                DelayedAction.playSoundAfterDelay("dirtyHit", 50);
                DelayedAction.playSoundAfterDelay("dirtyHit", 150);
            }
            else
                Game1.playSound("cancel");
        }

        public void DemoBuilding()
        {
            Building buildingAt = ((BuildableGameLocation)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize)));
            if (buildingAt != null && (buildingAt.daysOfConstructionLeft > 0 || buildingAt.daysUntilUpgrade > 0))
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction"), Color.Red, 3500f));
            else if (buildingAt != null && buildingAt.indoors != null && (buildingAt.indoors is AnimalHouse && (buildingAt.indoors as AnimalHouse).animalsThatLiveHere.Count > 0))
            {
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere"), Color.Red, 3500f));
            }
            else
            {
                if (buildingAt == null || !((BuildableGameLocation)Game1.getLocationFromName("Farm")).destroyStructure(buildingAt))
                    return;
                int tileY = buildingAt.tileY;
                int tilesHigh = buildingAt.tilesHigh;
                Game1.flashAlpha = 1f;
                buildingAt.showDestroyedAnimation((GameLocation)Game1.getFarm());
                Game1.playSound("explosion");
                Utility.spreadAnimalsAround(buildingAt, (Farm)Game1.getLocationFromName("Farm"));
                DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(this.returnToCarpentryMenu), 1500);
                this.freeze = true;
            }
        }

        public bool tryToBuild()
        {
            bool built = false;
            if (this.upgradeMode)
                built = ((BuildableGameLocation)Game1.getLocationFromName("Farm")).buildStructure((Building)this.currentBuilding, new Vector2(this.currentBuilding.tileX, this.currentBuilding.tileY), false, Game1.player);
            else if (this.currentBuilding.specialProperties[0].Equals("Harvester"))
            {
                JunimoHut harvester = new JunimoHut();
            }
            else
                built = ((BuildableGameLocation)Game1.getLocationFromName("Farm")).buildStructure((Building)this.currentBuilding, new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize)), false, Game1.player);
            this.currentBuilding.performActionOnConstruction((GameLocation)(BuildableGameLocation)Game1.getLocationFromName("Farm"));
            return built;
        }

        public void returnToCarpentryMenu()
        {
            Game1.currentLocation.cleanupBeforePlayerExit();
            //Game1.currentLocation = Game1.getLocationFromName(this.magicalConstruction ? "WizardHouse" : "ScienceHouse");
            Game1.currentLocation = this.shopLocation;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f);
            this.onFarm = false;
            this.resetBounds();
            this.upgrading = false;
            this.moving = false;
            this.freeze = false;
            Game1.displayHUD = true;
            Game1.viewportFreeze = false;
            Game1.viewport.Location = new Location(5 * Game1.tileSize, 24 * Game1.tileSize);
            this.drawBG = true;
            this.demolishing = false;
            Game1.displayFarmer = true;
        }

        public void returnToCarpentryMenuAfterSuccessfulBuild()
        {
            if(this.debug)
                this.Logger.Log("Returning to Carpentry Menu..."); // DEBUG REMOVE
            Game1.currentLocation.cleanupBeforePlayerExit();
            //Game1.currentLocation = Game1.getLocationFromName(this.magicalConstruction ? "WizardHouse" : "ScienceHouse");
            Game1.currentLocation = this.shopLocation;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear(new Game1.afterFadeFunction(this.robinConstructionMessage), 0.02f);
            this.Logger.Log($"Fading to {this.shopLocation}"); // DEBUG REMOVE
            Game1.displayHUD = true;
            Game1.viewportFreeze = false;
            Game1.viewport.Location = new Location(5 * Game1.tileSize, 24 * Game1.tileSize);
            this.freeze = true;
            Game1.displayFarmer = true;
        }

        // CHANGE THIS TODO
        public void robinConstructionMessage()
        {
            this.exitThisMenu(true);
            Game1.player.forceCanMove();
            /*
            if (this.magicalConstruction)
                return;
            string path = "Data\\ExtraDialogue:Robin_" + (this.upgrading ? "Upgrade" : "New") + "Construction";
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
                path += "_Festival";
            Game1.drawDialogue(Game1.getCharacterFromName("Robin", false), Game1.content.LoadString(path, (object)this.CurrentBlueprint.BuildingName.ToLower(), (object)((IEnumerable<string>)this.CurrentBlueprint.BuildingName.ToLower().Split(' ')).Last<string>()));
            */
        }

        public void setUpForBuildingPlacement()
        {
            Game1.currentLocation.cleanupBeforePlayerExit();
            this.hoverText = "";
            Game1.currentLocation = Game1.getLocationFromName("Farm");
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f);
            this.onFarm = true;
            this.cancelButton.bounds.X = Game1.viewport.Width - Game1.tileSize * 2;
            this.cancelButton.bounds.Y = Game1.viewport.Height - Game1.tileSize * 2;
            Game1.displayHUD = false;
            Game1.viewportFreeze = true;
            Game1.viewport.Location = new Location(49 * Game1.tileSize, 5 * Game1.tileSize);
            Game1.panScreen(0, 0);
            this.drawBG = false;
            this.freeze = false;
            Game1.displayFarmer = false;
            if (this.demolishing || this.CurrentBlueprint.NameOfBuildingToUpgrade == null || (this.CurrentBlueprint.NameOfBuildingToUpgrade.Length <= 0 || this.moving))
                return;
            this.upgrading = true;
        }

        public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
        {
            this.resetBounds();
        }

        public override void draw(SpriteBatch b)
        {
            //b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)tileX, (float)tileY) * (float)Game1.tileSize) + new Vector2(0.0f, (float)(Game1.tileSize - height * Game1.pixelZoom + Game1.tileSize / 4)), new Rectangle?(new Rectangle(351, 261, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-05f);
            //b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)tileX, (float)tileY) * (float)Game1.tileSize) + new Vector2(0.0f, (float)(Game1.tileSize - height * Game1.pixelZoom)) + (this.newConstructionTimer > 0 ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(new Rectangle(351, 293, 16, height)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, (float)(tileY * Game1.tileSize + Game1.tileSize - 1) / 10000f);
            if (this.drawBG)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            if (Game1.globalFade || this.freeze)
                return;

            // Draw Building Selection (the menu):
            if (!this.onFarm && !this.interior)
            {
                base.draw(b);
                // Draw box for building display
                IClickableMenu.drawTextureBox(b, this.xPositionOnScreen - Game1.tileSize * 3 / 2, this.yPositionOnScreen - Game1.tileSize / 4, this.maxWidthOfBuildingViewer + Game1.tileSize, this.maxHeightOfBuildingViewer + Game1.tileSize, this.magicalConstruction ? Color.RoyalBlue : Color.White);

                /* X: this.xPositionOnScreen + this.maxWidthOfBuildingViewer / 2 - this.currentBuilding.tilesWide * Game1.tileSize / 2 - Game1.tileSize
                 * this.xPositionOnScreen + this.maxWidthofBuildingViewer / 2
                 * - (this.currentBuilding.texture.Bounds.Width / 2) * this.zoom
                 * 
                 * maybe - do this in rectangle so that you always see the center of the building?
                 * if (this.currentBuilding.texture.Bounds.Width * this.zoom > this.maxWidthofBuildingViewer)
                 *  this.xPositionOnScreen
                 * 
                 * Y:this.yPositionOnScreen + this.maxHeightOfBuildingViewer / 2 - this.currentBuilding.getSourceRectForMenu().Height * Game1.pixelZoom / 2
                 * this.yPositionOnScreen + this.maxHeightOfBuildingViewer / 2
                 * - (this.currentBuilding.texture.Bounds.Height / 2) * this.zoom
                 */
                //this.currentBuilding.drawInMenu(b, this.xPositionOnScreen + this.maxWidthOfBuildingViewer / 2 - this.currentBuilding.tilesWide * Game1.tileSize / 2 - Game1.tileSize, this.yPositionOnScreen + this.maxHeightOfBuildingViewer / 2 - this.currentBuilding.getSourceRectForMenu().Height * Game1.pixelZoom / 2, this.zoom);
                float scalar = (float)this.maxWidthOfBuildingViewer / (float)this.currentBuilding.texture.Width;
                int shadowHeight = 16;

                if ((this.currentBuilding.texture.Height + shadowHeight)* scalar > this.maxHeightOfBuildingViewer)
                    scalar = (float)this.maxHeightOfBuildingViewer / (float)(this.currentBuilding.texture.Height + shadowHeight);

                //this.currentBuilding.drawInMenu(b, this.xPositionOnScreen - Game1.tileSize - IClickableMenu.spaceToClearSideBorder, this.yPositionOnScreen + this.maxHeightOfBuildingViewer / 2 - (this.currentBuilding.texture.Bounds.Height / 2) * this.zoom, this.zoom);
                //else
                Vector2 buildingPosition = new Vector2((int)(this.xPositionOnScreen + this.maxWidthOfBuildingViewer / 2 - (this.currentBuilding.texture.Bounds.Width / 2) * scalar - Game1.tileSize), (int)(this.yPositionOnScreen + this.maxHeightOfBuildingViewer / 2 - (this.currentBuilding.texture.Bounds.Height / 2) * scalar));
                this.currentBuilding.drawInMenu(b, (int)buildingPosition.X, (int)buildingPosition.Y, scalar);
                Microsoft.Xna.Framework.Rectangle farmerSize = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 32);
                this.drawFarmer(b, buildingPosition + new Vector2(-farmerSize.Width / 2 * scalar, (this.currentBuilding.texture.Bounds.Height - farmerSize.Height) * scalar), scalar);


                //
                // Should this be displayed with exterior only?
                if (this.CurrentBlueprint.isUpgrade())
                    this.upgradeIcon.draw(b);

                // TODO why is the string "Deluxe Barn"???? end of this line
                SpriteText.drawStringWithScrollBackground(b, this.buildingName, this.xPositionOnScreen + this.maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder - Game1.tileSize / 4 + Game1.tileSize + ((this.width - (this.maxWidthOfBuildingViewer + Game1.tileSize * 2)) / 2 - SpriteText.getWidthOfString("Deluxe Barn") / 2), this.yPositionOnScreen, "Deluxe Barn", 1f, -1);
                IClickableMenu.drawTextureBox(b, this.xPositionOnScreen + this.maxWidthOfBuildingViewer - Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize * 5 / 4, this.maxWidthOfDescription + Game1.tileSize, this.maxWidthOfDescription + Game1.tileSize * 3 / 2, this.magicalConstruction ? Color.RoyalBlue : Color.White);

                // Draw Current Tab:
                if (this.currentTab == 0)
                    this.DrawDescriptionTab(b);
                else if (this.currentTab == 1)
                    this.DrawDetailsTab(b);
                else if (this.currentTab == 2)
                    this.DrawCostTab(b);
                
                // Draw Buttons and Tabs:
                this.backButton.draw(b);
                this.forwardButton.draw(b);
                this.okButton.draw(b, this.blueprints[this.currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild() ? Color.White : Color.Gray * 0.8f, 0.88f);
                this.demolishButton.draw(b);
                this.moveButton.draw(b);
                //this.zoomIn.draw(b);
                //this.zoomOut.draw(b);
                this.descriptionTab.draw(b);
                this.detailsTab.draw(b);
                this.costTab.draw(b);
                if(this.CurrentBlueprint.HasInterior())
                    this.interiorButton.draw(b);

                SorterMenu.Draw(b, Game1.pixelZoom);
                //b.Draw(StardewValleyCustomMod.CustomTiles, new Vector2(this.xPositionOnScreen - Game1.tileSize * 5 / 2, Game1.viewport.Height / 2 - 38 / 2 * Game1.pixelZoom), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(150, 67, 10, 38)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.999f);
                //b.Draw(StardewValleyCustomMod.CustomTiles, new Vector2(this.xPositionOnScreen - Game1.tileSize * 5 / 2 - Game1.pixelZoom, Game1.viewport.Height / 2 + (-38 / 2 + 9 * (4 - this.zoom) + 2) * Game1.pixelZoom), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(139, 67, 10, 7)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.999f);
            }
            // Draw Interior Selection Screen:
            else if (this.interior)
            {
                this.DrawInterior(b);
            }
            else
            {
                string str;
                if (!this.upgrading)
                    str = this.demolishing ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish") : Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation");
                else
                    str = Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", (object)this.CurrentBlueprint.NameOfBuildingToUpgrade);
                string s = str;
                SpriteText.drawStringWithScrollBackground(b, s, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, Game1.tileSize / 4, "", 1f, -1);
                if (!this.upgrading && !this.demolishing && !this.moving)
                {
                    Vector2 vector2 = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize));
                    for (int y = 0; y < this.CurrentBlueprint.TilesHeight; ++y)
                    {
                        for (int x = 0; x < this.CurrentBlueprint.TilesWidth; ++x)
                        {
                            int structurePlacementTile = this.CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x, y);
                            Vector2 tileLocation = new Vector2(vector2.X + (float)x, vector2.Y + (float)y);
                            if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(tileLocation))
                                ++structurePlacementTile;
                            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * (float)Game1.tileSize), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + structurePlacementTile * 16, 388, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.999f);
                        }
                    }
                }
                else if (this.upgradeMode)
                    this.DrawBuildingTiles(b, (Building)this.currentBuilding);
                else if (this.moving && this.buildingToMove != null)
                {
                    this.DrawBuildingTiles(b, this.buildingToMove);
                }
            }
            this.cancelButton.draw(b);
            this.drawMouse(b);
            if (this.hoverText.Length > 0)
                IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
        }

        // Draws farmer to scale of building
        public void drawFarmer(SpriteBatch b, Vector2 position, float scalar)
        {
            // Draw Farmer scaled to building TODO
            StardewValley.Farmer farmer = Game1.player;
            
            //farmer.FarmerRenderer.draw(b, farmer.FarmerSprite.CurrentAnimationFrame, farmer.FarmerSprite.CurrentFrame, farmer.FarmerSprite.sourceRect, new Vector2(this.xPositionOnScreen - Game1.tileSize * 3 / 2, this.yPositionOnScreen + this.maxHeightOfBuildingViewer - Game1.tileSize * 4), Vector2.Zero, 1f, Color.White, 0.0f, scalar, farmer);

            FarmerRenderer farmerRend = farmer.FarmerRenderer;

            //Vector2 position = new Vector2((float)(this.xPositionOnScreen - Game1.tileSize * 5 / 4), (float)(this.yPositionOnScreen + this.maxHeightOfBuildingViewer - Game1.tileSize * 2 * scalar / Game1.pixelZoom));
            Vector2 origin = Vector2.Zero;
            Vector2 rotationAdjustment = Vector2.Zero;
            Vector2 positionOffset = new Vector2();
            
            int currentFrame = 0;
            int facingDirection = 2;
            int heightOffset = 0;
            if (!farmer.isMale)
                heightOffset = 4;
            int eyeXOffset = 5;
            int eyeYOffset = 12;
            int shirtYOffset = 15;
            int shirtXOffset = 4;

            float layerDepth = 0.8f;
            float rotation = 0.0f;
            Color overrideColor = Color.White;

            FarmerSprite.AnimationFrame animationFrame = new FarmerSprite.AnimationFrame(0, 0, false, false, (AnimatedSprite.endOfAnimationBehavior)null, false);
            positionOffset.Y = (float)animationFrame.positionOffset * Game1.pixelZoom;
            positionOffset.X = (float)animationFrame.xOffset * Game1.pixelZoom;

            Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 32);
            Microsoft.Xna.Framework.Rectangle shirtSourceRect = new Microsoft.Xna.Framework.Rectangle(farmer.shirt * 8 % FarmerRenderer.shirtsTexture.Width, farmer.shirt * 8 / FarmerRenderer.shirtsTexture.Width * 32, 8, 8);
            Microsoft.Xna.Framework.Rectangle hairstyleSourceRect = new Microsoft.Xna.Framework.Rectangle(farmer.getHair() * 16 % FarmerRenderer.hairStylesTexture.Width, farmer.getHair() * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);
            Microsoft.Xna.Framework.Rectangle accessorySourceRect = new Microsoft.Xna.Framework.Rectangle();

            if (farmer.accessory >= 0)
                accessorySourceRect = new Microsoft.Xna.Framework.Rectangle(farmer.accessory * 16 % FarmerRenderer.accessoriesTexture.Width, farmer.accessory * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16);
            
            //Vector2 position1 = new Vector2((float)(this.gamesToLoadButton[index].bounds.X + Game1.tileSize + Game1.tileSize - Game1.pixelZoom), (float)(this.gamesToLoadButton[index].bounds.Y + Game1.tileSize * 2 + Game1.pixelZoom * 4));
            // Draw Farmers shadow
            b.Draw(Game1.shadowTexture, position + origin + positionOffset + new Vector2((sourceRect.Width - Game1.shadowTexture.Width) / 2 * scalar, (sourceRect.Height - Game1.shadowTexture.Height / 2 - 1) * scalar), Game1.shadowTexture.Bounds, Color.White, rotation, origin, scalar, SpriteEffects.None, (float)layerDepth);

            // Draws base of head, chest, and shoes
            b.Draw(farmerRend.baseTexture, position + origin + positionOffset, new Microsoft.Xna.Framework.Rectangle?(sourceRect), overrideColor, rotation, origin, scalar, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

            // Draws nothing currently, look at farmer_base
            sourceRect.Offset(288, 0);
            b.Draw(farmerRend.baseTexture, position + origin + positionOffset, new Microsoft.Xna.Framework.Rectangle?(sourceRect), overrideColor.Equals(Color.White) ? farmer.pantsColor : overrideColor, rotation, origin, scalar, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (farmer.FarmerSprite.CurrentAnimationFrame.frame == 5 ? 0.00092f : 9.2E-08f));

            if (farmer.currentEyes != 0 && facingDirection != 0 && (!farmer.isRidingHorse() && Game1.timeOfDay < 2600) && (!farmer.FarmerSprite.pauseForSingleAnimation || farmer.usingTool && farmer.CurrentTool is FishingRod))
            {
                // Not sure - part of chest? - same x value as eyes so used that, y value is 1 less than eyes
                b.Draw(farmerRend.baseTexture, position + origin + positionOffset + new Vector2((float)(eyeXOffset * scalar), (float)(eyeYOffset - 1 * scalar)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(5, 16, facingDirection == 2 ? 6 : 2, 2)), overrideColor, 0.0f, origin, scalar, SpriteEffects.None, layerDepth + 5E-08f);
                // Eyes
                b.Draw(farmerRend.baseTexture, position + origin + positionOffset + new Vector2((float)(eyeXOffset * scalar), (float)(eyeYOffset * scalar)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(264 + (facingDirection == 3 ? 4 : 0), 2 + (farmer.currentEyes - 1) * 2, facingDirection == 2 ? 6 : 2, 2)), overrideColor, 0.0f, origin, scalar, SpriteEffects.None, layerDepth + 1.2E-07f);
            }

            
            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2((float)(shirtXOffset * scalar), (float)((double)(shirtYOffset * scalar))), new Microsoft.Xna.Framework.Rectangle?(shirtSourceRect), overrideColor, rotation, origin, scalar, SpriteEffects.None, layerDepth + 1.5E-07f);

            // Accessory
            if (farmer.accessory >= 0)
                b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2((float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * Game1.pixelZoom), (float)(8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * Game1.pixelZoom + heightOffset - 4)), new Microsoft.Xna.Framework.Rectangle?(accessorySourceRect), !overrideColor.Equals(Color.White) || farmer.accessory >= 6 ? overrideColor : farmer.hairstyleColor, rotation, origin, (float)(scalar + ((double)rotation != 0.0 ? 0.0 : 0.0)), SpriteEffects.None, layerDepth + (farmer.accessory < 8 ? 1.9E-05f : 2.9E-05f));
            // Hair
            b.Draw(FarmerRenderer.hairStylesTexture, position + origin + positionOffset + new Vector2((float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * Game1.pixelZoom), (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * Game1.pixelZoom + (!farmer.isMale || farmer.hair < 16 ? (farmer.isMale || farmer.hair >= 16 ? 0 : 4) : -4))), new Microsoft.Xna.Framework.Rectangle?(hairstyleSourceRect), overrideColor.Equals(Color.White) ? farmer.hairstyleColor : overrideColor, rotation, origin, scalar, SpriteEffects.None, layerDepth + 2.2E-05f);

            // Notsure
            sourceRect.Offset((animationFrame.secondaryArm ? 192 : 96) - 288, 0);
            b.Draw(farmerRend.baseTexture, position + origin + positionOffset + farmer.armOffset, new Microsoft.Xna.Framework.Rectangle?(sourceRect), overrideColor, rotation, origin, scalar, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (facingDirection != 0 ? 4.9E-05f : 0.0f));
        }

        public void DrawBuildingTiles(SpriteBatch b, Building building)
        {
            Vector2 vector2 = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize));
            
            if (this.upgradeMode)
            {
                int widthDifference = this.currentBuilding.tilesWide - this.buildingToUpgrade.tilesWide;
                int heightDifference = this.currentBuilding.tilesHigh - this.buildingToUpgrade.tilesHigh;

                if (vector2.X <= this.buildingToUpgrade.tileX - widthDifference)
                    vector2.X = this.buildingToUpgrade.tileX - widthDifference;
                else if (vector2.X >= this.buildingToUpgrade.tileX)
                    vector2.X = this.buildingToUpgrade.tileX;

                if (vector2.Y <= this.buildingToUpgrade.tileY - heightDifference)
                    vector2.Y = this.buildingToUpgrade.tileY - heightDifference;
                else if (vector2.Y >= this.buildingToUpgrade.tileY)
                    vector2.Y = this.buildingToUpgrade.tileY;

                this.currentBuilding.tileX = (int)vector2.X;
                this.currentBuilding.tileY = (int)vector2.Y;
            }

            for (int y = 0; y < building.tilesHigh; ++y)
            {
                for (int x = 0; x < building.tilesWide; ++x)
                {
                    int structurePlacementTile = building.getTileSheetIndexForStructurePlacementTile(x, y);
                    Vector2 tileLocation = new Vector2(vector2.X + (float)x, vector2.Y + (float)y);
                    if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(tileLocation))
                        ++structurePlacementTile;
                    if(this.buildingToUpgrade.tileX <= tileLocation.X && this.buildingToUpgrade.tileX + this.buildingToUpgrade.tilesWide - 1 >= tileLocation.X && this.buildingToUpgrade.tileY <= tileLocation.Y && this.buildingToUpgrade.tileY + this.buildingToUpgrade.tilesHigh - 1 >= tileLocation.Y)
                        b.Draw(StardewValleyCustomMod.CustomTiles, Game1.GlobalToLocal(Game1.viewport, tileLocation * (float)Game1.tileSize), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 115, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.999f);
                    else
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * (float)Game1.tileSize), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + structurePlacementTile * 16, 388, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.999f);
                }
            }
        }

        // TODO add scroll function
        public void DrawDescriptionTab(SpriteBatch b)
        {
            if (this.magicalConstruction)
            {
                Utility.drawTextWithShadow(b, Game1.parseText(this.buildingDescription, Game1.dialogueFont, this.maxWidthOfDescription + Game1.tileSize / 2), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize - Game1.pixelZoom), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4 + Game1.pixelZoom)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0.0f, 3);
                Utility.drawTextWithShadow(b, Game1.parseText(this.buildingDescription, Game1.dialogueFont, this.maxWidthOfDescription + Game1.tileSize / 2), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize - 1), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4 + Game1.pixelZoom)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0.0f, 3);
            } else
                Utility.drawTextWithShadow(b, Game1.parseText(this.buildingDescription, Game1.dialogueFont, this.maxWidthOfDescription + Game1.tileSize / 2), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4)), this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);

            /*//TODO this checks how many lines the description is, use for activation scroll
            string test = Game1.parseText(this.buildingDescription, Game1.dialogueFont, this.maxWidthOfDescription + Game1.tileSize / 2);
            int descriptionTotalLines = test.Split('\n').Length;
            */
        }

        public void DrawDetailsTab(SpriteBatch b)
        {
            // Not sure how accurate these are
            int fontHeight = 32;
            int spaceBetweenFontLines = 16;

            // Make a global for this?
            Texture2D customTiles = StardewValleyCustomMod.CustomTiles;
            
            Vector2 location = new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4));

            // Width
            location.Y += spaceBetweenFontLines;
            b.Draw(customTiles, location, new Microsoft.Xna.Framework.Rectangle(80, 0, 16, 16), Color.White, 0.0f, new Vector2(0.0f, 5.0f), (float)Game1.pixelZoom / 2, SpriteEffects.None, (float)0.0f);
            location.Y -= spaceBetweenFontLines;
            location.X += 16 * Game1.pixelZoom;
            Utility.drawTextWithShadow(b, this.currentBuilding.tilesWide.ToString(), Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            location.X -= 16 * Game1.pixelZoom;

            // Height
            location.X += Game1.tileSize * 7 / 2;//Game1.dialogueFont.MeasureString("Width: " + this.currentBuilding.tilesWide).Length();
            location.Y += spaceBetweenFontLines;
            b.Draw(customTiles, location, new Microsoft.Xna.Framework.Rectangle(64, 0, 16, 16), Color.White, 0.0f, new Vector2(0.0f, 5.0f), (float)Game1.pixelZoom / 2, SpriteEffects.None, (float)0.0f);
            location.Y -= spaceBetweenFontLines;
            location.X += 16 * Game1.pixelZoom;
            Utility.drawTextWithShadow(b, this.currentBuilding.tilesHigh.ToString(), Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            location.X -= 16 * Game1.pixelZoom;

            // TODO what is the second vector for (name is origin)???
            // Example
            //b.Draw(this.texture, this.color * this.alpha, 0.0f, new Vector2(0.0f, (float) this.texture.Bounds.Height), 4f, SpriteEffects.None, (float) ((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000f);

            // Interior Selected
            location.X -= Game1.tileSize * 7 / 2;
            location.Y += Game1.tileSize + spaceBetweenFontLines;
            b.Draw(customTiles, location, new Microsoft.Xna.Framework.Rectangle(32, 0, 25, 18), Color.White, 0.0f, new Vector2(0.0f, 5.0f), (float)Game1.pixelZoom / 2, SpriteEffects.None, (float)0.0f);
            location.Y -= spaceBetweenFontLines;
            location.X += 16 * Game1.pixelZoom;
            if (this.CurrentBlueprint.HasInterior())
                Utility.drawTextWithShadow(b, this.CurrentBlueprint.CurrentInterior.Name, Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            else
                Utility.drawTextWithShadow(b, "No Interior", Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            location.X -= 16 * Game1.pixelZoom;

            // Construction Time
            // Change the '2' to a field: daysToConstruct, Remove "Time" and replace with hourglass icon from cursors TODO
            location.Y += Game1.tileSize + spaceBetweenFontLines;
            b.Draw(customTiles, location, new Microsoft.Xna.Framework.Rectangle(16, 16, 16, 16), Color.White, 0.0f, new Vector2(0.0f, 5.0f), (float)Game1.pixelZoom / 2, SpriteEffects.None, (float)0.0f);
            location.Y -= spaceBetweenFontLines;
            location.X += 16 * Game1.pixelZoom;
            Utility.drawTextWithShadow(b, "2" + " days", Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            location.X -= 16 * Game1.pixelZoom;

            // Load max animal occupants - icon transition between animals?
            if (this.currentBuilding.indoors is AnimalHouse)
            {
                location.Y += Game1.tileSize + spaceBetweenFontLines;
                b.Draw(customTiles, location, new Microsoft.Xna.Framework.Rectangle(0, 65, 17, 13), Color.White, 0.0f, new Vector2(0.0f, 5.0f), (float)Game1.pixelZoom / 2, SpriteEffects.None, (float)0.0f);
                location.Y -= spaceBetweenFontLines;
                location.X += 16 * Game1.pixelZoom;
                Utility.drawTextWithShadow(b, ": " + this.CurrentBlueprint.MaxOccupants, Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
                location.X -= 16 * Game1.pixelZoom;
            }

            // Already Built icon: num (num w/ selected interior)
            int[] buildingCount = this.CurrentBlueprint.GetBuildingCount();
            location.Y += Game1.tileSize + spaceBetweenFontLines;
            b.Draw(customTiles, location, new Microsoft.Xna.Framework.Rectangle(0, 81, 32, 32), Color.White, 0.0f, new Vector2(0.0f, 5.0f), (float)1.0f, SpriteEffects.None, (float)0.0f);
            location.Y -= spaceBetweenFontLines;
            location.X += 16 * Game1.pixelZoom;
            Utility.drawTextWithShadow(b, buildingCount[0] + "(" + buildingCount[1] + " w/ interior)", Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            location.X -= 16 * Game1.pixelZoom;
        }

        public void DrawCostTab(SpriteBatch b)
        {
            //Vector2 location = new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize / 4 + Game1.tileSize), (float)(this.yPositionOnScreen + Game1.tileSize * 4 + Game1.tileSize / 2));
            Vector2 location = new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize / 4 + Game1.tileSize), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4));
            SpriteText.drawString(b, "$", (int)location.X, (int)location.Y, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1);
            if (this.magicalConstruction)
            {
                Utility.drawTextWithShadow(b, this.price.ToString() + "g", Game1.dialogueFont, new Vector2(location.X + (float)Game1.tileSize, location.Y + (float)(Game1.pixelZoom * 2)), Game1.textColor * 0.5f, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
                Utility.drawTextWithShadow(b, this.price.ToString() + "g", Game1.dialogueFont, new Vector2((float)((double)location.X + (double)Game1.tileSize + (double)Game1.pixelZoom - 1.0), location.Y + (float)(Game1.pixelZoom * 2)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            }
            Utility.drawTextWithShadow(b, this.price.ToString() + "g", Game1.dialogueFont, new Vector2(location.X + (float)Game1.tileSize + (float)Game1.pixelZoom, location.Y + (float)Game1.pixelZoom), Game1.player.money >= this.price ? (this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor) : Color.Red, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            location.X -= (float)(Game1.tileSize / 4);
            location.Y -= (float)(Game1.tileSize / 3);
            foreach (Item ingredient in this.ingredients)
            {
                location.Y += (float)(Game1.tileSize + Game1.pixelZoom);
                ingredient.drawInMenu(b, location, 1f);
                bool flag = !(ingredient is StardewValley.Object) || Game1.player.hasItemInInventory((ingredient as StardewValley.Object).parentSheetIndex, ingredient.Stack, 0);
                if (this.magicalConstruction)
                {
                    Utility.drawTextWithShadow(b, ingredient.Name, Game1.dialogueFont, new Vector2(location.X + (float)Game1.tileSize + (float)(Game1.pixelZoom * 3), location.Y + (float)(Game1.pixelZoom * 6)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
                    Utility.drawTextWithShadow(b, ingredient.Name, Game1.dialogueFont, new Vector2((float)((double)location.X + (double)Game1.tileSize + (double)(Game1.pixelZoom * 4) - 1.0), location.Y + (float)(Game1.pixelZoom * 6)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
                }
                Utility.drawTextWithShadow(b, ingredient.Name, Game1.dialogueFont, new Vector2(location.X + (float)Game1.tileSize + (float)(Game1.pixelZoom * 4), location.Y + (float)(Game1.pixelZoom * 5)), flag ? (this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor) : Color.Red, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
            }
        }

        public void DrawInterior(SpriteBatch b)
        {
            string interiorOption = "Select Interior";
            string currentInteriorName = this.CurrentBlueprint.CurrentInterior.Name;
            int nameWidth = SpriteText.getWidthOfString(currentInteriorName);
            int xCoord = Game1.viewport.Width / 2;
            int yCoord = Game1.tileSize / 4;

            SpriteText.drawStringWithScrollBackground(b, interiorOption, xCoord - SpriteText.getWidthOfString(interiorOption) / 2, yCoord, "", 1f, -1);
            SpriteText.drawStringWithScrollBackground(b, currentInteriorName, xCoord - nameWidth / 2, Game1.viewport.Height - yCoord * Game1.pixelZoom * 2, "", 1f, -1);
            //SpriteText.drawString(b, currentInteriorName, xCoord - nameWidth / 2, Game1.viewport.Height - yCoord * Game1.pixelZoom * 2, -1);
            //this.backButton.bounds.X = Game1.viewport.Width / 2 -

            // Update Button Locations
            this.backButton.bounds.X = xCoord - nameWidth / 2 - Game1.tileSize;
            this.backButton.bounds.Y = Game1.viewport.Height - yCoord * Game1.pixelZoom * 2;
            this.forwardButton.bounds.X = xCoord + nameWidth / 2 + Game1.tileSize;
            this.forwardButton.bounds.Y = Game1.viewport.Height - yCoord * Game1.pixelZoom * 2;

            // Draw Buttons
            // TODO only draw when there is more than 1 interior available, maybe still draw the ok button?
            this.backButton.draw(b);
            this.forwardButton.draw(b);
            this.okButton.draw(b);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public CustomBuildingBlueprint CurrentBlueprint
        {
            get
            {
                return this.blueprints[this.currentBlueprintIndex];
            }
        }

        private Building GetBuildingFromBlueprint(CustomBuildingBlueprint blueprint)
        {
            // Dummy location
            GameLocation blueprintIndoorLocation = (GameLocation)new Shed(Game1.content.Load<Map>("Maps\\" + "Shed"), "Shed");

            //blueprint.UpdateCurrentInterior(); // TODO better spot for this?

            blueprint.LoadCustomBuildingBlueprint(); // TODO better spot for this?

            // InteriorName or InteriorFileName???
            if (blueprint.Interiors.Count > 0)
            {
                if (blueprint.CurrentInterior.Name != null && blueprint.CurrentInterior.Name.Length > 0 && !blueprint.CurrentInterior.Name.Equals("null"))
                {
                    if (this.debug)
                        this.Logger.Log("mapToWarpTo is not null!");
                    //blueprintIndoorLocation = this.GetGameLocationFromBlueprint(blueprint);
                    blueprintIndoorLocation = blueprint.GetIndoors();
                }
            }
            else
            {
                StardewValleyCustomMod.Debug.DebugCustomBlueprintValues(blueprint);
                //blueprintIndoorLocation = null;
                return new Building(blueprint.ModName + "_" + blueprint.BuildingName, "null", (int)Vector2.Zero.X, (int)Vector2.Zero.Y,
                blueprint.TilesWidth, blueprint.TilesHeight, blueprint.HumanDoorTileCoord, blueprint.AnimalDoorTileCoord,
                blueprintIndoorLocation, blueprint.texture, blueprint.Magical, 0);
            }

            //if (this.debug)
                //StardewValleyCustomMod.Debug.DebugBlueprintDetails(blueprint, blueprintIndoorLocation);

            return new Building(blueprint.ModName + "_" + blueprint.BuildingName, blueprint.CurrentInterior.Name, (int)Vector2.Zero.X, (int)Vector2.Zero.Y,
                blueprint.TilesWidth, blueprint.TilesHeight, blueprint.HumanDoorTileCoord, blueprint.AnimalDoorTileCoord,
                blueprintIndoorLocation, blueprint.texture, blueprint.Magical, 0);
        }

        // TODO put in config? or somewhere else? used multiple times?
        // TODO NOT USED ANYMORE
        private GameLocation GetGameLocationFromBlueprint(BluePrint blueprint)
        {
            if (this.debug)
            {
                this.Logger.Log($"Getting location for: {blueprint.name}");
                this.Logger.Log($"Location is: {blueprint.mapToWarpTo}");
            }

            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.BlueprintList)
            {
                if (blu.BuildingName.Equals(blueprint.name))
                {
                    return blu.GetIndoors();
                }
            }

            return new GameLocation(this.content.Load<Map>("BuildingInterior\\" + blueprint.mapToWarpTo), blueprint.mapToWarpTo);
        }

        // Display arrows on bottom (or top?) with name of interior between them?
        private void DisplayInterior()
        {
            // Update current location:
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation = this.CurrentBlueprint.GetIndoors();
            Game1.currentLocation.resetForPlayerEntry();
            Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);

            if (!interior)
            {
                StardewValleyCustomMod.Logger.Log($"Initial Interior Load...");

                this.hoverText = ""; // TODO outside of if or in here?

                // Update button locations:
                this.cancelButton.bounds.X = Game1.viewport.Width - Game1.tileSize * 2;
                this.cancelButton.bounds.Y = Game1.viewport.Height - Game1.tileSize * 2;
                this.okButton.bounds.X = Game1.viewport.Width - Game1.tileSize * 4;
                this.okButton.bounds.Y = Game1.viewport.Height - Game1.tileSize * 2;

                this.interior = true;

                // Change screen settings:
                Game1.displayHUD = false;
                Game1.viewportFreeze = true;
                
                this.drawBG = false;
                this.freeze = false;
                Game1.displayFarmer = false;

                Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f); // TODO do I want this every time or just on menu to interior transition?
            }            

            // TODO Remove it after you go back to menu
            // TODO make sure the location is not duped
            StardewValleyCustomMod.Debug.DebugGame1LocationsList(Game1.locations);
            this.Logger.Log($"currentLocation: {Game1.currentLocation.name}");

            Game1.viewport.Location = new Location(49 * Game1.tileSize, 5 * Game1.tileSize); // ???
            Game1.panScreen(0, 0);
        }

        /*
        public virtual void drawInteriorInMenu(SpriteBatch b, int x, int y)
        {
            if (this.tilesWide <= 8)
            {
                this.drawShadow(b, x, y);
                b.Draw(this.texture, new Vector2((float)x, (float)y), new Rectangle?(this.texture.Bounds), this.color, 0.0f, new Vector2(0.0f, 0.0f), (float)Game1.pixelZoom, SpriteEffects.None, 0.89f);
            }
            else
            {
                int num1 = Game1.tileSize + 11 * Game1.pixelZoom;
                int num2 = Game1.tileSize / 2 - Game1.pixelZoom;
                b.Draw(this.texture, new Vector2((float)(x + num1), (float)(y + num2)), new Rectangle?(new Rectangle(this.texture.Bounds.Center.X - 64, this.texture.Bounds.Bottom - 136 - 2, 122, 138)), this.color, 0.0f, new Vector2(0.0f, 0.0f), (float)Game1.pixelZoom, SpriteEffects.None, 0.89f);
            }
        }*/
        
        // TODO make this more effecient
        private void UpdateCurrentTabSelected()
        {
            int noIndentTab = this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder / 2;
            int indent = IClickableMenu.spaceToClearSideBorder / 2;

            // Reset all tabs with no indent
            this.descriptionTab.bounds.X = noIndentTab;
            this.detailsTab.bounds.X = noIndentTab;
            this.costTab.bounds.X = noIndentTab;

            // Indent Current Tab
            // Description Tab
            if (this.currentTab == 0)
            {
                this.descriptionTab.bounds.X -= indent;
            }
            // Details Tab
            else if(this.currentTab == 1)
            {
                this.detailsTab.bounds.X -= indent;
            }
            // Cost Tab
            else if(this.currentTab == 2)
            {
                this.costTab.bounds.X -= indent;
            }
            else
            {
                StardewValleyCustomMod.Logger.Log($"Error - invalid tab selected: {this.currentTab}");
            }
        }
    }
}
