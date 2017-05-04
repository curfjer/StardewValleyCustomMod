// Decompiled with JetBrains decompiler
// Type: StardewValley.BluePrint
// Assembly: Stardew Valley, Version=1.0.6124.25603, Culture=neutral, PublicKeyToken=null
// MVID: 8735DDC4-C499-43F5-9D59-831F1FFC73CF
// Assembly location: E:\SteamLibrary\steamapps\common\Stardew Valley\Stardew Valley.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using StardewValley;
using StardewModdingAPI;

namespace StardewValleyCustomMod.CustomBlueprints
{
    public class CustomBuildingBlueprint
    {
        public List<string> namesOfOkayBuildingLocations = new List<string>();
        public Dictionary<int, int> itemsRequired = new Dictionary<int, int>();
        public string name;
        public int woodRequired;
        public int stoneRequired;
        public int copperRequired;
        public int IronRequired;
        public int GoldRequired;
        public int IridiumRequired;
        public int tilesWidth;
        public int tilesHeight;
        public int maxOccupants;
        public int moneyRequired;
        public Point humanDoor;
        public Point animalDoor;
        public string mapToWarpTo;
        public string description;
        public string blueprintType;
        public string nameOfBuildingToUpgrade;
        public string actionBehavior;
        public Texture2D texture;
        public Rectangle sourceRectForMenuView;
        public bool canBuildOnCurrentMap;
        public bool magical;
        public IMonitor monitor;
        private LocalizedContentManager content;

        public CustomBuildingBlueprint(string name, IMonitor monitor)
        {
            this.monitor = monitor;
            this.content = new LocalizedContentManager(Game1.content.ServiceProvider, "Mods\\StardewValleyCustomMod\\CustomBuildings");
            this.name = name;
            this.monitor.Log($"Filepath for Game1 is:{Game1.content.RootDirectory}"); // DEBUG REMOVE
            this.monitor.Log($"Filepath for mod is:{this.content.RootDirectory}"); // DEBUG REMOVE
            if (name.Equals("Info Tool"))
            {
                this.texture = Game1.content.Load<Texture2D>("LooseSprites\\Cursors");
                this.description = "Use to see information about your animals.";
                this.sourceRectForMenuView = new Rectangle(9 * Game1.tileSize, 0, Game1.tileSize, Game1.tileSize);
            }
            else
            {
                Dictionary<string, string> dictionary = content.Load<Dictionary<string, string>>("Blueprints");
                this.monitor.Log("Obtained blueprint list!"); // DEBUG REMOVE
                string str1 = (string)null;
                string key = name;
                // ISSUE: explicit reference operation
                // ISSUE: variable of a reference type
                string local = @str1;
                this.monitor.Log($"str1 Local: {local}"); // DEBUG REMOVE
                dictionary.TryGetValue(key, out local);
                this.monitor.Log($"Dict Local: {local}"); // DEBUG REMOVE
                str1 = local; //Is this redundant? Why was str1 and local needed in original code?
                if (str1 == null)
                    return;
                this.monitor.Log("Parsing File!"); // DEBUG REMOVE
                string[] strArray1 = str1.Split('/');
                if (strArray1[0].Equals("animal"))
                {
                    try
                    {
                        this.texture = Game1.content.Load<Texture2D>("Animals\\" + (name.Equals("Chicken") ? "White Chicken" : name));
                    }
                    catch (Exception ex)
                    {
                        Game1.debugOutput = "Blueprint loaded with no texture!";
                    }
                    this.moneyRequired = Convert.ToInt32(strArray1[1]);
                    this.sourceRectForMenuView = new Rectangle(0, 0, Convert.ToInt32(strArray1[2]), Convert.ToInt32(strArray1[3]));
                    this.blueprintType = "Animals";
                    this.tilesWidth = 1;
                    this.tilesHeight = 1;
                    this.description = strArray1[4];
                    this.humanDoor = new Point(-1, -1);
                    this.animalDoor = new Point(-1, -1);
                }
                else
                {
                    try
                    {
                        this.texture = content.Load<Texture2D>(name);
                        this.monitor.Log($"The building blueprint for {name} loaded with texture!"); //DEBUG REMOVE
                    }
                    catch (Exception ex)
                    {
                        this.monitor.Log($"The building blueprint for {name} loaded with no texture!");
                    }
                    this.monitor.Log("Parsing Building!"); // DEBUG REMOVE
                    string[] strArray2 = strArray1[0].Split(' ');
                    int index = 0;
                    while (index < strArray2.Length)
                    {
                        if (!strArray2[index].Equals(""))
                            this.itemsRequired.Add(Convert.ToInt32(strArray2[index]), Convert.ToInt32(strArray2[index + 1]));
                        index += 2;
                    }
                    this.tilesWidth = Convert.ToInt32(strArray1[1]);
                    this.tilesHeight = Convert.ToInt32(strArray1[2]);
                    this.humanDoor = new Point(Convert.ToInt32(strArray1[3]), Convert.ToInt32(strArray1[4]));
                    this.animalDoor = new Point(Convert.ToInt32(strArray1[5]), Convert.ToInt32(strArray1[6]));
                    this.mapToWarpTo = strArray1[7];
                    this.description = strArray1[8];
                    this.blueprintType = strArray1[9];
                    if (this.blueprintType.Equals("Upgrades"))
                        this.nameOfBuildingToUpgrade = strArray1[10];
                    this.sourceRectForMenuView = new Rectangle(0, 0, Convert.ToInt32(strArray1[11]), Convert.ToInt32(strArray1[12]));
                    this.maxOccupants = Convert.ToInt32(strArray1[13]);
                    this.actionBehavior = strArray1[14];
                    string str2 = strArray1[15];
                    char[] chArray = new char[1] { ' ' };
                    foreach (string str3 in str2.Split(chArray))
                        this.namesOfOkayBuildingLocations.Add(str3);
                    if (strArray1.Length > 16)
                        this.moneyRequired = Convert.ToInt32(strArray1[16]);
                    if (strArray1.Length <= 17)
                        return;
                    this.magical = Convert.ToBoolean(strArray1[17]);
                }
            }
        }

