using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValleyCustomMod.CustomBuildings;

namespace StardewValleyCustomMod
{
    public class Harvester : JunimoHarvester
    {
        private float alphaH = 1f;
        private Vector2 motionH = Vector2.Zero;
        private float alphaChangeH;
        private Rectangle nextPositionH;
        private Color colorH;
        private HarvesterBuilding homeH;
        private bool destroyH;
        private Item lastItemHarvestedH;
        private Task backgroundTaskH;
        //public int whichJunimoFromThisHut;
        private int harvestTimerH;

        
        public Harvester(Vector2 position, HarvesterBuilding myHome, int whichJunimoNumberFromThisHut)
        {
            this.homeH = myHome;
            this.whichJunimoFromThisHut = whichJunimoNumberFromThisHut;
            Random random = new Random(myHome.tileX + myHome.tileY * 777 + whichJunimoNumberFromThisHut);
            this.nextPositionH = this.GetBoundingBox();
            this.breather = false;
            this.speed = 3; 
            //var myField = typeof(Harvester).BaseType
            //                 .BaseType
            //                 .GetField("harvestTimer", BindingFlags.Instance | BindingFlags.NonPublic);
            //myField.SetValue(this, 5);
            this.forceUpdateTimer = 9999;
            this.collidesWithOtherCharacters = true;
            this.ignoreMovementAnimation = true;
            this.farmerPassesThrough = true;
            this.scale = 0.75f;
            this.alphaH = 0.0f;
            this.hideShadow = true;
            this.alphaChangeH = 0.05f;
            if (random.NextDouble() < 0.25)
            {
                switch (random.Next(8))
                {
                    case 0:
                        this.colorH = Color.Red;
                        break;
                    case 1:
                        this.colorH = Color.Goldenrod;
                        break;
                    case 2:
                        this.colorH = Color.Yellow;
                        break;
                    case 3:
                        this.colorH = Color.Lime;
                        break;
                    case 4:
                        this.colorH = new Color(0, (int)byte.MaxValue, 180);
                        break;
                    case 5:
                        this.colorH = new Color(0, 100, (int)byte.MaxValue);
                        break;
                    case 6:
                        this.colorH = Color.MediumPurple;
                        break;
                    case 7:
                        this.colorH = Color.Salmon;
                        break;
                }
                if (random.NextDouble() < 0.01)
                    this.colorH = Color.White;
            }
            else
            {
                switch (random.Next(8))
                {
                    case 0:
                        this.colorH = Color.LimeGreen;
                        break;
                    case 1:
                        this.colorH = Color.Orange;
                        break;
                    case 2:
                        this.colorH = Color.LightGreen;
                        break;
                    case 3:
                        this.colorH = Color.Tan;
                        break;
                    case 4:
                        this.colorH = Color.GreenYellow;
                        break;
                    case 5:
                        this.colorH = Color.LawnGreen;
                        break;
                    case 6:
                        this.colorH = Color.PaleGreen;
                        break;
                    case 7:
                        this.colorH = Color.Turquoise;
                        break;
                }
            }
            this.willDestroyObjectsUnderfoot = false;
            this.currentLocation = (GameLocation)Game1.getFarm();
            Vector2 v = Vector2.Zero;
            switch (whichJunimoNumberFromThisHut)
            {
                case 0:
                    v = Utility.recursiveFindOpenTileForCharacter((Character)this, this.currentLocation, new Vector2((float)(this.homeH.tileX + 1), (float)(this.homeH.tileY + this.homeH.tilesHigh + 1)), 30);
                    break;
                case 1:
                    v = Utility.recursiveFindOpenTileForCharacter((Character)this, this.currentLocation, new Vector2((float)(this.homeH.tileX - 1), (float)this.homeH.tileY), 30);
                    break;
                case 2:
                    v = Utility.recursiveFindOpenTileForCharacter((Character)this, this.currentLocation, new Vector2((float)(this.homeH.tileX + this.homeH.tilesWide), (float)this.homeH.tileY), 30);
                    break;
            }
            if (v != Vector2.Zero)
                this.controller = new PathFindController((Character)this, this.currentLocation, Utility.Vector2ToPoint(v), -1, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100);
            if (this.controller == null || this.controller.pathToEndPoint == null)
            {
                this.pathfindToRandomSpotAroundHut();
                if (this.controller == null || this.controller.pathToEndPoint == null)
                    this.destroyH = true;
            }
            this.collidesWithOtherCharacters = false;
        }

