// Decompiled with JetBrains decompiler
// Type: StardewValley.Menus.CarpenterMenu
// Assembly: Stardew Valley, Version=1.0.6124.25603, Culture=neutral, PublicKeyToken=null
// MVID: 8735DDC4-C499-43F5-9D59-831F1FFC73CF
// Assembly location: E:\SteamLibrary\steamapps\common\Stardew Valley\Stardew Valley.exe

/* TODO
 * Buttons on the side:
 *  Description
 *  Cost
 *  Stats? - How many built on farm, if animals how many on farm?
 *  Select Interior
 *  Details? - includes description, size, interior selected, days to construct
 *  
 * Interior options
 *  player can select what interior they want
 *  Select Interior Button
 * 
 * Exterior options?
 * 
 * View interiors other rooms/floors???
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
        private List<BluePrint> blueprints;
        private int currentBlueprintIndex;
        private ClickableTextureComponent okButton;
        private ClickableTextureComponent cancelButton;
        private ClickableTextureComponent backButton;
        private ClickableTextureComponent forwardButton;
        private ClickableTextureComponent upgradeIcon;
        private ClickableTextureComponent demolishButton;
        private ClickableTextureComponent moveButton;
        private ClickableTextureComponent detailsTab;
        private ClickableTextureComponent costTab;
        private ClickableTextureComponent interiorButton;
        private Building currentBuilding;
        private Building buildingToMove;
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

        public CustomBuildingsMenu()
        {
            Game1.player.forceCanMove();
            this.resetBounds();
            this.shopLocation = Game1.currentLocation;
            this.content = new LocalizedContentManager(Game1.content.ServiceProvider, "Mods\\StardewValleyCustomMod\\CustomBuildings");
            this.blueprints = new List<BluePrint>();
            this.Logger = StardewValleyCustomMod.Logger;
            this.debug = StardewValleyCustomMod.Config.Debug;
            this.currentTab = 0;
            this.interior = false;

            this.Logger.Log("Loading Crafting Menu...");
            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.BlueprintList)
            {
                this.blueprints.Add(blu.convertCustomBlueprintToBluePrint());
                if (this.debug)
                    this.Logger.Log($"{blu.name} custom blueprint has been converted.");
            }

            this.magicalConstruction = false; //REMOVE
            this.setNewActiveBlueprint();
        }

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
            this.detailsTab = new ClickableTextureComponent("Details", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + IClickableMenu.spaceToClearTopBorder, Game1.tileSize, Game1.tileSize), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(16, 0, 16, 16), (float)Game1.pixelZoom, false);
            this.costTab = new ClickableTextureComponent("Cost", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder / 2 /*- Game1.tileSize*/, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.tileSize + IClickableMenu.spaceToClearTopBorder, Game1.tileSize, Game1.tileSize), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), (float)Game1.pixelZoom, false);
            this.interiorButton = new ClickableTextureComponent("Select Interior", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth, this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.tileSize * 2 + Game1.pixelZoom*4 + IClickableMenu.spaceToClearTopBorder, 25 * Game1.pixelZoom, 18 * Game1.pixelZoom), (string)null, (string)null, customTiles, new Microsoft.Xna.Framework.Rectangle(32, 0, 25, 18), (float)Game1.pixelZoom, false);

            this.DrawCoords();
        }

        public void setNewActiveBlueprint()
        {
            this.Logger.Log($"CurrentBlueprintIndex: {this.currentBlueprintIndex}"); // DEBUG REMOVE
            //this.currentBuilding = new Building(this.blueprints[this.currentBlueprintIndex], Vector2.Zero);
            this.currentBuilding = this.GetBuildingFromBlueprint(this.CurrentBlueprint);
            this.price = this.blueprints[this.currentBlueprintIndex].moneyRequired;
            this.ingredients.Clear();
            foreach (KeyValuePair<int, int> keyValuePair in this.blueprints[this.currentBlueprintIndex].itemsRequired)
                this.ingredients.Add((Item)new StardewValley.Object(keyValuePair.Key, keyValuePair.Value, false, -1, 0));
            this.buildingDescription = this.blueprints[this.currentBlueprintIndex].description;
            this.buildingName = this.blueprints[this.currentBlueprintIndex].name;
            this.Logger.Log($"CurrentBuildingName: {this.buildingName}"); // DEBUG REMOVE
        }

        public override void performHoverAction(int x, int y)
        {
            this.cancelButton.tryHover(x, y, 0.1f);
            base.performHoverAction(x, y);
            if (!this.onFarm)
            {
                // Hover Icon
                this.backButton.tryHover(x, y, 1f);
                this.forwardButton.tryHover(x, y, 1f);
                this.okButton.tryHover(x, y, 0.1f);
                this.demolishButton.tryHover(x, y, 0.1f);
                this.moveButton.tryHover(x, y, 0.1f);
                //this.detailsButton.tryHover(x, y, 0.1f);
                //this.costButton.tryHover(x, y, 0.1f);
                this.interiorButton.tryHover(x, y, 0.1f);

                // Hover Text
                if (this.CurrentBlueprint.isUpgrade() && this.upgradeIcon.containsPoint(x, y))
                    this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Upgrade", (object)this.CurrentBlueprint.nameOfBuildingToUpgrade);
                else if (this.demolishButton.containsPoint(x, y))
                    this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Demolish");
                else if (this.moveButton.containsPoint(x, y))
                    this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings");
                else if (this.okButton.containsPoint(x, y) && this.CurrentBlueprint.doesFarmerHaveEnoughResourcesToBuild())
                    this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Build");
                else if (this.detailsTab.containsPoint(x, y))
                    this.hoverText = "Details";
                else if (this.costTab.containsPoint(x, y))
                    this.hoverText = "Cost";
                else if (this.interiorButton.containsPoint(x, y))
                    this.hoverText = "Select Interior";
                else
                    this.hoverText = "";
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
                    if (building1 != null && this.CurrentBlueprint.nameOfBuildingToUpgrade != null && this.CurrentBlueprint.nameOfBuildingToUpgrade.Equals(building1.buildingType))
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
            if (!this.onFarm || Game1.globalFade)
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
            if (!this.onFarm)
                base.receiveLeftClick(x, y, playSound);

            // Check buttons and tabs:
            if (this.cancelButton.containsPoint(x, y))
            {
                if (!this.onFarm)
                {
                    this.exitThisMenu(true);
                    Game1.player.forceCanMove();
                    Game1.playSound("bigDeSelect");
                }
                else
                {
                    if (this.moving && this.buildingToMove != null)
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                    if (this.interior)
                        this.interior = false;
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.returnToCarpentryMenu), 0.02f);
                    Game1.playSound("smallSelect");
                    return;
                }
            }
            if (!this.onFarm && this.backButton.containsPoint(x, y))
            {
                this.currentBlueprintIndex = this.currentBlueprintIndex - 1;
                if (this.currentBlueprintIndex < 0)
                    this.currentBlueprintIndex = this.blueprints.Count - 1;
                this.setNewActiveBlueprint();
                Game1.playSound("shwip");
                this.backButton.scale = this.backButton.baseScale;
            }
            if (!this.onFarm && this.forwardButton.containsPoint(x, y))
            {
                this.currentBlueprintIndex = (this.currentBlueprintIndex + 1) % this.blueprints.Count;
                this.setNewActiveBlueprint();
                this.backButton.scale = this.backButton.baseScale;
                Game1.playSound("shwip");
            }
            if (!this.onFarm && this.demolishButton.containsPoint(x, y))
            {
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
                Game1.playSound("smallSelect");
                this.onFarm = true;
                this.demolishing = true;
            }
            if (!this.onFarm && this.moveButton.containsPoint(x, y))
            {
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
                Game1.playSound("smallSelect");
                this.onFarm = true;
                this.moving = true;
            }
            if (this.okButton.containsPoint(x, y) && !this.onFarm && (Game1.player.money >= this.price && this.blueprints[this.currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild()))
            {
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
                Game1.playSound("smallSelect");
                this.onFarm = true;
            }
            if (!this.onFarm && this.detailsTab.containsPoint(x, y))
            {
                Game1.playSound("smallSelect");
                this.currentTab = 0;
                this.UpdateCurrentTabSelected();
            }
            if (!this.onFarm && this.costTab.containsPoint(x, y))
            {
                Game1.playSound("smallSelect");
                this.currentTab = 1;
                this.UpdateCurrentTabSelected();
            }
            if (!this.onFarm && this.interiorButton.containsPoint(x, y))
            {
                Game1.playSound("smallSelect");
                this.interior = true;
                this.DisplayInterior();
            }

            // TODO Do not need this if once you make the methods for the if statements below
            if (!this.onFarm || this.freeze || Game1.globalFade)
                return;

            // Update current option selected:
            // TODO separate into a method for each if and put in the if statements above
            if (this.demolishing)
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
            else if (this.upgrading)
            {
                Building buildingAt = ((BuildableGameLocation)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize)));
                if (buildingAt != null && this.CurrentBlueprint.name != null && buildingAt.buildingType.Equals(this.CurrentBlueprint.nameOfBuildingToUpgrade))
                {
                    this.CurrentBlueprint.consumeResources();
                    buildingAt.daysUntilUpgrade = 2;
                    buildingAt.showUpgradeAnimation((GameLocation)Game1.getFarm());
                    Game1.playSound("axe");
                    DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(this.returnToCarpentryMenuAfterSuccessfulBuild), 1500);
                    this.freeze = true;
                }
                else
                {
                    if (buildingAt == null)
                        return;
                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType"), Color.Red, 3500f));
                }
            }
            else if (this.moving)
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
            else if (this.tryToBuild())
            {
                this.Logger.Log($"Successfully starting building the {currentBuilding.buildingType}"); // DEBUG REMOVE
                this.CurrentBlueprint.consumeResources();
                DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(this.returnToCarpentryMenuAfterSuccessfulBuild), 2000);
                this.freeze = true;
            }
            else
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), Color.Red, 3500f));
        }

        public bool tryToBuild()
        {
            bool built = ((BuildableGameLocation)Game1.getLocationFromName("Farm")).buildStructure(this.currentBuilding, new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize)), false, Game1.player);
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
            if (this.magicalConstruction)
                return;
            string path = "Data\\ExtraDialogue:Robin_" + (this.upgrading ? "Upgrade" : "New") + "Construction";
            if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
                path += "_Festival";
            Game1.drawDialogue(Game1.getCharacterFromName("Robin", false), Game1.content.LoadString(path, (object)this.CurrentBlueprint.name.ToLower(), (object)((IEnumerable<string>)this.CurrentBlueprint.name.ToLower().Split(' ')).Last<string>()));
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
            if (this.demolishing || this.CurrentBlueprint.nameOfBuildingToUpgrade == null || (this.CurrentBlueprint.nameOfBuildingToUpgrade.Length <= 0 || this.moving))
                return;
            this.upgrading = true;
        }

        public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
        {
            this.resetBounds();
        }

        public override void draw(SpriteBatch b)
        {
            if (this.drawBG)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            if (Game1.globalFade || this.freeze)
                return;
            if (!this.onFarm)
            {
                base.draw(b);
                IClickableMenu.drawTextureBox(b, this.xPositionOnScreen - Game1.tileSize * 3 / 2, this.yPositionOnScreen - Game1.tileSize / 4, this.maxWidthOfBuildingViewer + Game1.tileSize, this.maxHeightOfBuildingViewer + Game1.tileSize, this.magicalConstruction ? Color.RoyalBlue : Color.White);

                // TODO do I still need this if else or do I just need that else?
                if (this.interior)
                {
                    //drawInteriorInMenu(b, this.xPositionOnScreen + this.maxWidthOfBuildingViewer / 2 - this.currentBuilding.tilesWide * Game1.tileSize / 2 - Game1.tileSize, this.yPositionOnScreen + this.maxHeightOfBuildingViewer / 2 - this.currentBuilding.getSourceRectForMenu().Height * Game1.pixelZoom / 2);
                }
                else
                {
                    this.currentBuilding.drawInMenu(b, this.xPositionOnScreen + this.maxWidthOfBuildingViewer / 2 - this.currentBuilding.tilesWide * Game1.tileSize / 2 - Game1.tileSize, this.yPositionOnScreen + this.maxHeightOfBuildingViewer / 2 - this.currentBuilding.getSourceRectForMenu().Height * Game1.pixelZoom / 2);

                }
                
                // Should this be displayed with exterior only?
                if (this.CurrentBlueprint.isUpgrade())
                    this.upgradeIcon.draw(b);


                SpriteText.drawStringWithScrollBackground(b, this.buildingName, this.xPositionOnScreen + this.maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder - Game1.tileSize / 4 + Game1.tileSize + ((this.width - (this.maxWidthOfBuildingViewer + Game1.tileSize * 2)) / 2 - SpriteText.getWidthOfString("Deluxe Barn") / 2), this.yPositionOnScreen, "Deluxe Barn", 1f, -1);
                IClickableMenu.drawTextureBox(b, this.xPositionOnScreen + this.maxWidthOfBuildingViewer - Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize * 5 / 4, this.maxWidthOfDescription + Game1.tileSize, this.maxWidthOfDescription + Game1.tileSize * 3 / 2, this.magicalConstruction ? Color.RoyalBlue : Color.White);

                // Draw Details Tab:
                if(this.currentTab == 0)
                {
                    if (this.magicalConstruction)
                    {
                        Utility.drawTextWithShadow(b, Game1.parseText(this.buildingDescription, Game1.dialogueFont, this.maxWidthOfDescription + Game1.tileSize / 2), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize - Game1.pixelZoom), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4 + Game1.pixelZoom)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0.0f, 3);
                        Utility.drawTextWithShadow(b, Game1.parseText(this.buildingDescription, Game1.dialogueFont, this.maxWidthOfDescription + Game1.tileSize / 2), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize - 1), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4 + Game1.pixelZoom)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0.0f, 3);
                    }
                    Utility.drawTextWithShadow(b, Game1.parseText(this.buildingDescription, Game1.dialogueFont, this.maxWidthOfDescription + Game1.tileSize / 2), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4)), this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);

                    string test = Game1.parseText(this.buildingDescription, Game1.dialogueFont, this.maxWidthOfDescription + Game1.tileSize / 2);
                    int descriptionTotalLines = test.Split('\n').Length;
                    
                    // Need to figure out the height of base game font... TODO
                    Vector2 location = new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfDescription + Game1.tileSize), (float)(this.yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom * 4 + (20 + Game1.pixelZoom) * 2 * descriptionTotalLines));

                    Utility.drawTextWithShadow(b, "Width: " + this.currentBuilding.tilesWide, Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);

                    location.X += Game1.tileSize * 7 / 2;//Game1.dialogueFont.MeasureString("Width: " + this.currentBuilding.tilesWide).Length();
                    Utility.drawTextWithShadow(b, "Height: " + this.currentBuilding.tilesHigh, Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);

                    location.X -= Game1.tileSize * 7 / 2;
                    location.Y += Game1.tileSize * 2 / 3;
                    Utility.drawTextWithShadow(b, "Interior: " + this.currentBuilding.nameOfIndoorsWithoutUnique, Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);

                    // Change the '2' to a field: daysToConstruct, Remove "Time" and replace with hourglass icon from cursors TODO
                    location.Y += Game1.tileSize * 2 / 3;
                    Utility.drawTextWithShadow(b, "Time: " + "2" + " days", Game1.dialogueFont, location, this.magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, this.magicalConstruction ? 0.0f : 0.25f, 3);
                }

                // Draw Cost Tab:
                else if (this.currentTab == 1)
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
                
                // Draw Buttons and Tabs:
                this.backButton.draw(b);
                this.forwardButton.draw(b);
                this.okButton.draw(b, this.blueprints[this.currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild() ? Color.White : Color.Gray * 0.8f, 0.88f);
                this.demolishButton.draw(b);
                this.moveButton.draw(b);
                this.detailsTab.draw(b);
                this.costTab.draw(b);
                this.interiorButton.draw(b);
            }
            // Draw Interior Selection Screen:
            else if (this.interior)
            {
                string interiorOption = "Select Interior";
                SpriteText.drawStringWithScrollBackground(b, interiorOption, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(interiorOption) / 2, Game1.tileSize / 4, "", 1f, -1);
            }
            else
            {
                string str;
                if (!this.upgrading)
                    str = this.demolishing ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish") : Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation");
                else
                    str = Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", (object)this.CurrentBlueprint.nameOfBuildingToUpgrade);
                string s = str;
                SpriteText.drawStringWithScrollBackground(b, s, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, Game1.tileSize / 4, "", 1f, -1);
                if (!this.upgrading && !this.demolishing && !this.moving)
                {
                    Vector2 vector2 = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize));
                    for (int y = 0; y < this.CurrentBlueprint.tilesHeight; ++y)
                    {
                        for (int x = 0; x < this.CurrentBlueprint.tilesWidth; ++x)
                        {
                            int structurePlacementTile = this.CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x, y);
                            Vector2 tileLocation = new Vector2(vector2.X + (float)x, vector2.Y + (float)y);
                            if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(tileLocation))
                                ++structurePlacementTile;
                            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * (float)Game1.tileSize), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + structurePlacementTile * 16, 388, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.999f);
                        }
                    }
                }
                else if (this.moving && this.buildingToMove != null)
                {
                    Vector2 vector2 = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX()) / Game1.tileSize), (float)((Game1.viewport.Y + Game1.getOldMouseY()) / Game1.tileSize));
                    for (int y = 0; y < this.buildingToMove.tilesHigh; ++y)
                    {
                        for (int x = 0; x < this.buildingToMove.tilesWide; ++x)
                        {
                            int structurePlacementTile = this.buildingToMove.getTileSheetIndexForStructurePlacementTile(x, y);
                            Vector2 tileLocation = new Vector2(vector2.X + (float)x, vector2.Y + (float)y);
                            if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(tileLocation))
                                ++structurePlacementTile;
                            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, tileLocation * (float)Game1.tileSize), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + structurePlacementTile * 16, 388, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.999f);
                        }
                    }
                }
            }
            this.cancelButton.draw(b);
            this.drawMouse(b);
            if (this.hoverText.Length <= 0)
                return;
            IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public BluePrint CurrentBlueprint
        {
            get
            {
                return this.blueprints[this.currentBlueprintIndex];
            }
        }

        private Building GetBuildingFromBlueprint(BluePrint blueprint)
        {
            // Dummy location
            GameLocation blueprintIndoorLocation = (GameLocation)new Shed(Game1.content.Load<Map>("Maps\\" + "Shed"), "Shed");

            if (blueprint.mapToWarpTo != null && blueprint.mapToWarpTo.Length > 0 && !blueprint.mapToWarpTo.Equals("null"))
            {
                if (this.debug)
                    this.Logger.Log("mapToWarpTo is not null!");
                blueprintIndoorLocation = this.GetGameLocationFromBlueprint(blueprint);
            }

            if (this.debug)
                StardewValleyCustomMod.Debug.DebugBlueprintDetails(blueprint, blueprintIndoorLocation);

            return new Building(blueprint.name, blueprint.mapToWarpTo, (int)Vector2.Zero.X, (int)Vector2.Zero.Y,
                blueprint.tilesWidth, blueprint.tilesHeight, blueprint.humanDoor, blueprint.animalDoor,
                blueprintIndoorLocation, blueprint.texture, blueprint.magical, 0);
        }

        // TODO put in config? or somewhere else? used multiple times?
        private GameLocation GetGameLocationFromBlueprint(BluePrint blueprint)
        {
            if (this.debug)
            {
                this.Logger.Log($"Getting location for: {blueprint.name}");
                this.Logger.Log($"Location is: {blueprint.mapToWarpTo}");
            }

            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.BlueprintList)
            {
                if (blu.name.Equals(blueprint.name))
                {
                    return blu.GetIndoors();
                }
            }

            return new GameLocation(this.content.Load<Map>("BuildingInterior\\" + blueprint.mapToWarpTo), blueprint.mapToWarpTo);
        }

        // Display arrows on bottom (or top?) with name of interior between them?
        private void DisplayInterior()
        {
            GameLocation buildingInterior = GetGameLocationFromBlueprint(this.CurrentBlueprint);
            this.Logger.Log($"buildingInterior: {buildingInterior.name}");
            /*
            if (buildingInterior.IsFarm && buildingInterior.terrainFeatures == null)
                buildingInterior.terrainFeatures = new SerializableDictionary<Vector2, TerrainFeature>();
            foreach (NPC character in buildingInterior.characters)
                character.reloadSprite();
            foreach (TerrainFeature terrainFeature in buildingInterior.terrainFeatures.Values)
                terrainFeature.loadSprite();
            foreach (KeyValuePair<Vector2, StardewValley.Object> keyValuePair in (Dictionary<Vector2, StardewValley.Object>)buildingInterior.objects)
            {
                keyValuePair.Value.initializeLightSource(keyValuePair.Key);
                keyValuePair.Value.reloadSprite();
            }
            */
            //GameLocation test = new GameLocation(Game1.content.Load<Map>(StardewValleyCustomMod.ModPath + "\\CustomBuildings\\BuildingInterior\\WineryInterior"), "WineryInterior");
            /*
            GameLocation loc;
            string str = StardewValleyCustomMod.ModPath + "\\CustomBuildings\\BuildingInterior\\WineryInterior";
            StardewValleyCustomMod.ContentRegistry.RegisterXnb(str, str);
            xTile.Map map = Game1.content.Load<xTile.Map>(str);
            loc = new StardewValley.Locations.Cellar(map, "WineryInteriorTest");
            loc.objects = new SerializableDictionary<Microsoft.Xna.Framework.Vector2, StardewValley.Object>();
            loc.isOutdoors = false;
            loc.isFarm = true;
            Game1.locations.Add(loc);
            */

            Game1.currentLocation.cleanupBeforePlayerExit();
            this.hoverText = "";
            //Game1.locations.Add(buildingInterior);
            //Game1.currentLocation = Game1.getLocationFromName(buildingInterior.name);
            Game1.currentLocation = buildingInterior;
            //Game1.currentLocation = Game1.getLocationFromName("WineryInteriorTest");
            this.Logger.Log($"currentLocation: {Game1.currentLocation.name}");
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
            // Check exterior and load its interiors
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
            // Details Tab
            if(this.currentTab == 0)
            {
                this.detailsTab.bounds.X = this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder;
                this.costTab.bounds.X = this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder / 2;
            }
            // Cost Tab
            else
            {
                this.detailsTab.bounds.X = this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder / 2;
                this.costTab.bounds.X = this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder;
            }
        }
    }
}
