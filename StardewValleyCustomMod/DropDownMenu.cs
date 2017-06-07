using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace StardewValleyCustomMod.Menus
{
    class DropDownMenu
    {
        private Texture2D BoxTexture;
        private bool MenuOpen;
        private int MaxStringLength;
        private String[] Options;
        private String Title;
        private String OptionSelected;
        private Vector2 Location;
        private Vector2 OptionsLocation;
        private float Scalar;

        // Should this be non static and in constructor for user to specify? TODO
        private static int FontHeight = 11;
        private static int BorderBuffer = 4;

        public DropDownMenu(String title, String[] options, Vector2 location)
        {
            this.Title = title;
            this.Options = options;
            this.Location = location;
            this.MenuOpen = false;
            this.OptionSelected = "Select an Option...";

            this.MaxStringLength = (int)Game1.dialogueFont.MeasureString("Select an Option").X;
            int stringLength = 0;
            for (int i = 0; i < options.Length; i++)
            {
                stringLength = (int)Game1.dialogueFont.MeasureString(options[i]).X;
                if (this.MaxStringLength < stringLength)
                    this.MaxStringLength = stringLength;
            }
        }

        public String OptionClicked(int x, int y)
        {
            // Return the option that was selected
            if (x >= this.Location.X && x <= this.Location.X + this.MaxStringLength + DropDownMenu.BorderBuffer)
            {
                if (this.MenuOpen && y >= this.OptionsLocation.Y && y <= this.OptionsLocation.Y + (int)(DropDownMenu.FontHeight * this.Options.Length * this.Scalar + BorderBuffer * 2 * this.Scalar))
                {
                    // integer division rounds down right TODO
                    int optionY = (int)((y - (int)this.OptionsLocation.Y) / (DropDownMenu.FontHeight * this.Scalar));
                    this.MenuOpen = false;
                    return this.OptionSelected = this.Options[optionY];
                }
                // Clicking menu when closed opens it
                else if (y >= this.Location.Y && y <= this.Location.Y + DropDownMenu.FontHeight * this.Scalar + BorderBuffer * 2 * this.Scalar)
                    this.MenuOpen = true;  
            } 
            // Clicking outside menu closes it
            else
                this.MenuOpen = false;

            return null;
        }

        public bool HoverCheck(int index)
        {
            Vector2 mouseCoord = new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY());
            if (mouseCoord.X >= this.OptionsLocation.X && mouseCoord.X <= this.OptionsLocation.X + this.MaxStringLength + DropDownMenu.BorderBuffer 
                && mouseCoord.Y >= this.OptionsLocation.Y + DropDownMenu.FontHeight * index * Scalar 
                && mouseCoord.Y <= this.OptionsLocation.Y + DropDownMenu.FontHeight * (index + 1)* this.Scalar)
                return true;
            return false;
        }

        public void Draw(SpriteBatch b, float scalar)
        {
            // Draw Title Box
            //b.Draw(StardewValleyCustomMod.CustomTiles, this.Location, new Microsoft.Xna.Framework.Rectangle(0, 144, 32, 64), Color.White, 0.0f, new Vector2(0.0f, 5.0f), scalar, SpriteEffects.None, (float)0.0f);
            IClickableMenu.drawTextureBox(b, (int)this.Location.X, (int)this.Location.Y, this.MaxStringLength + DropDownMenu.BorderBuffer, (int)(DropDownMenu.FontHeight * scalar + DropDownMenu.BorderBuffer * 2 * scalar), Color.White);
            // Draw Title
            Utility.drawTextWithShadow(b, this.Title + this.OptionSelected, Game1.dialogueFont, this.Location + new Vector2(DropDownMenu.BorderBuffer, DropDownMenu.BorderBuffer) * scalar, Game1.textColor, 1f, -1f, -1, -1, 0.25f, 3);

            // Find a better spot for this? TODO
            this.OptionsLocation = this.Location + new Vector2(0, (int)(FontHeight + DropDownMenu.BorderBuffer * 2)) * scalar;
            this.Scalar = scalar;

            // Draw Dropdown Menu
            if (this.MenuOpen)
            {
                // Draw Option Box
                //IClickableMenu.drawTextureBox(b, (int)this.OptionsLocation.X, (int)(this.OptionsLocation.Y), this.MaxStringLength + DropDownMenu.BorderBuffer, (int)(DropDownMenu.FontHeight * this.Options.Length * scalar + BorderBuffer * 2 * scalar), Color.White);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), (int)this.OptionsLocation.X, (int)this.OptionsLocation.Y, this.MaxStringLength + DropDownMenu.BorderBuffer, (int)(DropDownMenu.FontHeight * this.Options.Length * scalar), Color.White, 1f, false);
                //b.Draw(StardewValleyCustomMod.CustomTiles, this.Location + new Vector2(0, 32), new Microsoft.Xna.Framework.Rectangle(0, 144, 32, 64), Color.White, 0.0f, new Vector2(0.0f, 5.0f), scalar, SpriteEffects.None, (float)0.0f);

                // Draw Options
                for (int i = 0; i < this.Options.Length; i++)
                {
                    // Draw highlighted box behind option
                    if (this.HoverCheck(i))
                    {
                        StardewValleyCustomMod.Logger.Log("$");
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), (int)this.OptionsLocation.X, (int)(this.OptionsLocation.Y + ( i * FontHeight) * scalar), this.MaxStringLength + DropDownMenu.BorderBuffer, (int)(DropDownMenu.FontHeight * scalar), Color.Wheat, 1f, false);
                    }
                        
                    //IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), 
                    // this.gamesToLoadButton[index].bounds.X, this.gamesToLoadButton[index].bounds.Y, 
                    //this.gamesToLoadButton[index].bounds.Width, this.gamesToLoadButton[index].bounds.Height, 
                    //this.currentItemIndex + index == this.selected && this.timerToLoad % 150 > 75 && 
                    //this.timerToLoad > 1000 || this.selected == -1 && 
                    //this.gamesToLoadButton[index].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) &&
                    //(!this.scrolling && !this.deleteConfirmationScreen) ? (this.deleteButtons[index].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())
                    //? Color.White : Color.Wheat) : Color.White, (float) Game1.pixelZoom, false);

                    // Draw option
                    Utility.drawTextWithShadow(b, this.Options[i], Game1.dialogueFont, this.OptionsLocation + new Vector2(DropDownMenu.BorderBuffer, i * FontHeight) * scalar, Game1.textColor, 1f, -1f, -1, -1, 0.25f, 3);
                }
            }
        }

        public Vector2 GetLocation()
        {
            return this.Location;
        }

        public void SetLocation(Vector2 location)
        {
            this.Location = location;
        }
    }
}
