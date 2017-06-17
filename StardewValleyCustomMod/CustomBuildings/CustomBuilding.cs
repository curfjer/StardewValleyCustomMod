using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using CustomFarmBuildings.CustomBlueprints;
using xTile;

namespace CustomFarmBuildings
{
    public class CustomBuilding : StardewValley.Buildings.Building
    {
        public string modName;
        public bool seasonal;
        public string fileName;
        public string folderName;
        public string buildingName;
        public float scalar;
        public bool customConstruction;
        public int daysToConstruct;
        public int[] constructionDayTextureList;
        public string[] specialProperties;
        public Texture2D ConstructionTexture;
        public int textureBitSize;
        public int textureScalar;

        [XmlIgnore]
        public LocalizedContentManager Content;

        public CustomBuilding()
        {

        }

        // Create CustomBuilding from a blueprint and the location for the building
        public CustomBuilding(CustomBuildingBlueprint blu, Vector2 coord)
        {
            // Names
            this.modName = blu.ModName;
            this.fileName = blu.FileName;
            this.folderName = blu.FolderName;

            // Building Info
            this.buildingType = blu.BuildingType;
            this.buildingName = blu.BuildingName;
            this.tileX = (int)coord.X;
            this.tileY = (int)coord.Y;
            this.tilesWide = blu.TilesWidth;
            this.tilesHigh = blu.TilesHeight;
            this.texture = blu.texture;
            this.maxOccupants = blu.MaxOccupants;
            this.daysToConstruct = blu.DaysToConstruct;
            this.customConstruction = blu.CustomConstruction;
            this.constructionDayTextureList = blu.ConstructionDayTextureList;

            // Interior
            if (blu.HasInterior())
            {
                this.indoors = blu.GetIndoors();
                this.nameOfIndoors = blu.CurrentInterior.Name + "-" + this.tileX + "-" + this.tileY;
                this.baseNameOfIndoors = blu.CurrentInterior.Name;
                this.nameOfIndoorsWithoutUnique = blu.CurrentInterior.Name;
            }
            
            // Upgrade
            //if(blu.isUpgrade())
                //this.daysUntilUpgrade = blu.DaysToConstruct;
            //else
                this.daysOfConstructionLeft = blu.DaysToConstruct;

            // Doors
            this.humanDoor = blu.HumanDoorTileCoord;

            // Extra
            this.seasonal = blu.Seasonal;
            this.magical = blu.Magical;
            this.scalar = 4;
            this.specialProperties = blu.SpecialPropteries;
            this.textureBitSize = 16;//DEFAULT for now Change later TODO
            this.textureScalar = Game1.tileSize / this.textureBitSize;
        }

        // Create CustomBuilding from Building (base game)
        public CustomBuilding(Building building)
        {
            string[] modBuilding = building.buildingType.Split('_');

            this.color = building.color;
            this.indoors = building.indoors;
            this.texture = building.texture;
            this.tileX = building.tileX;
            this.tileY = building.tileY;
            this.tilesWide = building.tilesWide;
            this.tilesHigh = building.tilesHigh;
            this.maxOccupants = building.maxOccupants;
            this.currentOccupants = building.currentOccupants;
            this.daysOfConstructionLeft = building.daysOfConstructionLeft;
            this.daysUntilUpgrade = building.daysUntilUpgrade;
            this.modName = modBuilding[0];
            this.buildingType = modBuilding[1];
            this.nameOfIndoors = building.nameOfIndoors;
            this.baseNameOfIndoors = building.baseNameOfIndoors;
            this.nameOfIndoorsWithoutUnique = building.nameOfIndoorsWithoutUnique;
            this.humanDoor = building.humanDoor;
            this.animalDoor = building.animalDoor;
            this.animalDoorOpen = building.animalDoorOpen;
            this.magical = building.magical;
            this.owner = building.owner;
        }

        // Change name TODO this grabs the values of the bulding and gives it to custombuilding
        public virtual void ConvertBuildingToCustomBuilding(Building building)
        {
            string[] modBuilding = building.buildingType.Split('_');

            this.color = building.color;
            this.indoors = building.indoors;
            this.texture = building.texture;
            this.tileX = building.tileX;
            this.tileY = building.tileY;
            this.tilesWide = building.tilesWide;
            this.tilesHigh = building.tilesHigh;
            this.maxOccupants = building.maxOccupants;
            this.currentOccupants = building.currentOccupants;
            this.daysOfConstructionLeft = building.daysOfConstructionLeft;
            this.daysUntilUpgrade = building.daysUntilUpgrade;
            this.modName = modBuilding[0];
            this.buildingType = modBuilding[1];
            this.nameOfIndoors = building.nameOfIndoors;
            this.baseNameOfIndoors = building.baseNameOfIndoors;
            this.nameOfIndoorsWithoutUnique = building.nameOfIndoorsWithoutUnique;
            this.humanDoor = building.humanDoor;
            this.animalDoor = building.animalDoor;
            this.animalDoorOpen = building.animalDoorOpen;
            this.magical = building.magical;
            this.owner = building.owner;
        }

