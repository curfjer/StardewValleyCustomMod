using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValleyCustomMod.CustomBuildings;

namespace StardewValleyCustomMod
{
    class Harvester : JunimoHarvester
    {
        private float alpha = 1f;
        private Vector2 motion = Vector2.Zero;
        private float alphaChange;
        private Rectangle nextPosition;
        private Color color;
        private HarvesterBuilding home;
        private bool destroy;
        //private Item lastItemHarvested;
        //private Task backgroundTask;
        //private int harvestTimer;


        public Harvester(Vector2 position, HarvesterBuilding myHome, int whichJunimoNumberFromThisHut)
        {
            this.home = myHome;
            this.whichJunimoFromThisHut = whichJunimoNumberFromThisHut;
            Random random = new Random(myHome.tileX + myHome.tileY * 777 + whichJunimoNumberFromThisHut);
            this.nextPosition = this.GetBoundingBox();
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
            this.alpha = 0.0f;
            this.hideShadow = true;
            this.alphaChange = 0.05f;
            if (random.NextDouble() < 0.25)
            {
                switch (random.Next(8))
                {
                    case 0:
                        this.color = Color.Red;
                        break;
                    case 1:
                        this.color = Color.Goldenrod;
                        break;
                    case 2:
                        this.color = Color.Yellow;
                        break;
                    case 3:
                        this.color = Color.Lime;
                        break;
                    case 4:
                        this.color = new Color(0, (int)byte.MaxValue, 180);
                        break;
                    case 5:
                        this.color = new Color(0, 100, (int)byte.MaxValue);
                        break;
                    case 6:
                        this.color = Color.MediumPurple;
                        break;
                    case 7:
                        this.color = Color.Salmon;
                        break;
                }
                if (random.NextDouble() < 0.01)
                    this.color = Color.White;
            }
            else
            {
                switch (random.Next(8))
                {
                    case 0:
                        this.color = Color.LimeGreen;
                        break;
                    case 1:
                        this.color = Color.Orange;
                        break;
                    case 2:
                        this.color = Color.LightGreen;
                        break;
                    case 3:
                        this.color = Color.Tan;
                        break;
                    case 4:
                        this.color = Color.GreenYellow;
                        break;
                    case 5:
                        this.color = Color.LawnGreen;
                        break;
                    case 6:
                        this.color = Color.PaleGreen;
                        break;
                    case 7:
                        this.color = Color.Turquoise;
                        break;
                }
            }
            this.willDestroyObjectsUnderfoot = false;
            this.currentLocation = (GameLocation)Game1.getFarm();
            Vector2 v = Vector2.Zero;
            switch (whichJunimoNumberFromThisHut)
            {
                case 0:
                    v = Utility.recursiveFindOpenTileForCharacter((Character)this, this.currentLocation, new Vector2((float)(this.home.tileX + 1), (float)(this.home.tileY + this.home.tilesHigh + 1)), 30);
                    break;
                case 1:
                    v = Utility.recursiveFindOpenTileForCharacter((Character)this, this.currentLocation, new Vector2((float)(this.home.tileX - 1), (float)this.home.tileY), 30);
                    break;
                case 2:
                    v = Utility.recursiveFindOpenTileForCharacter((Character)this, this.currentLocation, new Vector2((float)(this.home.tileX + this.home.tilesWide), (float)this.home.tileY), 30);
                    break;
            }
            if (v != Vector2.Zero)
                this.controller = new PathFindController((Character)this, this.currentLocation, Utility.Vector2ToPoint(v), -1, new PathFindController.endBehavior(this.reachFirstDestinationFromHut), 100);
            if (this.controller == null || this.controller.pathToEndPoint == null)
            {
                this.pathfindToRandomSpotAroundHut();
                if (this.controller == null || this.controller.pathToEndPoint == null)
                    this.destroy = true;
            }
            this.collidesWithOtherCharacters = false;
        }
    }
}
