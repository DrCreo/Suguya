using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Suguya.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suguya.Commands
{
    public class DNDCommands : BaseCommandModule
    {
        Random rng = new Random();

        [Command("roll"), Aliases("r", "r-d","roll-d"), Description("Rolls a die.")]
        public async Task RollAsync(CommandContext ctx, [Description("Number of sides the die has.")] int dienumber)
        {
            await Task.Delay(1000);
            await ctx.RespondAsync($"You cast your {dienumber} sided die...");
            await Task.Delay(2000);
            await ctx.RespondAsync($"You roll a {rng.Next(1, dienumber + 1)}!");
        }
    }
}
