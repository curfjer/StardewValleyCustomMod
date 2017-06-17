using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using CustomFarmBuildings.CustomBlueprints;
using xTile;


//TODO
/* Add to config list
 * - 2 building lists
 *  - custom buildings
 *  - custom animal buildings
 * 
 * 
 * 
 */
namespace CustomFarmBuildings
{
    public class AnimalBuilding : CustomBuilding
    {
        public bool animalHouse;
        public static int openAnimalDoorPosition = -Game1.tileSize + Game1.pixelZoom * 3;
        public const int closedAnimalDoorPosition = 0;
        public int yPositionOfAnimalDoor;
        public int animalDoorMotion;
        public Vector2 animalDoorTileSheetCoords;
        public int animalDoorHeight;
        public int animalDoorWidth;
        public Texture2D AnimalDoorTexture;
        public Point AnimalDoorTileCoord;
        public int AnimalDoorOverhang = 3; // TODO default 3, user can change? need to fix current door, does it have be divisible by 16?

        public AnimalBuilding()
        {

        }

        public AnimalBuilding(Building building) : base(building)
        {
        }

        public AnimalBuilding(CustomBuildingBlueprint blu, Vector2 coord) : base(blu, coord)
        {
            this.animalHouse = true; // TODO do I need this since if it is this class then obviosly it's an animal house

            // Door
            this.animalDoorTileSheetCoords = new Vector2(0, 0);
            this.animalDoorWidth = blu.AnimalDoorWidth;
            this.animalDoorHeight = blu.AnimalDoorHeight;
            this.AnimalDoorTexture = blu.AnimalDoorTexture;
            this.animalDoorOpen = false;
            this.AnimalDoorTileCoord = blu.AnimalDoorTileCoord;
            this.animalDoor = new Point(this.AnimalDoorTileCoord.X, this.AnimalDoorTileCoord.Y + this.animalDoorHeight / this.textureBitSize - 1); // TODO uses pixelzoom change to scalar? 16, 32, 64 bit check
        }

        public override void load()
        {
            base.load();
            this.AnimalDoorTexture = this.Content.Load<Texture2D>(this.fileName + "_AnimalDoor");
        }
        /*
        protected override GameLocation getIndoors()
        {
            if (this.indoors != null)
                this.nameOfIndoorsWithoutUnique = this.indoors.name;
            string indoorsWithoutUnique1 = this.nameOfIndoorsWithoutUnique;
            if (!(indoorsWithoutUnique1 == "Big Coop"))
            {
                if (indoorsWithoutUnique1 == "Deluxe Coop")
                    this.nameOfIndoorsWithoutUnique = "Coop3";
            }
            else
                this.nameOfIndoorsWithoutUnique = "Coop2";
            GameLocation gameLocation = (GameLocation)new AnimalHouse(Game1.game1.xTileContent.Load<Map>("Maps\\" + this.nameOfIndoorsWithoutUnique), this.buildingType);
            gameLocation.IsFarm = true;
            gameLocation.isStructure = true;
            string indoorsWithoutUnique2 = this.nameOfIndoorsWithoutUnique;
            if (!(indoorsWithoutUnique2 == "Big Coop"))
            {
                if (indoorsWithoutUnique2 == "Deluxe Coop")
                    (gameLocation as AnimalHouse).animalLimit = 12;
            }
            else
                (gameLocation as AnimalHouse).animalLimit = 8;
            foreach (Warp warp in gameLocation.warps)
            {
                int num1 = this.humanDoor.X + this.tileX;
                warp.TargetX = num1;
                int num2 = this.humanDoor.Y + this.tileY + 1;
                warp.TargetY = num2;
            }
            if (this.animalDoorOpen)
                this.yPositionOfAnimalDoor = Coop.openAnimalDoorPosition;
            if ((gameLocation as AnimalHouse).incubatingEgg.Y > 0)
                gameLocation.map.GetLayer("Front").Tiles[1, 2].TileIndex += Game1.player.ActiveObject.ParentSheetIndex == 180 || Game1.player.ActiveObject.ParentSheetIndex == 182 ? 2 : 1;
            return gameLocation;
        }*/

        public override void performActionOnConstruction(GameLocation location)
        {
            base.performActionOnConstruction(location);
            this.indoors.objects.Add(new Vector2(3f, 3f), new StardewValley.Object(new Vector2(3f, 3f), 99, false)
            {
                fragility = 2
            });
            this.daysOfConstructionLeft = this.daysToConstruct;
        }

        public override void performActionOnUpgrade(GameLocation location)
        {
            (this.indoors as AnimalHouse).animalLimit += 4;
            if ((this.indoors as AnimalHouse).animalLimit == 8)
            {
                this.indoors.objects.Add(new Vector2(2f, 3f), new StardewValley.Object(new Vector2(2f, 3f), 104, false)
                {
                    fragility = 2
                });
                this.indoors.moveObject(1, 3, 14, 7);
            }
            else
            {
                this.indoors.moveObject(14, 7, 21, 7);
                this.indoors.moveObject(14, 8, 21, 8);
                this.indoors.moveObject(14, 4, 20, 4);
            }
        }

