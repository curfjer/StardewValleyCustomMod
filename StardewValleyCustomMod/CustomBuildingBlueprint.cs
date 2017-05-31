// Decompiled with JetBrains decompiler
// Type: StardewValley.BluePrint
// Assembly: Stardew Valley, Version=1.0.6124.25603, Culture=neutral, PublicKeyToken=null
// MVID: 8735DDC4-C499-43F5-9D59-831F1FFC73CF
// Assembly location: E:\SteamLibrary\steamapps\common\Stardew Valley\Stardew Valley.exe

// TODO
// Skill level requirement for building?

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using StardewValley;
using StardewModdingAPI;
using Entoarox.Framework;
using System.IO;
using StardewValley.Locations;
using StardewValley.Buildings;

namespace StardewValleyCustomMod.CustomBlueprints
{
    public class CustomBuildingBlueprint
    {
        public string FileName;
        public string BuildingName;
        public string NameOfBuildingToUpgrade;
        public string FolderName;

        public List<CustomInterior> Interiors;
        public CustomInterior CurrentInterior; // Use this for selecting interior, shift through the interiors list^^^ in the menu and grab currentInterior
        //public bool CustomConstruction;
        //public List<string> CustomConstructionFileNames;
        public List<CustomTileSheets> CustomTileSheets;

        public int TilesWidth;
        public int TilesHeight;
        public Point HumanDoorTileCoord;
        public Point AnimalDoorTileCoord;
        public int MaxOccupants;
        public int AnimalDoorWidth;
        public int AnimalDoorHeight;
        public Texture2D AnimalDoorTexture;

        public string Description;
        public bool Magical;
        public bool Seasonal;

        public Dictionary<int, int> ResourcesRequired = new Dictionary<int, int>();
        public int MoneyRequired;
        public int[] SkillsRequired;
        public int DaysToConstruct;

        public int SourceRectHeight;// What are these for?
        public int SourceRectWidth; // ^^^
        public string ActionBehavior;

        public string ModName;

        private List<string> namesOfOkayBuildingLocations = new List<string>();
        private Dictionary<int, int> itemsRequired = new Dictionary<int, int>();
        
        public int woodRequired;
        public int stoneRequired;
        public int copperRequired;
        public int IronRequired;
        public int GoldRequired;
        public int IridiumRequired;
        public string BlueprintType;
        public Texture2D texture;
        private Rectangle sourceRectForMenuView;
        private bool canBuildOnCurrentMap;
        private LocalizedContentManager content;

        private int CurrentInteriorIndex;

        public CustomBuildingBlueprint()
        {
            this.canBuildOnCurrentMap = false; //Does this break anything if defaulted to false?
            
            this.FileName = "Default";
            this.BuildingName = "Default";
            this.ModName = "Default";
            this.NameOfBuildingToUpgrade = "";
            this.FolderName = "Default";

            this.Interiors = new List<CustomInterior>();
            this.CustomTileSheets = new List<CustomTileSheets>();

            this.TilesHeight = 0;
            this.TilesHeight = 0;
            this.HumanDoorTileCoord = new Point(-1, -1);
            this.AnimalDoorTileCoord = new Point(-1, -1);
            this.MaxOccupants = 0;

            this.AnimalDoorWidth = 0;
            this.AnimalDoorHeight = 0;

            this.Description = "No description.";
            this.Magical = false;
            this.Seasonal = false;

            this.MoneyRequired = 0;
            this.SkillsRequired = new int[]{ 0, 0, 0, 0};
            this.DaysToConstruct = 2;

            this.SourceRectHeight = 0;
            this.SourceRectWidth = 0;
            this.sourceRectForMenuView = new Rectangle(0, 0, this.SourceRectWidth, this.SourceRectHeight);

            this.ActionBehavior = "Farm";
            this.BlueprintType = "Buildings";

            this.CurrentInteriorIndex = 0;
        }

        // Base Code
        public void consumeResources()
        {
            foreach (KeyValuePair<int, int> keyValuePair in this.ResourcesRequired)
                Game1.player.consumeObject(keyValuePair.Key, keyValuePair.Value);
            Game1.player.Money -= this.MoneyRequired;
        }

