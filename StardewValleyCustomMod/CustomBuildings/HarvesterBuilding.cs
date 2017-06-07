using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValleyCustomMod.CustomBlueprints;

namespace StardewValleyCustomMod.CustomBuildings
{
    class HarvesterBuilding : CustomBuilding
    {
        public int HarvesterRaduis;
        private Texture2D MinionTexture;

        public HarvesterBuilding(CustomBuildingBlueprint blu, Vector2 coord) : base(blu, coord)
        {
            this.HarvesterRaduis = blu.HarvesterRadius;
        }
    }
}