        // TODO needs a better check for which tile is part of the door, if the door varies in width
        // Check for player interactions
        public override bool doAction(Vector2 tileLocation, StardewValley.Farmer who)
        {
            if (this.daysOfConstructionLeft > 0 || (double)tileLocation.X < (double)(this.tileX + this.animalDoor.X) || (double)tileLocation.X >= (double)(this.tileX + this.animalDoor.X + this.animalDoorWidth / (Game1.tileSize / Game1.pixelZoom)) || (double)tileLocation.Y != (double)(this.tileY + this.animalDoor.Y))
                return base.doAction(tileLocation, who);
            //base.doAction(tileLocation, who); // this will return true, does that leave this method??? TODO

            CustomFarmBuildings.Logger.Log($"Selected Tiled: {tileLocation}");
            CustomFarmBuildings.Logger.Log($"tileX: {this.tileX}, animalDoorX: {this.animalDoor.X}, animalDoorW:{this.animalDoorWidth}");
            // Animal Door Interaction
            if (!this.animalDoorOpen)
                Game1.playSound("doorCreak");
            else
                Game1.playSound("doorCreakReverse");
            this.animalDoorOpen = !this.animalDoorOpen;
            this.animalDoorMotion = this.animalDoorOpen ? -2 : 2; // TODO change to different values?

            CustomFarmBuildings.Logger.Log("Checking for farm animals...");

            
            foreach (long key in Game1.getFarm().animals.Keys)
            {
                FarmAnimal animal;
                Game1.getFarm().animals.TryGetValue(key, out animal);
                CustomFarmBuildings.Logger.Log($"Name: {animal.displayName}");
            }
            return true;
        }

        public override Rectangle getSourceRectForMenu()
        {
            return new Rectangle(0, 0, this.texture.Bounds.Width, this.texture.Bounds.Height - 16);
        }
        /*
        // I cant change this since I convert to building then add it to the farms list, or maybe I can???
        public override bool doAction(Vector2 tileLocation, Farmer who)
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
        }*/

        public override void updateWhenFarmNotCurrentLocation(GameTime time)
        {
            base.updateWhenFarmNotCurrentLocation(time);
            ((AnimalHouse)this.indoors).updateWhenNotCurrentLocation((Building)this, time);
        }

        public override void dayUpdate(int dayOfMonth)
        {
            base.dayUpdate(dayOfMonth);
            if (this.daysOfConstructionLeft <= 0)
            {
                if ((this.indoors as AnimalHouse).incubatingEgg.Y > 0)
                {
                    --(this.indoors as AnimalHouse).incubatingEgg.X;
                    if ((this.indoors as AnimalHouse).incubatingEgg.X <= 0)
                    {
                        long newId = MultiplayerUtility.getNewID();
                        FarmAnimal farmAnimal = new FarmAnimal((this.indoors as AnimalHouse).incubatingEgg.Y == 442 ? "Duck" : ((this.indoors as AnimalHouse).incubatingEgg.Y == 180 || (this.indoors as AnimalHouse).incubatingEgg.Y == 182 ? "BrownChicken" : ((this.indoors as AnimalHouse).incubatingEgg.Y == 107 ? "Dinosaur" : "Chicken")), newId, this.owner);
                        (this.indoors as AnimalHouse).incubatingEgg.X = 0;
                        (this.indoors as AnimalHouse).incubatingEgg.Y = -1;
                        this.indoors.map.GetLayer("Front").Tiles[1, 2].TileIndex = 45;
                        ((AnimalHouse)this.indoors).animals.Add(newId, farmAnimal);
                    }
                }
                if ((this.indoors as AnimalHouse).animalLimit == 16)
                {
                    int num = Math.Min((this.indoors as AnimalHouse).animals.Count - this.indoors.numberOfObjectsWithName("Hay"), (Game1.getLocationFromName("Farm") as Farm).piecesOfHay);
                    (Game1.getLocationFromName("Farm") as Farm).piecesOfHay -= num;
                    for (int index = 0; index < 16 && num > 0; ++index)
                    {
                        Vector2 key = new Vector2((float)(6 + index), 3f);
                        if (!this.indoors.objects.ContainsKey(key))
                            this.indoors.objects.Add(key, new StardewValley.Object(178, 1, false, -1, 0));
                        --num;
                    }
                }
            }
            this.currentOccupants = ((AnimalHouse)this.indoors).animals.Count;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            
            // Update the animal door
            if (this.animalHouse)
            {
                if (this.animalDoorMotion != 0)
                {
                    if (this.animalDoorOpen && this.yPositionOfAnimalDoor <= -(this.animalDoorHeight * Game1.pixelZoom) + Game1.pixelZoom * 3) //pixelZoom * 3 is what base game uses, so keeping it consistent with the base game
                    {
                        this.animalDoorMotion = 0;
                        this.yPositionOfAnimalDoor = -(this.animalDoorHeight * Game1.pixelZoom) + Game1.pixelZoom * this.AnimalDoorOverhang;
                    }
                    else if (!this.animalDoorOpen && this.yPositionOfAnimalDoor >= 0)
                    {
                        this.animalDoorMotion = 0;
                        this.yPositionOfAnimalDoor = 0;
                    }
                    this.yPositionOfAnimalDoor = this.yPositionOfAnimalDoor + this.animalDoorMotion;
                }
            }
        }

