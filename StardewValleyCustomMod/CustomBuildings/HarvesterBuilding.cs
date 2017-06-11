// TODO use base games junimoharvesters
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
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValleyCustomMod.CustomBlueprints;

namespace StardewValleyCustomMod.CustomBuildings
{
    public class HarvesterBuilding : JunimoHut
    {
        public int HarvesterRadius;
        private Texture2D MinionTexture;
        public int MaxHarvesters = 3;

        [XmlIgnore]
        //public List<JunimoHarvester> myJunimos = new List<JunimoHarvester>();
        //public List<JunimoHarvester> myJunimos = new List<JunimoHarvester>();
        //[XmlIgnore]
        //public Point lastKnownCropLocation = Point.Zero;

        private Rectangle lightInteriorRect = new Rectangle(0, 0, 18, 17);
        private Rectangle bagRect = new Rectangle(0, 17, 15, 13);
        //public const int cropHarvestRadius = 8;
        //public Chest output;
        //public bool noHarvest;
        //public Rectangle sourceRect;
        private int junimoSendOutTimer;
        private bool wasLit;
        private JunimoHut junimoHut;
        public Texture2D extraTexture;

        public string modName;
        public bool seasonal;
        public string fileName;
        public string folderName;
        public float scalar;
        public bool customConstruction;
        public int daysToConstruct;
        public int[] constructionDayTextureList;
        public string[] specialProperties;
        public Texture2D ConstructionTexture;

        [XmlIgnore]
        public LocalizedContentManager Content;

        public HarvesterBuilding()
        {
            
        }

        // Create CustomBuilding from a blueprint and the location for the building
        public HarvesterBuilding(CustomBuildingBlueprint blu, Vector2 coord)
        {
            // Names
            this.modName = blu.ModName;
            this.fileName = blu.FileName;
            this.folderName = blu.FolderName;

            // Building Info
            this.buildingType = blu.BuildingName;
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

            this.HarvesterRadius = blu.HarvesterRadius;

            this.output = new Chest(true);

            //junimoHut = new JunimoHut(blu.convertCustomBlueprintToBluePrint(), coord);
        }

        public override Rectangle getRectForAnimalDoor()
        {
            return new Rectangle((1 + this.tileX) * Game1.tileSize, (this.tileY + 1) * Game1.tileSize, Game1.tileSize, Game1.tileSize);
        }

