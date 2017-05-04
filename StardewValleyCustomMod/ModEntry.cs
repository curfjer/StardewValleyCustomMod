using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewValleyCustomMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
{
    /********* 
    ** Public methods
    *********/
    /// <summary>Initialise the mod.</summary>
    /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
    public override void Entry(IModHelper helper)
    {
        ControlEvents.KeyPressed += this.ReceiveKeyPress;
            
    }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            Game1.activeClickableMenu = (IClickableMenu) new CarpenterMenu(false);
            //Game1.activeClickableMenu = (IClickableMenu)new ShopMenu(new List<Item>(), 0, (string)null);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
    {
        this.Monitor.Log($"Player pressed {e.KeyPressed}.");
            if (e.KeyPressed.ToString().Equals("T"))
            {
                SaveEvents.AfterLoad += SaveEvents_AfterLoad;
                //ControlEvents.add_MouseChanged(new EventHandler<EventArgsMouseStateChanged>();
                this.Monitor.Log("Player tried to access the carpenter menu.");
            }
        }
}
}