        public new void reachFirstDestinationFromHut(Character c, GameLocation l)
        {
            this.tryToHarvestHere();
        }

        public new void tryToHarvestHere()
        {
            if (this.currentLocation == null)
                return;
            if (this.currentLocation.terrainFeatures.ContainsKey(this.getTileLocation()) && this.currentLocation.terrainFeatures[this.getTileLocation()] is HoeDirt && (this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).readyForHarvest())
                this.harvestTimerH = 2000;
            else
                this.pokeToHarvest();
        }

        public new void pokeToHarvest()
        {
            if (!this.homeH.isTilePassable(this.getTileLocation()))
            {
                this.destroyH = true;
            }
            else
            {
                if (this.harvestTimerH > 0 || Game1.random.NextDouble() >= 0.7)
                    return;
                this.pathfindToNewCrop();
            }
        }

        public override bool shouldCollideWithBuildingLayer(GameLocation location)
        {
            return true;
        }

        public new void fadeAway()
        {
            this.collidesWithOtherCharacters = false;
            this.alphaChangeH = -0.015f;
        }

        public new void setAlpha(float a)
        {
            this.alphaH = a;
        }

        public new void fadeBack()
        {
            this.alphaH = 0.0f;
            this.alphaChangeH = 0.02f;
            this.isInvisible = false;
        }

        public new void setMoving(int xSpeed, int ySpeed)
        {
            this.motionH.X = (float)xSpeed;
            this.motionH.Y = (float)ySpeed;
        }

        public new void setMoving(Vector2 motion)
        {
            this.motionH = motion;
        }

        public override void Halt()
        {
            base.Halt();
            this.motionH = Vector2.Zero;
        }

        public new void junimoReachedHut(Character c, GameLocation l)
        {
            this.controller = (PathFindController)null;
            this.motionH.X = 0.0f;
            this.motionH.Y = -1f;
            this.destroyH = true;
        }

        public new bool foundCropEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
        {
            return location.isCropAtTile(currentNode.x, currentNode.y) && (location.terrainFeatures[new Vector2((float)currentNode.x, (float)currentNode.y)] as HoeDirt).readyForHarvest();
        }

        public new void pathFindToNewCrop_doWork()
        {
            if (Game1.timeOfDay > 1900)
            {
                if (this.controller != null)
                    return;
                this.returnToJunimoHut(this.currentLocation);
            }
            else if (Game1.random.NextDouble() < 0.035 || this.homeH.noHarvest)
            {
                this.pathfindToRandomSpotAroundHut();
            }
            else
            {
                this.controller = new PathFindController((Character)this, this.currentLocation, new PathFindController.isAtEnd(this.foundCropEndFunction), -1, false, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100, Point.Zero);
                if (this.controller.pathToEndPoint == null || Math.Abs(this.controller.pathToEndPoint.Last<Point>().X - (this.homeH.tileX + 1)) > 8 || Math.Abs(this.controller.pathToEndPoint.Last<Point>().Y - (this.homeH.tileY + 1)) > 8)
                {
                    if (Game1.random.NextDouble() < 0.5 && !this.homeH.lastKnownCropLocation.Equals(Point.Zero))
                        this.controller = new PathFindController((Character)this, this.currentLocation, this.homeH.lastKnownCropLocation, -1, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100);
                    else if (Game1.random.NextDouble() < 0.25)
                        this.returnToJunimoHut(this.currentLocation);
                    else
                        this.pathfindToRandomSpotAroundHut();
                }
                else
                    this.sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
            }
        }

