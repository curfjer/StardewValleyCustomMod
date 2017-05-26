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
        private LocalizedContentManager Content;

        public CustomBuilding()
        {

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
            string buildingPath = Path.Combine("Mods\\StardewValleyCustomMod", "buildingMods", this.modName, "Buildings");
            this.Content = new LocalizedContentManager(Game1.content.ServiceProvider, buildingPath);
            this.GetInfoFromBlueprint();

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

                    // Set warps for interior:
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
    }
}