        public Building ConvertCustomBuildingToBuilding()
        {
            Building building = new Building();

            building.color = this.color;
            building.indoors = this.indoors;
            building.texture = this.texture;
            building.tileX = this.tileX;
            building.tileY = this.tileY;
            building.tilesWide = this.tilesWide;
            building.tilesHigh = this.tilesHigh;
            building.maxOccupants = this.maxOccupants;
            building.currentOccupants = this.currentOccupants;
            building.daysOfConstructionLeft = this.daysOfConstructionLeft;
            building.daysUntilUpgrade = this.daysUntilUpgrade;
            building.buildingType = "MOD_" + this.modName + "_" + this.buildingName;
            building.nameOfIndoors = this.nameOfIndoors;
            building.baseNameOfIndoors = this.baseNameOfIndoors;
            building.nameOfIndoorsWithoutUnique = this.nameOfIndoorsWithoutUnique;
            building.humanDoor = this.humanDoor;
            building.animalDoor = this.animalDoor;
            building.animalDoorOpen = this.animalDoorOpen;
            building.magical = this.magical;
            building.owner = this.owner;

            return building;
        }

        // Load the custom building and its interior into memory
        public override void load()
        {
            string buildingPath = Path.Combine("Mods\\StardewValleyCustomMod", "buildingMods", this.modName, this.folderName);
            this.Content = new LocalizedContentManager(Game1.content.ServiceProvider, buildingPath);

            // Load Building
            if (this.seasonal)
                this.texture = this.Content.Load<Texture2D>(this.fileName + "_" + Game1.currentSeason);
            else
                this.texture = this.Content.Load<Texture2D>(this.fileName);// Create a custom building then convert back to orig building         

            if (this.customConstruction)
                this.UpdateConstructionTexture();                

            // Interior:
            GameLocation indoors1 = this.GetIndoors();
            if (indoors1 != null)
            {
                indoors1.characters = this.indoors.characters;
                indoors1.objects = this.indoors.objects;
                indoors1.terrainFeatures = this.indoors.terrainFeatures;
                indoors1.isStructure = true;
                indoors1.uniqueName = indoors1.name + (object)(this.tileX * 2000 + this.tileY);
                indoors1.numberOfSpawnedObjectsOnMap = this.indoors.numberOfSpawnedObjectsOnMap;

                // Load Animals
                if (this.indoors.GetType().Equals(typeof(AnimalHouse)))
                {
                    ((AnimalHouse)indoors1).animals = ((AnimalHouse)this.indoors).animals;
                    ((AnimalHouse)indoors1).animalsThatLiveHere = ((AnimalHouse)this.indoors).animalsThatLiveHere;
                    foreach (KeyValuePair<long, FarmAnimal> animal in (Dictionary<long, FarmAnimal>)((AnimalHouse)indoors1).animals)
                    {
                        animal.Value.reload();
                        //Game1.getFarm().animals.Add(animal.Key, animal.Value);// TODO do I need this for the animal to go outside
                    }
                }
                
                // TODO change this???
                if (this.indoors is Shed)
                {
                    ((DecoratableLocation)indoors1).furniture = ((DecoratableLocation)this.indoors).furniture;
                    foreach (Furniture furniture in ((DecoratableLocation)indoors1).furniture)
                        furniture.updateDrawPosition();
                    ((DecoratableLocation)indoors1).wallPaper = ((DecoratableLocation)this.indoors).wallPaper;
                    ((DecoratableLocation)indoors1).floor = ((DecoratableLocation)this.indoors).floor;
                }
                this.indoors = indoors1;

                // Other:
                if (this.indoors.IsFarm && this.indoors.terrainFeatures == null)
                    this.indoors.terrainFeatures = new SerializableDictionary<Vector2, TerrainFeature>();
                foreach (NPC character in this.indoors.characters)
                    character.reloadSprite();
                foreach (TerrainFeature terrainFeature in this.indoors.terrainFeatures.Values)
                    terrainFeature.loadSprite();
                foreach (KeyValuePair<Vector2, StardewValley.Object> keyValuePair in (Dictionary<Vector2, StardewValley.Object>)this.indoors.objects)
                {
                    keyValuePair.Value.initializeLightSource(keyValuePair.Key);
                    keyValuePair.Value.reloadSprite();
                }
            }
        }