        public override void upgrade()
        {
            base.upgrade();
            if (this.buildingType.Equals("Big Coop"))
            {
                this.indoors.moveObject(2, 3, 14, 8);
                this.indoors.moveObject(1, 3, 14, 7);
                this.indoors.moveObject(10, 4, 14, 4);
                this.indoors.objects.Add(new Vector2(2f, 3f), new StardewValley.Object(new Vector2(2f, 3f), 101, false));
                if (!Game1.player.hasOrWillReceiveMail("incubator"))
                    Game1.mailbox.Enqueue("incubator");
            }
            if ((this.indoors as AnimalHouse).animalLimit == 8)
                return;
            this.indoors.moveObject(14, 7, 21, 7);
            this.indoors.moveObject(14, 8, 21, 8);
            this.indoors.moveObject(14, 4, 20, 4);
        }

        public override void drawInMenu(SpriteBatch b, int x, int y)
        {
            this.drawShadow(b, x, y);

            b.Draw(this.AnimalDoorTexture, new Vector2((float)x, (float)y) + new Vector2((float)this.animalDoor.X * (float)Game1.tileSize, ((float)this.texture.Height - this.AnimalDoorTexture.Height) * Game1.pixelZoom), new Rectangle?(new Rectangle((int)this.animalDoorTileSheetCoords.X, (int)this.animalDoorTileSheetCoords.Y, this.animalDoorWidth, this.animalDoorHeight)), Color.White, 0.0f, Vector2.Zero, this.scalar, SpriteEffects.None, 1f);
            b.Draw(this.texture, new Vector2((float)x, (float)y), new Rectangle?(this.texture.Bounds), this.color, 0.0f, new Vector2(0.0f, 0.0f), this.scalar, SpriteEffects.None, 0f);
        }

        public override Vector2 getUpgradeSignLocation()
        {
            return new Vector2((float)this.tileX, (float)(this.tileY + 1)) * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize * 2), (float)Game1.pixelZoom);
        }

        public override void draw(SpriteBatch b)
        {
            // Construction
            if (this.daysOfConstructionLeft > 0 || this.daysUntilUpgrade > 0)
                if (this.customConstruction)
                    this.DrawInCustomConstruction(b);
                else
                    this.drawInConstruction(b);

            this.drawShadow(b, -1, -1);

            //b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX + this.animalDoor.X), (float)(this.tileY + this.animalDoor.Y - 1)) * (float)Game1.tileSize), new Rectangle?(new Rectangle(32, 112, 32, 16)), Color.White * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);
            //b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX + this.animalDoor.X), (float)(this.tileY + this.animalDoor.Y)) * (float)Game1.tileSize), new Rectangle?(new Rectangle(64, 112, 32, 16)), Color.White * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-06f);

            b.Draw(this.AnimalDoorTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX + this.animalDoor.X), (float)((this.tileY + this.tilesHigh))) * Game1.tileSize) - new Vector2(0, this.AnimalDoorTexture.Height) * this.textureScalar, new Rectangle?(new Rectangle((int)this.animalDoorTileSheetCoords.X + this.animalDoorWidth, (int)this.animalDoorTileSheetCoords.Y, this.animalDoorWidth, this.AnimalDoorTexture.Height)), Color.White * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            b.Draw(this.AnimalDoorTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX + this.animalDoor.X), (float)((this.tileY + this.tilesHigh))) * Game1.tileSize - new Vector2((float)0, (float)(this.AnimalDoorTexture.Height) * Game1.pixelZoom - this.yPositionOfAnimalDoor)), new Rectangle?(new Rectangle((int)this.animalDoorTileSheetCoords.X, (int)this.animalDoorTileSheetCoords.Y, this.animalDoorWidth, this.AnimalDoorTexture.Height)), Color.White * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0000001f);
            b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.texture.Bounds), this.color * this.alpha, 0.0f, new Vector2(0.0f, 112f), 4f, SpriteEffects.None, (float)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000f);

            // Upgrading
            if (this.daysUntilUpgrade > 0)
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.getUpgradeSignLocation()), new Rectangle?(new Rectangle(367, 309, 16, 15)), Color.White * this.alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, (float)((double)((this.tileY + this.tilesHigh) * Game1.tileSize) / 10000.0 + 9.99999974737875E-05));
        }
    }
}
