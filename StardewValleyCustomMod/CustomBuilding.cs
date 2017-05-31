using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValleyCustomMod.CustomBlueprints;
using xTile;

namespace StardewValleyCustomMod
{
    public class CustomBuilding : StardewValley.Buildings.Building
    {
        public string modName;
        public bool seasonal;
        public string fileName;
        public string folderName;
        public bool animalHouse;
        public static int openAnimalDoorPosition = -Game1.tileSize + Game1.pixelZoom * 3;
        private const int closedAnimalDoorPosition = 0;
        private int yPositionOfAnimalDoor;
        private int animalDoorMotion;
        private Vector2 animalDoorTileSheetCoords;
        private int animalDoorHeight;
        private int animalDoorWidth;
        private Texture2D AnimalDoorTexture;
        private LocalizedContentManager Content;

        public CustomBuilding()
        {

        }

        public CustomBuilding(CustomBuildingBlueprint blu, Vector2 coord)// , Vector???? TODO
        {
            this.modName = blu.ModName;
            this.fileName = blu.FileName;
            this.folderName = blu.FolderName;

            this.buildingType = blu.BuildingName;
            this.tileX = (int)coord.X;
            this.tileY = (int)coord.Y;
            this.tilesWide = blu.TilesWidth;
            this.tilesHigh = blu.TilesHeight;

            if (blu.HasInterior())
            {
                this.indoors = blu.GetIndoors();
                this.nameOfIndoors = blu.CurrentInterior.Name + "-" + this.tileX + "-" + this.tileY;
                this.baseNameOfIndoors = blu.CurrentInterior.Name;
                this.nameOfIndoorsWithoutUnique = blu.CurrentInterior.Name;
            }
                
            this.texture = blu.texture;
            
            this.maxOccupants = blu.MaxOccupants;
            this.daysOfConstructionLeft = blu.DaysToConstruct;
            this.daysUntilUpgrade = blu.DaysToConstruct;

            this.humanDoor = blu.HumanDoorTileCoord;
            this.animalDoor = blu.AnimalDoorTileCoord;
            this.animalDoorOpen = false;
            this.animalDoorTileSheetCoords = new Vector2(0,0);
            this.animalDoorWidth = blu.AnimalDoorWidth;
            this.animalDoorHeight = blu.AnimalDoorHeight;
            this.AnimalDoorTexture = blu.AnimalDoorTexture;

            if (blu.BlueprintType.Equals("AnimalHouse"))
                this.animalHouse = true;
            else
                this.animalHouse = false;

            this.seasonal = blu.Seasonal;
            this.magical = blu.Magical;
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

        // Load the custom building into memory
        public override void load()
        {
            string buildingPath = Path.Combine("Mods\\StardewValleyCustomMod", "buildingMods", this.modName, this.folderName);
            this.Content = new LocalizedContentManager(Game1.content.ServiceProvider, buildingPath);
            //this.GetInfoFromBlueprint();

            if (this.seasonal)
                this.texture = this.Content.Load<Texture2D>(this.fileName + "_" + Game1.currentSeason);
            else
                this.texture = this.Content.Load<Texture2D>(this.fileName);// Create a custom building then convert back to orig building         

            // Interior:
            GameLocation indoors1 = this.GetIndoors();
            if (indoors1 == null)
                return;
            indoors1.characters = this.indoors.characters;
            indoors1.objects = this.indoors.objects;
            indoors1.terrainFeatures = this.indoors.terrainFeatures;
            indoors1.isStructure = true;
            indoors1.uniqueName = indoors1.name + (object)(this.tileX * 2000 + this.tileY);
            indoors1.numberOfSpawnedObjectsOnMap = this.indoors.numberOfSpawnedObjectsOnMap;
            if (this.indoors.GetType().Equals(typeof(AnimalHouse)))
            {
                this.animalHouse = true;
                ((AnimalHouse)indoors1).animals = ((AnimalHouse)this.indoors).animals;
                ((AnimalHouse)indoors1).animalsThatLiveHere = ((AnimalHouse)this.indoors).animalsThatLiveHere;
                foreach (KeyValuePair<long, FarmAnimal> animal in (Dictionary<long, FarmAnimal>)((AnimalHouse)indoors1).animals)
                    animal.Value.reload();
            }
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

        // Returns GameLocation interior for custom building
        // Returns null if blueprint not found for custom building
        public GameLocation GetIndoors()
        {
            GameLocation interior = null;

            // Get interior for custom building from the custom blueprint list
            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.BlueprintList)
            {
                if (blu.BuildingName.Equals(this.buildingType) && blu.Interiors.Count > 0)
                {
                    interior = blu.GetIndoors();

                    //Should I do all of this in blu.GetIndoors()????? - warps, animal house stuff?
                    // Set warps for interior:
                    foreach (Warp warp in interior.warps)
                    {
                        int num1 = this.humanDoor.X + this.tileX;
                        warp.TargetX = num1;
                        int num2 = this.humanDoor.Y + this.tileY + 1;
                        warp.TargetY = num2;
                    }
                    
                    if(this.animalHouse)
                        if (this.animalDoorOpen)
                            this.yPositionOfAnimalDoor = CustomBuilding.openAnimalDoorPosition;
                        //if ((interior as AnimalHouse).incubatingEgg.Y > 0)
                           // interior.map.GetLayer("Front").Tiles[1, 2].TileIndex += Game1.player.ActiveObject.ParentSheetIndex == 180 || Game1.player.ActiveObject.ParentSheetIndex == 182 ? 2 : 1;
                        
                    return interior;
                }
            }
            //StardewValleyCustomMod.Logger.ExitGameImmediately($"{this.nameOfIndoorsWithoutUnique} not found for {this.buildingType} from {this.modName}");

            // Building interior not found, interior is null
            StardewValleyCustomMod.Logger.Log($"{this.nameOfIndoorsWithoutUnique} not found for {this.buildingType} from {this.modName}");
            return interior;
        }

        // Returns Building with the CustomBuilding values
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
            building.buildingType = this.modName + "_" + this.buildingType;
            //building.buildingType = this.buildingType;
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

        // Get information from custom blueprint that building does not store
        public void GetInfoFromBlueprint()
        {
            CustomBuildingBlueprint blu = StardewValleyCustomMod.Config.GetCustomBuildingBlueprint(this.buildingType);
            this.seasonal = blu.Seasonal;
            this.fileName = blu.FileName;
        }

        // TODO fix this for custom buildings that are bigger than the menu screen, ex. the winery
        // maybe zoom out some how?
        // add zoom function to the pic in the menu?
        public override void drawInMenu(SpriteBatch b, int x, int y)
        {
            if (this.animalHouse)
            {
                this.drawShadow(b, x, y);
                //b.Draw(this.texture, new Vector2((float)x, (float)y) + new Vector2((float)this.animalDoor.X, (float)(this.animalDoor.Y + 4)) * (float)Game1.tileSize, new Rectangle?(new Rectangle(16, 112, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
                //b.Draw(this.texture, new Vector2((float)x, (float)y) + new Vector2((float)this.animalDoor.X, (float)this.animalDoor.Y + 3.5f) * (float)Game1.tileSize, new Rectangle?(new Rectangle(0, 112, 16, 15)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((double)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000.0 - 1.0000000116861E-07));
                //b.Draw(this.texture, new Vector2((float)x, (float)y), new Rectangle?(new Rectangle(0, 0, 96, 112)), this.color, 0.0f, new Vector2(0.0f, 0.0f), 4f, SpriteEffects.None, 0.89f);

                b.Draw(this.AnimalDoorTexture, new Vector2((float)x, (float)y) + new Vector2((float)this.animalDoor.X, (float)(this.animalDoor.Y) + 1) * (float)Game1.tileSize, new Rectangle?(new Rectangle((int)this.animalDoorTileSheetCoords.X, (int)this.animalDoorTileSheetCoords.Y, this.animalDoorWidth, this.animalDoorHeight)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                b.Draw(this.texture, new Vector2((float)x, (float)y), new Rectangle?(this.texture.Bounds), this.color, 0.0f, new Vector2(0.0f, 0.0f), 4f, SpriteEffects.None, 0.89f);
                //StardewValleyCustomMod.Logger.Log($"Drawing animal house: {this.buildingType} - {this.texture.Bounds} - { this.AnimalDoorTexture.Bounds.ToString()}");
                //StardewValleyCustomMod.Logger.Log($"Animal Door: w-{this.animalDoorWidth} h-{this.animalDoorHeight} xy-{this.animalDoor.ToString()}");
            }
            else
                base.drawInMenu(b, x, y);
        }

        public override void draw(SpriteBatch b)
        {
            if(!this.animalHouse)
                base.draw(b);
            else
            {
                this.drawShadow(b, -1, -1);
                b.Draw(this.AnimalDoorTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX + this.animalDoor.X), (float)(this.tileY + this.animalDoor.Y - 1)) * (float)Game1.tileSize), new Rectangle?(new Rectangle((int)this.animalDoorTileSheetCoords.X, (int)this.animalDoorTileSheetCoords.Y, this.animalDoorWidth, this.animalDoorHeight)), Color.White * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.texture.Bounds), this.color * this.alpha, 0.0f, new Vector2(0.0f, 112f), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000f);

                //b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX + this.animalDoor.X), (float)(this.tileY + this.animalDoor.Y)) * (float)Game1.tileSize), new Rectangle?(new Rectangle(16, 112, 16, 16)), Color.White * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
                //b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((this.tileX + this.animalDoor.X) * Game1.tileSize), (float)((this.tileY + this.animalDoor.Y) * Game1.tileSize + this.yPositionOfAnimalDoor))), new Rectangle?(new Rectangle(0, 112, 16, 16)), Color.White * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((double)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000.0 - 1.0000000116861E-07));
                //b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(new Rectangle(0, 0, 96, 112)), this.color * this.alpha, 0.0f, new Vector2(0.0f, 112f), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000f);

                if (this.daysUntilUpgrade > 0)
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.getUpgradeSignLocation()), new Rectangle?(new Rectangle(367, 309, 16, 15)), Color.White * this.alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, (float)((double)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000.0 + 9.99999974737875E-05));
            }
        }
        /*
         *  if (this.tilesWide <= 8)
            {
                this.drawShadow(b, x, y);
                b.Draw(this.texture, new Vector2((float) x, (float) y), new Rectangle?(this.texture.Bounds), this.color, 0.0f, new Vector2(0.0f, 0.0f), (float) Game1.pixelZoom, SpriteEffects.None, 0.89f);
            }
            else
            {
                int num1 = Game1.tileSize + 11 * Game1.pixelZoom;
                int num2 = Game1.tileSize / 2 - Game1.pixelZoom;
                b.Draw(this.texture, new Vector2((float) (x + num1), (float) (y + num2)), new Rectangle?(new Rectangle(this.texture.Bounds.Center.X - 64, this.texture.Bounds.Bottom - 136 - 2, 122, 138)), this.color, 0.0f, new Vector2(0.0f, 0.0f), (float) Game1.pixelZoom, SpriteEffects.None, 0.89f);
            } 
         */

        public override void Update(GameTime time)
        {
            base.Update(time);
            if (this.animalHouse)
            {
                if (this.animalDoorMotion == 0)
                    return;
                if (this.animalDoorOpen && this.yPositionOfAnimalDoor <= Coop.openAnimalDoorPosition)
                {
                    this.animalDoorMotion = 0;
                    this.yPositionOfAnimalDoor = Coop.openAnimalDoorPosition;
                }
                else if (!this.animalDoorOpen && this.yPositionOfAnimalDoor >= 0)
                {
                    this.animalDoorMotion = 0;
                    this.yPositionOfAnimalDoor = 0;
                }
                this.yPositionOfAnimalDoor = this.yPositionOfAnimalDoor + this.animalDoorMotion;
            }
        }
        
        public override bool doAction(Vector2 tileLocation, StardewValley.Farmer who)
        {
            if (this.daysOfConstructionLeft > 0 || (double)tileLocation.X != (double)(this.tileX + this.animalDoor.X) || (double)tileLocation.Y != (double)(this.tileY + this.animalDoor.Y))
                return base.doAction(tileLocation, who);
            if (!this.animalDoorOpen)
                Game1.playSound("doorCreak");
            else
                Game1.playSound("doorCreakReverse");
            this.animalDoorOpen = !this.animalDoorOpen;
            this.animalDoorMotion = this.animalDoorOpen ? -2 : 2;
            return true;
        }
    }
}