        public new void pathfindToNewCrop()
        {
            if (this.backgroundTaskH != null && !this.backgroundTaskH.IsCompleted)
                return;
            this.backgroundTaskH = new Task(new Action(this.pathFindToNewCrop_doWork));
            this.backgroundTaskH.Start();
        }

        public new void returnToJunimoHut(GameLocation location)
        {
            if (Utility.isOnScreen(Utility.Vector2ToPoint(this.position / (float)Game1.tileSize), Game1.tileSize, this.currentLocation))
                this.jump();
            this.collidesWithOtherCharacters = false;
            this.controller = new PathFindController((Character)this, location, new Point(this.homeH.tileX + 1, this.homeH.tileY + 1), 0, new PathFindController.endBehavior(this.junimoReachedHut));
            if (this.controller.pathToEndPoint == null || this.controller.pathToEndPoint.Count<Point>() == 0 || location.isCollidingPosition(this.nextPositionH, Game1.viewport, false, 0, false, (Character)this))
                this.destroyH = true;
            if (!Utility.isOnScreen(Utility.Vector2ToPoint(this.position / (float)Game1.tileSize), Game1.tileSize, this.currentLocation))
                return;
            Game1.playSound("junimoMeep1");
        }

        public override void faceDirection(int direction)
        {
        }