        public override Rectangle getSourceRectForMenu()
        {
            return new Rectangle(Utility.getSeasonNumber(Game1.currentSeason) * 48, 0, 48, 64);
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

            this.extraTexture = this.Content.Load<Texture2D>(this.fileName + "_extra");

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
                        animal.Value.reload();
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

        public override void dayUpdate(int dayOfMonth)
        {
            base.dayUpdate(dayOfMonth);

            /*// this is done in base right? TODO
            int constructionLeft = this.daysOfConstructionLeft;
            this.sourceRect = this.getSourceRectForMenu();
            this.myJunimos.Clear();
            this.wasLit = false;*/

            StardewValleyCustomMod.Logger.Log($"ConstructionDays: {this.daysOfConstructionLeft}");
            StardewValleyCustomMod.Logger.Log($"UpgradeDays: {this.daysUntilUpgrade}");

            // TODO is this where I should put it?
            if (this.customConstruction)
                this.UpdateConstructionTexture();
        }

        public void sendOutJunimos()
        {
            this.junimoSendOutTimer = 1000;
        }

        public override void performActionOnConstruction(GameLocation location)
        {
            base.performActionOnConstruction(location);
            this.sendOutJunimos();
        }

        public override void performActionOnPlayerLocationEntry()
        {
            base.performActionOnPlayerLocationEntry();
            if (Game1.timeOfDay < 2000 || Game1.timeOfDay >= 2400 || Game1.IsWinter)
                return;
            Game1.currentLightSources.Add(new LightSource(4, new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)) * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), 0.5f)
            {
                identifier = this.tileX + this.tileY * 777
            });
            AmbientLocationSounds.addSound(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)), 1);
            this.wasLit = true;
        }

        public int getUnusedJunimoNumber()
        {
            for (int index = 0; index < this.MaxHarvesters; ++index)
            {
                if (index >= this.myJunimos.Count<JunimoHarvester>())
                    return index;
                bool flag = false;
                foreach (JunimoHarvester junimo in this.myJunimos)
                {
                    if (junimo.whichJunimoFromThisHut == index)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    return index;
            }
            return 2;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if (this.junimoSendOutTimer <= 0)
                return;
            this.junimoSendOutTimer = this.junimoSendOutTimer - time.ElapsedGameTime.Milliseconds;
            if (this.junimoSendOutTimer > 0 || this.myJunimos.Count<JunimoHarvester>() >= this.MaxHarvesters || (Game1.IsWinter || Game1.isRaining) || (!this.areThereMatureCropsWithinRadius() || Game1.farmEvent != null) || this.isUnderConstruction())
                return;
            JunimoHarvester junimoHarvester = new JunimoHarvester(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)) * (float)Game1.tileSize + new Vector2(0.0f, (float)(Game1.tileSize / 2)), this, this.getUnusedJunimoNumber());
            Game1.getFarm().characters.Add((NPC)junimoHarvester);
            this.myJunimos.Add(junimoHarvester);
            this.junimoSendOutTimer = 1000;
            if (!Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1))), Game1.tileSize, (GameLocation)Game1.getFarm()))
                return;
            try
            {
                Game1.playSound("junimoMeep1");
            }
            catch (Exception ex)
            {
            }
        }

        public bool areThereMatureCropsWithinRadius()
        {
            Farm farm = Game1.getFarm();
            for (int index1 = this.tileX + 1 - this.HarvesterRadius; index1 < this.tileX + 2 + this.HarvesterRadius; ++index1)
            {
                for (int index2 = this.tileY - this.HarvesterRadius + 1; index2 < this.tileY + 2 + this.HarvesterRadius; ++index2)
                {
                    if (farm.isCropAtTile(index1, index2) && (farm.terrainFeatures[new Vector2((float)index1, (float)index2)] as HoeDirt).readyForHarvest())
                    {
                        this.lastKnownCropLocation = new Point(index1, index2);
                        return true;
                    }
                }
            }
            this.lastKnownCropLocation = Point.Zero;
            return false;
        }

        public override void performTenMinuteAction(int timeElapsed)
        {
            base.performTenMinuteAction(timeElapsed);
            for (int index = this.myJunimos.Count - 1; index >= 0; --index)
            {
                if (!Game1.getFarm().characters.Contains((NPC)this.myJunimos[index]))
                    this.myJunimos.RemoveAt(index);
                else
                    this.myJunimos[index].pokeToHarvest();
            }
            if (this.myJunimos.Count<JunimoHarvester>() < this.MaxHarvesters && Game1.timeOfDay < 1900)
                this.junimoSendOutTimer = 1;
            if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && (!Game1.IsWinter && Utility.getLightSource(this.tileX + this.tileY * 777) == null) && Game1.random.NextDouble() < 0.2)
            {
                Game1.currentLightSources.Add(new LightSource(4, new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)) * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), 0.5f)
                {
                    identifier = this.tileX + this.tileY * 777
                });
                AmbientLocationSounds.addSound(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)), 1);
                this.wasLit = true;
            }
            else
            {
                if (Game1.timeOfDay != 2400 || Game1.IsWinter)
                    return;
                Utility.removeLightSource(this.tileX + this.tileY * 777);
                AmbientLocationSounds.removeSound(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)));
            }
        }

        public override bool doAction(Vector2 tileLocation, StardewValley.Farmer who)
        {
            if (!who.IsMainPlayer || (double)tileLocation.X < (double)this.tileX || ((double)tileLocation.X >= (double)(this.tileX + this.tilesWide) || (double)tileLocation.Y < (double)this.tileY) || ((double)tileLocation.Y >= (double)(this.tileY + this.tilesHigh) || this.output == null))
                return base.doAction(tileLocation, who);
            Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(this.output.items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(this.output.grabItemFromInventory), (string)null, new ItemGrabMenu.behaviorOnItemSelect(this.output.grabItemFromChest), false, true, true, true, true, 1, (Item)null, 1, (object)this);
            return true;
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

        public override void draw(SpriteBatch b)
        {
            // Construction
            if (this.daysOfConstructionLeft > 0 || this.daysUntilUpgrade > 0)
                if (this.customConstruction)
                    this.DrawInCustomConstruction(b);
                else
                    this.drawInConstruction(b);
            else
            {
                this.drawShadow(b, -1, -1);
                //b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.sourceRect), this.color * this.alpha, 0.0f, new Vector2(0.0f, (float)this.texture.Bounds.Height), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh - 1) * Game1.tileSize) / 10000f);
                b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.texture.Bounds), this.color * this.alpha, 0.0f, new Vector2(0.0f, (float)this.texture.Bounds.Height), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh - 1) * Game1.tileSize) / 10000f);
                if (!this.output.isEmpty())
                    b.Draw(this.extraTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize + Game1.tileSize * 2 + Game1.pixelZoom * 3), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize - Game1.tileSize / 2))), new Rectangle?(this.bagRect), this.color * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh - 1) * Game1.tileSize + 1) / 10000f);
                if (Game1.timeOfDay < 2000 || Game1.timeOfDay >= 2400 || (Game1.IsWinter || !this.wasLit))
                    return;
                b.Draw(this.extraTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize + Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize - Game1.tileSize))), new Rectangle?(this.lightInteriorRect), this.color * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh - 1) * Game1.tileSize + 1) / 10000f);
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
            StardewValleyCustomMod.Logger.Log($"{this.nameOfIndoorsWithoutUnique} not found for {this.buildingType} from {this.modName}");
            return interior;
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

        public void DrawInCustomConstruction(SpriteBatch b)
        {
            b.Draw(this.ConstructionTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.texture.Bounds), this.color * this.alpha, 0.0f, new Vector2(0.0f, 112f), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000f);
        }

        public void UpdateConstructionTexture()
        {
            this.ConstructionTexture = this.Content.Load<Texture2D>(this.fileName + "_construction_" + this.constructionDayTextureList[this.daysToConstruct - this.daysOfConstructionLeft]);
        }
    }
}