        // Returns GameLocation interior for custom building
        // Returns null if blueprint not found for custom building
        public GameLocation GetIndoors()
        {
            GameLocation interior = null;

            // Get interior for custom building from the custom blueprint list
            foreach (CustomBuildingBlueprint blu in CustomFarmBuildings.Config.BlueprintList)
            {
                if (blu.BuildingName.Equals(this.buildingName) && blu.Interiors.Count > 0)
                {
                    interior = blu.GetIndoors();
                    
                    // Set warps for interior:
                    foreach (Warp warp in interior.warps)
                    {
                        int num1 = this.humanDoor.X + this.tileX;
                        warp.TargetX = num1;
                        int num2 = this.humanDoor.Y + this.tileY + 1;
                        warp.TargetY = num2;
                    }
                    
                    /*
                    // TODO is this needed?
                    if(this.animalHouse)
                        if (this.animalDoorOpen)
                            this.yPositionOfAnimalDoor = CustomBuilding.openAnimalDoorPosition;
                        //if ((interior as AnimalHouse).incubatingEgg.Y > 0)
                           // interior.map.GetLayer("Front").Tiles[1, 2].TileIndex += Game1.player.ActiveObject.ParentSheetIndex == 180 || Game1.player.ActiveObject.ParentSheetIndex == 182 ? 2 : 1;
                        */
                    return interior;
                }
            }

            // Building interior not found, interior is null
            CustomFarmBuildings.Logger.Log($"{this.nameOfIndoorsWithoutUnique} not found for {this.buildingName} from {this.modName}");
            return interior;
        }

        public override void drawInMenu(SpriteBatch b, int x, int y)
        {
            this.drawShadow(b, x, y);
            b.Draw(this.texture, new Vector2((float)x, (float)y), this.texture.Bounds, this.color, 0.0f, new Vector2(0.0f, 0.0f), (float)this.scalar, SpriteEffects.None, 0.89f);
        }

        // Scales the building to fit in menu
        public void drawInMenu(SpriteBatch b, int x, int y, float scalar)
        {
            this.scalar = scalar;
            this.drawInMenu(b, x, y);
        }

        //
        public override void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
        {
            Vector2 position = localX == -1 ? Game1.GlobalToLocal(new Vector2((float)(this.tileX * Game1.tileSize), (float)((this.tileY + this.tilesHigh) * Game1.tileSize))) : new Vector2((float)localX, (float)(localY + this.texture.Height * this.scalar));
            if (localY != -1)
            {
                b.Draw(Game1.mouseCursors, position, new Rectangle?(Building.leftShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)this.scalar, SpriteEffects.None, 1E-05f);
                for (int index = 1; index < this.tilesWide - 1; ++index)
                    b.Draw(Game1.mouseCursors, position + new Vector2((float)(index * Game1.tileSize / 4 * this.scalar), 0.0f), new Rectangle?(Building.middleShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)this.scalar, SpriteEffects.None, 1E-05f);
                b.Draw(Game1.mouseCursors, position + new Vector2((float)((this.tilesWide - 1) * Game1.tileSize / 4 * this.scalar), 0.0f), new Rectangle?(Building.rightShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)this.scalar, SpriteEffects.None, 1E-05f);
            }
            else
            {
                b.Draw(Game1.mouseCursors, position, new Rectangle?(Building.leftShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-05f);
                for (int index = 1; index < this.tilesWide - 1; ++index)
                    b.Draw(Game1.mouseCursors, position + new Vector2((float)(index * Game1.tileSize), 0.0f), new Rectangle?(Building.middleShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-05f);
                b.Draw(Game1.mouseCursors, position + new Vector2((float)((this.tilesWide - 1) * Game1.tileSize), 0.0f), new Rectangle?(Building.rightShadow), Color.White * (localX == -1 ? this.alpha : 1f), 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-05f);
            }            
        }

        public override void draw(SpriteBatch b)
        {
            // Construction
            if (this.daysOfConstructionLeft > 0 || this.daysUntilUpgrade > 0)
                if (this.customConstruction)
                    this.DrawInCustomConstruction(b);
                else
                    this.drawInConstruction(b);

            // Building
            else
                base.draw(b);
        }

        public void DrawInCustomConstruction(SpriteBatch b)
        {
            b.Draw(this.ConstructionTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.texture.Bounds), this.color * this.alpha, 0.0f, new Vector2(0.0f, 112f), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000f);
        }

        // Update any user interactions with the building
        public override void Update(GameTime time)
        {
            base.Update(time);
        }

        public override void dayUpdate(int dayOfMonth)
        {
            base.dayUpdate(dayOfMonth);

            CustomFarmBuildings.Logger.Log($"ConstructionDays: {this.daysOfConstructionLeft}");
            CustomFarmBuildings.Logger.Log($"UpgradeDays: {this.daysUntilUpgrade}");

            // TODO is this where I should put it?
            if (this.customConstruction)
                this.UpdateConstructionTexture();
        }

        public void UpdateConstructionTexture()
        {
            this.ConstructionTexture = this.Content.Load<Texture2D>(this.fileName + "_construction_" + this.constructionDayTextureList[this.daysToConstruct - this.daysOfConstructionLeft]);
        }
        
        // TODO needs a better check for which tile is part of the door, if the door varies in width
        // Check for player interactions
        /*public override bool doAction(Vector2 tileLocation, StardewValley.Farmer who)
        {
            

            return true;
        }*/
    }
}