        // Base Code
        public int getTileSheetIndexForStructurePlacementTile(int x, int y)
        {
            if (x == this.HumanDoorTileCoord.X && y == this.HumanDoorTileCoord.Y)
                return 2;
            return x == this.AnimalDoorTileCoord.X && y == this.AnimalDoorTileCoord.Y ? 4 : 0;
        }

        // Base Code
        public bool isUpgrade()
        {
            if (this.NameOfBuildingToUpgrade != null)
                return this.NameOfBuildingToUpgrade.Length > 0;
            return false;
        }

        // Base Code
        public bool doesFarmerHaveEnoughResourcesToBuild()
        {
            foreach (KeyValuePair<int, int> keyValuePair in this.ResourcesRequired)
            {
                if (!Game1.player.hasItemInInventory(keyValuePair.Key, keyValuePair.Value, 0))
                    return false;
            }
            return Game1.player.Money >= this.MoneyRequired;
        }

        // Base Code NOT USED????
        public void drawDescription(SpriteBatch b, int x, int y, int width)
        {
            b.DrawString(Game1.smallFont, this.BuildingName, new Vector2((float)x, (float)y), Game1.textColor);
            string text = Game1.parseText(this.Description, Game1.smallFont, width);
            b.DrawString(Game1.smallFont, text, new Vector2((float)x, (float)y + Game1.smallFont.MeasureString(this.BuildingName).Y), Game1.textColor * 0.75f);
            int num1 = (int)((double)y + (double)Game1.smallFont.MeasureString(this.BuildingName).Y + (double)Game1.smallFont.MeasureString(text).Y);
            foreach (KeyValuePair<int, int> keyValuePair in this.ResourcesRequired)
            {
                b.Draw(Game1.objectSpriteSheet, new Vector2((float)(x + Game1.tileSize / 8), (float)num1), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, keyValuePair.Key, 16, 16)), Color.White, 0.0f, new Vector2(6f, 3f), (float)Game1.pixelZoom * 0.5f, SpriteEffects.None, 0.999f);
                Color color = Game1.player.hasItemInInventory(keyValuePair.Key, keyValuePair.Value, 0) ? Color.DarkGreen : Color.DarkRed;
                Game1.drawWithBorder(string.Concat((object)keyValuePair.Value), Game1.textColor, Color.AntiqueWhite, new Vector2((float)(x + Game1.tileSize / 2) - Game1.tinyFont.MeasureString(string.Concat((object)keyValuePair.Value)).X, (float)(num1 + Game1.tileSize / 2) - Game1.tinyFont.MeasureString(string.Concat((object)keyValuePair.Value)).Y), 0.0f, 1f, 0.9f, true);
                b.DrawString(Game1.smallFont, Game1.objectInformation[keyValuePair.Key].Split('/')[0], new Vector2((float)(x + Game1.tileSize / 2 + Game1.tileSize / 4), (float)num1), color);
                num1 += (int)Game1.smallFont.MeasureString("P").Y;
            }
            if (this.MoneyRequired <= 0)
                return;
            b.Draw(Game1.debrisSpriteSheet, new Vector2((float)x, (float)num1), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, -1, -1)), Color.White, 0.0f, new Vector2((float)(Game1.tileSize / 2 - Game1.tileSize / 8), (float)(Game1.tileSize / 2 - Game1.tileSize / 3)), 0.5f, SpriteEffects.None, 0.999f);
            Color color1 = Game1.player.money >= this.MoneyRequired ? Color.DarkGreen : Color.DarkRed;
            b.DrawString(Game1.smallFont, this.MoneyRequired.ToString() + "g", new Vector2((float)(x + Game1.tileSize / 4 + Game1.tileSize / 8), (float)num1), color1);
            int num2 = num1 + (int)Game1.smallFont.MeasureString(string.Concat((object)this.MoneyRequired)).Y;
        }

        // Not needed?
        public BluePrint convertCustomBlueprintToBluePrint()
        {
            // Create dummy blueprint
            BluePrint blueprint = new BluePrint("Shed");
            this.LoadCustomBuildingBlueprint();

            StardewValleyCustomMod.Logger.Log($"The building blueprint for {this.BuildingName} loaded with {blueprint.texture.ToString()} and {this.texture.ToString()}!");

            if (StardewValleyCustomMod.Config.Debug)
                StardewValleyCustomMod.Debug.DebugCustomBlueprintValues(this);

            // Change dummy blueprint to the current custom blueprint
            blueprint.namesOfOkayBuildingLocations = this.namesOfOkayBuildingLocations;
            blueprint.itemsRequired = this.ResourcesRequired;
            blueprint.name = this.BuildingName;
            blueprint.woodRequired = this.woodRequired;
            blueprint.stoneRequired = this.stoneRequired;
            blueprint.copperRequired = this.copperRequired;
            blueprint.IronRequired = this.IronRequired;
            blueprint.GoldRequired = this.GoldRequired;
            blueprint.IridiumRequired = this.IridiumRequired;
            blueprint.tilesWidth = this.TilesWidth;
            blueprint.tilesHeight = this.TilesHeight;
            blueprint.maxOccupants = this.MaxOccupants;
            blueprint.moneyRequired = this.MoneyRequired;
            blueprint.humanDoor = this.HumanDoorTileCoord;
            blueprint.animalDoor = this.AnimalDoorTileCoord;
            blueprint.mapToWarpTo = this.CurrentInterior.Name;
            blueprint.description = this.Description;
            blueprint.blueprintType = this.BlueprintType;
            blueprint.nameOfBuildingToUpgrade = this.NameOfBuildingToUpgrade;
            blueprint.actionBehavior = this.ActionBehavior;
            blueprint.texture = this.texture;
            blueprint.sourceRectForMenuView = this.sourceRectForMenuView;
            //blueprint.sourceRectForMenuView = new Rectangle(0, 0, this.sourceRectHeight, this.sourceRectWidth); ;
            blueprint.canBuildOnCurrentMap = this.canBuildOnCurrentMap;
            blueprint.magical = this.Magical;

            return blueprint;
        }
        
        //
        public void LoadCustomBuildingBlueprint()
        {
            this.content = new LocalizedContentManager(Game1.content.ServiceProvider, Path.Combine("Mods\\StardewValleyCustomMod\\buildingMods", this.ModName, this.FolderName));
            try
            {
                this.texture = content.Load<Texture2D>(this.FileName);
                if (this.BlueprintType.Equals("AnimalHouse"))
                    this.AnimalDoorTexture = content.Load<Texture2D>(this.FileName + "_AnimalDoor");
                StardewValleyCustomMod.Logger.Log($"The building blueprint for {this.BuildingName} loaded with texture!"); //DEBUG REMOVE
            }
            catch (Exception ex)
            {
                StardewValleyCustomMod.Logger.ExitGameImmediately($"The building blueprint for {this.BuildingName} loaded with no texture!",ex);
            }
            this.sourceRectForMenuView = new Rectangle(0, 0, this.SourceRectHeight, this.SourceRectWidth);
        }

        // Returns the interior of the building
        public GameLocation GetIndoors()
        {
            UpdateCurrentInterior(0); // TODO Find a better spot for this
            
            String interiorPath = Path.Combine(StardewValleyCustomMod.ModPath, "buildingMods", this.ModName, this.FolderName, this.CurrentInterior.FileName);
            StardewValleyCustomMod.Logger.Log($"{interiorPath}");
            try
            {
                GameLocation interior;
                StardewValleyCustomMod.ContentRegistry.RegisterXnb(interiorPath, interiorPath);
                xTile.Map map = Game1.content.Load<xTile.Map>(interiorPath);
                switch (this.CurrentInterior.Type)
                {
                    case "Cellar":
                        interior = new StardewValley.Locations.Cellar(map, this.CurrentInterior.Name);
                        interior.objects = new SerializableDictionary<Microsoft.Xna.Framework.Vector2, StardewValley.Object>();
                        break;
                    case "BathHousePool":
                        interior = new StardewValley.Locations.BathHousePool(map, this.CurrentInterior.Name);
                        break;
                    case "Decoratable":
                        interior = new StardewValley.Locations.DecoratableLocation(map, this.CurrentInterior.Name);
                        break;
                    case "Desert":
                        interior = new StardewValley.Locations.Desert(map, this.CurrentInterior.Name);
                        break;
                    case "Greenhouse":
                        interior = new GameLocation(map, this.CurrentInterior.Name);
                        //interior = new StardewValley.Locations.Greenhouse(map, this.mapToWarpTo);
                        break;
                    case "Sewer":
                        interior = new StardewValley.Locations.Sewer(map, this.CurrentInterior.Name);
                        break;
                    case "AnimalHouse":
                        interior = new AnimalHouse(map, this.CurrentInterior.Name);
                        (interior as AnimalHouse).animalLimit = this.MaxOccupants;
                        break;
                    default:
                        interior = new GameLocation(map, this.CurrentInterior.Name);
                        break;
                }
                interior.isOutdoors = this.CurrentInterior.Outdoor;
                interior.isFarm = this.CurrentInterior.Farmable;
                interior.objects = new SerializableDictionary<Microsoft.Xna.Framework.Vector2, StardewValley.Object>();
                interior = this.LoadCustomTileSheets(interior);
                if(!Game1.locations.Contains(interior))
                {
                    Game1.locations.Add(interior);
                    StardewValleyCustomMod.Logger.Log($"Added {this.CurrentInterior.Name}");
                }
                return interior;
            }
            catch (Exception err)
            {
                StardewValleyCustomMod.Logger.ExitGameImmediately("Unable to add custom location, a unexpected error occured: " + "Winery" + err);
            }
            return null;
        }

        // TODO Change or add another method that accepts a parameter to either go up or down the interior array
        public void UpdateCurrentInterior(int increment)
        {
            StardewValleyCustomMod.Logger.Log($"UpdateCurrentInterior...");
            StardewValleyCustomMod.Logger.Log($"Index: {this.CurrentInteriorIndex}");
            StardewValleyCustomMod.Logger.Log($"Increment: {increment}");
            if (this.CurrentInteriorIndex + increment >= this.Interiors.Count)
                this.CurrentInteriorIndex = 0;
            else if (this.CurrentInteriorIndex + increment < 0)
                this.CurrentInteriorIndex = this.Interiors.Count - 1;
            else
                this.CurrentInteriorIndex += increment;

            StardewValleyCustomMod.Logger.Log($"UpdatedIndex: {this.CurrentInteriorIndex}");

            this.CurrentInterior = this.GetCustomInterior();
        }

        public void SetModName(string modName)
        {
            this.ModName = modName;
        }
        
        public void SetSourceRect()
        {
            this.sourceRectForMenuView = new Rectangle(0, 0, this.SourceRectWidth, this.SourceRectHeight);
            StardewValleyCustomMod.Logger.Log($"SourceRect: {this.sourceRectForMenuView.ToString()}");
        }

        public GameLocation LoadCustomTileSheets(GameLocation interior)
        {
            Texture2D sheet = null;
            string filePathStart = Path.Combine(StardewValleyCustomMod.ModPath, "buildingMods", this.ModName, this.FolderName);
            string filePath = filePathStart;

            foreach (CustomTileSheets tileSheet in CustomTileSheets)
            {
                filePath = filePathStart; // Reset filePath
                try
                {
                    filePath = Path.Combine(filePath, tileSheet.FileName);
                    if (tileSheet.Seasonal)
                        filePath = Path.Combine(filePath, "_", Game1.currentSeason);
                    StardewValleyCustomMod.Logger.Log($"FilePath: {filePath}");
                    StardewValleyCustomMod.ContentRegistry.RegisterXnb(filePath, filePath);

                    if (interior.map.GetTileSheet(tileSheet.SheetID) != null)
                    {
                        interior.map.GetTileSheet(tileSheet.SheetID).ImageSource = filePath;
                        StardewValleyCustomMod.Logger.Log($"{tileSheet.FileName}{(tileSheet.Seasonal ? "_seasonal" : "")}/override", LogLevel.Trace);
                    }
                    else
                    {
                        sheet = Game1.content.Load<Texture2D>(filePath);
                        interior.map.AddTileSheet(new xTile.Tiles.TileSheet(tileSheet.SheetID, interior.map, filePath, new xTile.Dimensions.Size((int)Math.Ceiling(sheet.Width / 16.0), (int)Math.Ceiling(sheet.Height / 16.0)), new xTile.Dimensions.Size(16, 16)));
                        StardewValleyCustomMod.Logger.Log($"{tileSheet.FileName}{(tileSheet.Seasonal ? "_seasonal" : "")}/add", LogLevel.Trace);
                    }
                }
                catch (Exception err)
                {
                    StardewValleyCustomMod.Logger.ExitGameImmediately($"Unable to load tilesheet '{tileSheet.FileName}' for {interior.name}", err);
                }
                
            }

            return interior;
        }

        public bool HasInterior()
        {
            return this.Interiors.Count > 0 ? true : false;
        }

        public CustomInterior GetCustomInterior()
        {
            return this.Interiors.Count > 0 ? this.Interiors[this.CurrentInteriorIndex] : null;
        }

        public void ResetCustomInterior()
        {
            this.CurrentInteriorIndex = 0;
            if (this.Interiors.Count > 0)
                this.CurrentInterior = this.Interiors[0];
        }

        public int[] GetBuildingCount()
        {
            int[] buildingCount = new int[2] { 0, 0};

            foreach (Building building in ((BuildableGameLocation)Game1.getLocationFromName("Farm")).buildings)
            {
                if (building.buildingType.Equals(this.ModName + "_" + this.BuildingName))
                {
                    buildingCount[0]++;
                    if(this.HasInterior())
                        if (building.nameOfIndoorsWithoutUnique.Equals(this.CurrentInterior.Name))
                            buildingCount[1]++;
                }
            }
            return buildingCount;
        }

        // Copies the value of one parameter to other similar parameters if they are not default value.
        // Assumes the other parameters are of the same value.
        public void SetDefaults()
        {
            if (this.FileName != "Default")
            {
                if (this.BuildingName == "Default")
                    this.BuildingName = this.FileName;
                if (this.FolderName == "Default")
                    this.FolderName = this.FileName;
            }else if(this.FolderName != "Default")
            {
                if (this.BuildingName == "Default")
                    this.BuildingName = this.FolderName;
                if (this.FileName == "Default")
                    this.FileName = this.FolderName;
            }
            else if (this.BuildingName != "Default")
            {
                if (this.FolderName == "Default")
                    this.FolderName = this.BuildingName;
                if (this.FileName == "Default")
                    this.FileName = this.BuildingName;
            }

            foreach (CustomInterior interior in this.Interiors)
                interior.SetDefaults();

            foreach (CustomTileSheets tiles in this.CustomTileSheets)
                tiles.SetDefaults();

            this.LoadCustomBuildingBlueprint();
        }
    }

    public class CustomInterior
    {
        public string FileName;
        public string Name;
        public string Type;
        public bool Seasonal;
        public bool Farmable;
        public bool Outdoor;

        public CustomInterior()
        {
            this.FileName = "Default";
            this.Name = "Default";
            this.Type = "Default";
            this.Seasonal = false;
            this.Farmable = true;
            this.Outdoor = false;
        }

        public void SetDefaults()
        {
            if (this.FileName != "Default" && this.Name == "Default")
                this.Name = this.FileName;
            else if (this.Name != "Default" && this.FileName == "Default")
                    this.FileName = this.Name;
        }
    }

    public class CustomTileSheets
    {
        public string FileName;
        public string SheetID;
        public bool Seasonal;

        public CustomTileSheets()
        {
            this.FileName = "Default";
            this.SheetID = "Default";
            this.Seasonal = false;
        }

        public void SetDefaults()
        {
            if (this.FileName != "Default" && this.SheetID == "Default")
                this.SheetID = this.FileName;
            else if (this.SheetID != "Default" && this.FileName == "Default")
                this.FileName = this.SheetID;
        }
    }
}