        public override void update(GameTime time, GameLocation location)
        {
            if (this.backgroundTaskH != null && !this.backgroundTaskH.IsCompleted)
            {
                this.sprite.Animate(time, 8, 4, 100f);
            }
            else
            {
                //((this as JunimoHarvester) as NPC).update(time, location);
                base.update(time, location);
                this.forceUpdateTimer = 99999;
                if (this.eventActor)
                    return;
                if (this.destroyH)
                    this.alphaChangeH = -0.05f;
                this.alphaH = this.alphaH + this.alphaChangeH;
                if ((double)this.alphaH > 1.0)
                {
                    this.alphaH = 1f;
                    this.hideShadow = false;
                }
                else if ((double)this.alphaH < 0.0)
                {
                    this.alphaH = 0.0f;
                    this.isInvisible = true;
                    this.hideShadow = true;
                    if (this.destroyH)
                    {
                        location.characters.Remove((NPC)this);
                        this.homeH.myJunimos.Remove(this);
                    }
                }
                if (this.harvestTimerH > 0)
                {
                    int harvestTimer = this.harvestTimerH;
                    this.harvestTimerH = this.harvestTimerH - time.ElapsedGameTime.Milliseconds;
                    if (this.harvestTimerH > 1800)
                        this.sprite.CurrentFrame = 0;
                    else if (this.harvestTimerH > 1600)
                        this.sprite.CurrentFrame = 1;
                    else if (this.harvestTimerH > 1000)
                    {
                        this.sprite.CurrentFrame = 2;
                        this.shake(50);
                    }
                    else if (harvestTimer >= 1000 && this.harvestTimerH < 1000)
                    {
                        this.sprite.CurrentFrame = 0;
                        if (this.currentLocation != null && !this.homeH.noHarvest && (this.currentLocation.terrainFeatures.ContainsKey(this.getTileLocation()) && this.currentLocation.terrainFeatures[this.getTileLocation()] is HoeDirt) && (this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).readyForHarvest())
                        {
                            this.sprite.CurrentFrame = 44;
                            this.lastItemHarvestedH = (Item)null;
                            if ((this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).crop.harvest(this.getTileX(), this.getTileY(), this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt, this))
                                (this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).destroyCrop(this.getTileLocation(), Game1.currentLocation.Equals((object)this.currentLocation));
                            if (this.lastItemHarvestedH != null && this.currentLocation.Equals((object)Game1.currentLocation))
                            {
                                this.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.lastItemHarvestedH.parentSheetIndex, 16, 16), 1000f, 1, 0, this.position + new Vector2(0.0f, (float)(-Game1.tileSize + 6 * Game1.pixelZoom)), false, false, (float)((double)this.getStandingY() / 10000.0 + 0.00999999977648258), 0.02f, Color.White, (float)Game1.pixelZoom, -0.01f, 0.0f, 0.0f, false)
                                {
                                    motion = new Vector2(0.08f, -0.25f)
                                });
                                if (this.lastItemHarvestedH is ColoredObject)
                                    this.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.lastItemHarvestedH.parentSheetIndex + 1, 16, 16), 1000f, 1, 0, this.position + new Vector2(0.0f, (float)(-Game1.tileSize + 6 * Game1.pixelZoom)), false, false, (float)((double)this.getStandingY() / 10000.0 + 0.0149999996647239), 0.02f, (this.lastItemHarvestedH as ColoredObject).color, (float)Game1.pixelZoom, -0.01f, 0.0f, 0.0f, false)
                                    {
                                        motion = new Vector2(0.08f, -0.25f)
                                    });
                            }
                        }
                    }
                    else if (this.harvestTimerH <= 0)
                        this.pokeToHarvest();
                }
                else if (!this.isInvisible && this.controller == null)
                {
                    if (this.addedSpeed > 0 || this.speed > 2 || this.isCharging)
                        this.destroyH = true;
                    this.nextPositionH = this.GetBoundingBox();
                    this.nextPositionH.X += (int)this.motionH.X;
                    bool flag = false;
                    if (!location.isCollidingPosition(this.nextPositionH, Game1.viewport, (Character)this))
                    {
                        this.position.X += (float)(int)this.motionH.X;
                        flag = true;
                    }
                    this.nextPositionH.X -= (int)this.motionH.X;
                    this.nextPositionH.Y += (int)this.motionH.Y;
                    if (!location.isCollidingPosition(this.nextPositionH, Game1.viewport, (Character)this))
                    {
                        this.position.Y += (float)(int)this.motionH.Y;
                        flag = true;
                    }
                    if (!this.motionH.Equals(Vector2.Zero) & flag && Game1.random.NextDouble() < 0.005)
                        location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.random.NextDouble() < 0.5 ? 10 : 11, this.position, this.colorH, 8, false, 100f, 0, -1, -1f, -1, 0)
                        {
                            motion = this.motionH / 4f,
                            alphaFade = 0.01f,
                            layerDepth = 0.8f,
                            scale = 0.75f,
                            alpha = 0.75f
                        });
                    if (Game1.random.NextDouble() < 0.002)
                    {
                        switch (Game1.random.Next(6))
                        {
                            case 0:
                                this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(12, 200),
                  new FarmerSprite.AnimationFrame(13, 200),
                  new FarmerSprite.AnimationFrame(14, 200),
                  new FarmerSprite.AnimationFrame(15, 200)
                });
                                break;
                            case 1:
                                this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(44, 200),
                  new FarmerSprite.AnimationFrame(45, 200),
                  new FarmerSprite.AnimationFrame(46, 200),
                  new FarmerSprite.AnimationFrame(47, 200)
                });
                                break;
                            case 2:
                                this.sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
                                break;
                            case 3:
                                this.jumpWithoutSound(8f);
                                this.yJumpVelocity = this.yJumpVelocity / 2f;
                                this.sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
                                break;
                            case 4:
                                if (!this.homeH.noHarvest)
                                {
                                    this.pathfindToNewCrop();
                                    break;
                                }
                                break;
                            case 5:
                                this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(28, 100),
                  new FarmerSprite.AnimationFrame(29, 100),
                  new FarmerSprite.AnimationFrame(30, 100),
                  new FarmerSprite.AnimationFrame(31, 100)
                });
                                break;
                        }
                    }
                }
                if (this.controller != null || !this.motionH.Equals(Vector2.Zero))
                {
                    this.sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
                    if (this.moveRight || (double)Math.Abs(this.motionH.X) > (double)Math.Abs(this.motionH.Y) && (double)this.motionH.X > 0.0)
                    {
                        this.flip = false;
                        if (!this.sprite.Animate(time, 16, 8, 50f))
                            return;
                        this.sprite.CurrentFrame = 16;
                    }
                    else if (this.moveLeft || (double)Math.Abs(this.motionH.X) > (double)Math.Abs(this.motionH.Y) && (double)this.motionH.X < 0.0)
                    {
                        if (this.sprite.Animate(time, 16, 8, 50f))
                            this.sprite.CurrentFrame = 16;
                        this.flip = true;
                    }
                    else if (this.moveUp || (double)Math.Abs(this.motionH.Y) > (double)Math.Abs(this.motionH.X) && (double)this.motionH.Y < 0.0)
                    {
                        if (!this.sprite.Animate(time, 32, 8, 50f))
                            return;
                        this.sprite.CurrentFrame = 32;
                    }
                    else
                    {
                        if (!this.moveDown)
                            return;
                        this.sprite.Animate(time, 0, 8, 50f);
                    }
                }
                else
                {
                    if (this.sprite.CurrentAnimation != null || this.harvestTimerH > 0)
                        return;
                    this.sprite.Animate(time, 8, 4, 100f);
                }
            }
        }

        public new void pathfindToRandomSpotAroundHut()
        {
            this.controller = new PathFindController((Character)this, this.currentLocation, Utility.Vector2ToPoint(new Vector2((float)(this.homeH.tileX + 1 + Game1.random.Next(-8, 9)), (float)(this.homeH.tileY + 1 + Game1.random.Next(-8, 9)))), -1, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100);
        }

        public new void tryToAddItemToHut(Item i)
        {
            this.lastItemHarvestedH = i;
            Item obj = this.homeH.output.addItem(i);
            if (obj == null || !(i is StardewValley.Object))
                return;
            for (int index = 0; index < obj.Stack; ++index)
                Game1.createObjectDebris(i.parentSheetIndex, this.getTileX(), this.getTileY(), -1, (i as StardewValley.Object).quality, 1f, this.currentLocation);
        }

        public override void draw(SpriteBatch b, float alpha = 1f)
        {
            if (this.isInvisible)
                return;
            b.Draw(this.Sprite.Texture, this.getLocalPosition(Game1.viewport) + new Vector2((float)(this.sprite.spriteWidth * Game1.pixelZoom / 2), (float)((double)this.sprite.spriteHeight * 3.0 / 4.0 * (double)Game1.pixelZoom / Math.Pow((double)(this.sprite.spriteHeight / 16), 2.0)) + (float)this.yJumpOffset - (float)(Game1.pixelZoom * 2)) + (this.shakeTimer > 0 ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(this.Sprite.SourceRect), this.colorH * this.alphaH, this.rotation, new Vector2((float)(this.sprite.spriteWidth * Game1.pixelZoom / 2), (float)((double)(this.sprite.spriteHeight * Game1.pixelZoom) * 3.0 / 4.0)) / (float)Game1.pixelZoom, Math.Max(0.2f, this.scale) * (float)Game1.pixelZoom, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0.0f, this.drawOnTop ? 0.991f : (float)(((double)(this.getStandingY() + this.whichJunimoFromThisHut) + (double)this.getStandingX() / 10000.0) / 10000.0)));
            if (this.swimming || this.hideShadow)
                return;
            b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2((float)(this.sprite.spriteWidth * Game1.pixelZoom) / 2f, (float)(Game1.tileSize * 3) / 4f - (float)Game1.pixelZoom)), new Rectangle?(Game1.shadowTexture.Bounds), this.colorH * this.alphaH, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), ((float)Game1.pixelZoom + (float)this.yJumpOffset / 40f) * this.scale, SpriteEffects.None, Math.Max(0.0f, (float)this.getStandingY() / 10000f) - 1E-06f);
        }
    }
}