        public void consumeResources()
        {
            foreach (KeyValuePair<int, int> keyValuePair in this.itemsRequired)
                Game1.player.consumeObject(keyValuePair.Key, keyValuePair.Value);
            Game1.player.Money -= this.moneyRequired;
        }

        public int getTileSheetIndexForStructurePlacementTile(int x, int y)
        {
            if (x == this.humanDoor.X && y == this.humanDoor.Y)
                return 2;
            return x == this.animalDoor.X && y == this.animalDoor.Y ? 4 : 0;
        }

        public bool isUpgrade()
        {
            if (this.nameOfBuildingToUpgrade != null)
                return this.nameOfBuildingToUpgrade.Length > 0;
            return false;
        }

        public bool doesFarmerHaveEnoughResourcesToBuild()
        {
            foreach (KeyValuePair<int, int> keyValuePair in this.itemsRequired)
            {
                if (!Game1.player.hasItemInInventory(keyValuePair.Key, keyValuePair.Value, 0))
                    return false;
            }
            return Game1.player.Money >= this.moneyRequired;
        }

        public void drawDescription(SpriteBatch b, int x, int y, int width)
        {
            b.DrawString(Game1.smallFont, this.name, new Vector2((float)x, (float)y), Game1.textColor);
            string text = Game1.parseText(this.description, Game1.smallFont, width);
            b.DrawString(Game1.smallFont, text, new Vector2((float)x, (float)y + Game1.smallFont.MeasureString(this.name).Y), Game1.textColor * 0.75f);
            int num1 = (int)((double)y + (double)Game1.smallFont.MeasureString(this.name).Y + (double)Game1.smallFont.MeasureString(text).Y);
            foreach (KeyValuePair<int, int> keyValuePair in this.itemsRequired)
            {
                b.Draw(Game1.objectSpriteSheet, new Vector2((float)(x + Game1.tileSize / 8), (float)num1), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, keyValuePair.Key, 16, 16)), Color.White, 0.0f, new Vector2(6f, 3f), (float)Game1.pixelZoom * 0.5f, SpriteEffects.None, 0.999f);
                Color color = Game1.player.hasItemInInventory(keyValuePair.Key, keyValuePair.Value, 0) ? Color.DarkGreen : Color.DarkRed;
                Game1.drawWithBorder(string.Concat((object)keyValuePair.Value), Game1.textColor, Color.AntiqueWhite, new Vector2((float)(x + Game1.tileSize / 2) - Game1.tinyFont.MeasureString(string.Concat((object)keyValuePair.Value)).X, (float)(num1 + Game1.tileSize / 2) - Game1.tinyFont.MeasureString(string.Concat((object)keyValuePair.Value)).Y), 0.0f, 1f, 0.9f, true);
                b.DrawString(Game1.smallFont, Game1.objectInformation[keyValuePair.Key].Split('/')[0], new Vector2((float)(x + Game1.tileSize / 2 + Game1.tileSize / 4), (float)num1), color);
                num1 += (int)Game1.smallFont.MeasureString("P").Y;
            }
            if (this.moneyRequired <= 0)
                return;
            b.Draw(Game1.debrisSpriteSheet, new Vector2((float)x, (float)num1), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, -1, -1)), Color.White, 0.0f, new Vector2((float)(Game1.tileSize / 2 - Game1.tileSize / 8), (float)(Game1.tileSize / 2 - Game1.tileSize / 3)), 0.5f, SpriteEffects.None, 0.999f);
            Color color1 = Game1.player.money >= this.moneyRequired ? Color.DarkGreen : Color.DarkRed;
            b.DrawString(Game1.smallFont, this.moneyRequired.ToString() + "g", new Vector2((float)(x + Game1.tileSize / 4 + Game1.tileSize / 8), (float)num1), color1);
            int num2 = num1 + (int)Game1.smallFont.MeasureString(string.Concat((object)this.moneyRequired)).Y;
        }

        public BluePrint convertCustomBlueprintToBluePrint()
        {
            BluePrint blueprint = new BluePrint("Shed");

            this.monitor.Log($"The building blueprint for {this.name} loaded with {blueprint.texture.ToString()} and {this.texture.ToString()}!");


            blueprint.namesOfOkayBuildingLocations = this.namesOfOkayBuildingLocations;
            blueprint.itemsRequired = this.itemsRequired;
            blueprint.name = this.name;
            blueprint.woodRequired = this.woodRequired;
            blueprint.stoneRequired = this.stoneRequired;
            blueprint.copperRequired = this.copperRequired;
            blueprint.IronRequired = this.IronRequired;
            blueprint.GoldRequired = this.GoldRequired;
            blueprint.IridiumRequired = this.IridiumRequired;
            blueprint.tilesWidth = this.tilesWidth;
            blueprint.tilesHeight = this.tilesHeight;
            blueprint.maxOccupants = this.maxOccupants;
            blueprint.moneyRequired = this.moneyRequired;
            blueprint.humanDoor = this.humanDoor;
            blueprint.animalDoor = this.animalDoor;
            blueprint.mapToWarpTo = this.mapToWarpTo;
            blueprint.description = this.description;
            blueprint.blueprintType = this.blueprintType;
            blueprint.nameOfBuildingToUpgrade = this.nameOfBuildingToUpgrade;
            blueprint.actionBehavior = this.actionBehavior;
            blueprint.texture = this.texture;
            blueprint.sourceRectForMenuView = this.sourceRectForMenuView;
            blueprint.canBuildOnCurrentMap = this.canBuildOnCurrentMap;
            blueprint.magical = this.magical;

            return blueprint;
    }
    }
}
