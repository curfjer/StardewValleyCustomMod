using System;
using System.Collections.Generic;
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
        public bool seasonal;
        private LocalizedContentManager Content;

        public CustomBuilding()
        {

        }

        public CustomBuilding(Building building)
        {
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
            this.buildingType = building.buildingType;
            this.nameOfIndoors = building.nameOfIndoors;
            this.baseNameOfIndoors = building.baseNameOfIndoors;
            this.nameOfIndoorsWithoutUnique = building.nameOfIndoorsWithoutUnique;
            this.humanDoor = building.humanDoor;
            this.animalDoor = building.animalDoor;
            this.animalDoorOpen = building.animalDoorOpen;
            this.magical = building.magical;
            this.owner = building.owner;
        }

        public override void load()
        {
            this.Content = new LocalizedContentManager(Game1.content.ServiceProvider, "Mods\\StardewValleyCustomMod\\CustomBuildings");
            this.GetInfoFromBlueprint();
            if (this.seasonal)
            {
                this.texture = this.Content.Load<Texture2D>(this.buildingType + "_" + Game1.currentSeason);
            }
            else
            {
                this.texture = this.Content.Load<Texture2D>(this.buildingType);// Create a custom building then convert back to orig building
            }            
            GameLocation indoors1 = this.GetIndoors();
            if (indoors1 == null)
                return;
            indoors1.characters = this.indoors.characters;
            indoors1.objects = this.indoors.objects;
            indoors1.terrainFeatures = this.indoors.terrainFeatures;
            //indoors1.IsFarm = true;
            //indoors1.IsOutdoors = false;
            indoors1.isStructure = true;
            indoors1.uniqueName = indoors1.name + (object)(this.tileX * 2000 + this.tileY);
            indoors1.numberOfSpawnedObjectsOnMap = this.indoors.numberOfSpawnedObjectsOnMap;
            if (this.indoors.GetType().Equals(typeof(AnimalHouse)))
            {
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
            foreach (Warp warp in this.indoors.warps)
            {
                int num1 = this.humanDoor.X + this.tileX;
                warp.TargetX = num1;
                int num2 = this.humanDoor.Y + this.tileY + 1;
                warp.TargetY = num2;
            }
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
            if (!(this.indoors is AnimalHouse))
                return;
            AnimalHouse indoors2 = this.indoors as AnimalHouse;
            string str = this.buildingType.Split(' ')[0];
            if (!(str == "Big"))
            {
                if (str == "Deluxe")
                    indoors2.animalLimit = 12;
                else
                    indoors2.animalLimit = 4;
            }
            else
                indoors2.animalLimit = 8;
        }

        /*
        protected override GameLocation getIndoors()
        {
            if (this.buildingType.Equals("Slime Hutch"))
            {
                if (this.indoors != null)
                    this.nameOfIndoorsWithoutUnique = this.indoors.name;
                if (this.nameOfIndoorsWithoutUnique == "Slime Hutch")
                    this.nameOfIndoorsWithoutUnique = "SlimeHutch";
                GameLocation gameLocation = (GameLocation)new SlimeHutch(Game1.game1.xTileContent.Load<Map>("Maps\\" + this.nameOfIndoorsWithoutUnique), this.buildingType);
                gameLocation.IsFarm = true;
                gameLocation.isStructure = true;
                foreach (Warp warp in gameLocation.warps)
                {
                    int num1 = this.humanDoor.X + this.tileX;
                    warp.TargetX = num1;
                    int num2 = this.humanDoor.Y + this.tileY + 1;
                    warp.TargetY = num2;
                }
                return gameLocation;
            }
            if (this.buildingType.Equals("Shed"))
            {
                if (this.indoors != null)
                    this.nameOfIndoorsWithoutUnique = this.indoors.name;
                if (this.nameOfIndoorsWithoutUnique == "Shed")
                    this.nameOfIndoorsWithoutUnique = "Shed";
                GameLocation gameLocation = (GameLocation)new Shed(Game1.game1.xTileContent.Load<Map>("Maps\\" + this.nameOfIndoorsWithoutUnique), this.buildingType);
                gameLocation.IsFarm = true;
                gameLocation.isStructure = true;
                foreach (Warp warp in gameLocation.warps)
                {
                    int num1 = this.humanDoor.X + this.tileX;
                    warp.TargetX = num1;
                    int num2 = this.humanDoor.Y + this.tileY + 1;
                    warp.TargetY = num2;
                }
                return gameLocation;
            }
            if (this.nameOfIndoorsWithoutUnique == null || this.nameOfIndoorsWithoutUnique.Length <= 0 || this.nameOfIndoorsWithoutUnique.Equals("null"))
                return (GameLocation)null;
            //GameLocation gameLocation1 = new GameLocation(Game1.game1.xTileContent.Load<Map>("Maps\\" + this.nameOfIndoorsWithoutUnique), this.buildingType);
            GameLocation gameLocation1 = new GameLocation(this.Content.Load<Map>("BuildingInterior\\" + this.nameOfIndoorsWithoutUnique), this.buildingType);
            gameLocation1.IsFarm = true;
            gameLocation1.isStructure = true;
            if (gameLocation1.name.Equals("Greenhouse"))
                gameLocation1.terrainFeatures = new SerializableDictionary<Vector2, TerrainFeature>();
            foreach (Warp warp in gameLocation1.warps)
            {
                int num1 = this.humanDoor.X + this.tileX;
                warp.TargetX = num1;
                int num2 = this.humanDoor.Y + this.tileY + 1;
                warp.TargetY = num2;
            }
            if (gameLocation1 is AnimalHouse)
            {
                AnimalHouse animalHouse = gameLocation1 as AnimalHouse;
                string str = this.buildingType.Split(' ')[0];
                animalHouse.animalLimit = str == "Big" ? 8 : (str == "Deluxe" ? 12 : 4);
            }
            return gameLocation1;
        }*/

        public GameLocation GetIndoors()
        {
            GameLocation interior = null;

            foreach (CustomBuildingBlueprint blu in StardewValleyCustomMod.Config.BlueprintList)
            {
                if (blu.name.Equals(this.buildingType))
                {
                    interior = blu.GetIndoors();

                    foreach (Warp warp in interior.warps)
                    {
                        int num1 = this.humanDoor.X + this.tileX;
                        warp.TargetX = num1;
                        int num2 = this.humanDoor.Y + this.tileY + 1;
                        warp.TargetY = num2;
                    }
                    return interior;
                }
            }

            return interior;
            /*
            try
            {
                GameLocation loc;
                StardewValleyCustomMod.ContentRegistry.RegisterXnb(wineryPath, wineryPath);
                xTile.Map map = Game1.content.Load<xTile.Map>(wineryPath);
                switch (location.Type)
                {
                    case "Cellar":
                        loc = new StardewValley.Locations.Cellar(map, location.MapName);
                        loc.objects = new SerializableDictionary<Microsoft.Xna.Framework.Vector2, StardewValley.Object>();
                        break;
                    case "BathHousePool":
                        loc = new StardewValley.Locations.BathHousePool(map, location.MapName);
                        break;
                    case "Decoratable":
                        loc = new StardewValley.Locations.DecoratableLocation(map, location.MapName);
                        break;
                    case "Desert":
                        loc = new StardewValley.Locations.Desert(map, location.MapName);
                        break;
                    case "Greenhouse":
                        loc = new StardewValley.Locations.Greenhouse(map, location.MapName);
                        break;
                    case "Sewer":
                        loc = new StardewValley.Locations.Sewer(map, location.MapName);
                        break;
                    default:
                        loc = new GameLocation(map, location.MapName);
                        break;
                }
                //loc.isOutdoors = location.Outdoor;
                //loc.isFarm = location.Farmable;
                loc = new StardewValley.Locations.Cellar(map, "WineryInterior");
                loc.objects = new SerializableDictionary<Microsoft.Xna.Framework.Vector2, StardewValley.Object>();
                loc.isOutdoors = false;
                loc.isFarm = false;
                Game1.locations.Add(loc);
                StardewValleyCustomMod.Logger.Log("Adding Winery Tilesheet...");
            }
            catch (Exception err)
            {
                StardewValleyCustomMod.Logger.ExitGameImmediately("Unable to add custom location, a unexpected error occured: " + "Winery" + err);
            }

            

            GameLocation gameLocation1 = new GameLocation(this.Content.Load<Map>("BuildingInterior\\" + this.nameOfIndoorsWithoutUnique), this.buildingType);
            gameLocation1.IsFarm = true;
            gameLocation1.isStructure = true;
            */
            /*
            foreach (Warp warp in gameLocation1.warps)
            {
                int num1 = this.humanDoor.X + this.tileX;
                warp.TargetX = num1;
                int num2 = this.humanDoor.Y + this.tileY + 1;
                warp.TargetY = num2;
            }
            return gameLocation1;*/

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
            building.buildingType = this.buildingType;
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
            this.seasonal = blu.seasonal;
        }
    }
}
