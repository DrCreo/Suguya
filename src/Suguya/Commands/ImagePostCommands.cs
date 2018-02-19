using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Suguya.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suguya.Commands
{
    [Group("image_poster"), Aliases("ip"), Description("Commands for configuring the image posting."), RequireUserPermissions(Permissions.ManageGuild)]
    public sealed class ImagePostCommands : BaseCommandModule
    {
        private ImagePoster _imagePoster { get; }
        private DiscordClient _client { get; }
        private InteractivityExtension _interactivity { get; }

        public ImagePostCommands(ImagePoster imagePoster, DiscordClient client)
        {
            _imagePoster = imagePoster;
            _client = client;
            _interactivity = _client.GetExtension<InteractivityExtension>();
        }

        [Command("registerchannel"), Aliases("addchannel"), Description("Registers a provided channel which the bot will automatically post images to."), RequireUserPermissions(Permissions.ManageGuild)]
        public async Task RegisterChannel(CommandContext ctx, [Description("ID of the channel.")] ulong ChannelID,
            [Description("Lets you choose to use the default tag filter list which filters some extreme and distasteful content, can be customized later")] bool UseDefaultTagFilter,
            [Description("Toggle posting.")] bool TogglePosting)
        {

            if (!ctx.Guild.Channels.Any(c => c.Id == ChannelID))
            {
                await ctx.Channel.SendMessageAsync("That channel does not belong to this guild.");
                return;
            }

            List<string> tagfilter = new List<string>();
            if (UseDefaultTagFilter)
                tagfilter = ConstantVars.FILTERS.Split(" ").ToList();
            var tmpChannel = new PostChannel(ChannelID, tagfilter, new List<string>(), TogglePosting);
            _imagePoster.Channels.Add(tmpChannel);
            await _imagePoster.SaveChannelData();

            await ctx.Channel.SendMessageAsync("Channel added you can now configure its settings.");
        }

        [Command("edit-tagfilter"), Description("edit the tags to filter"), RequireUserPermissions(Permissions.ManageGuild)]
        public async Task EditChannelTags(CommandContext ctx, [Description("ID of the channel to edit.")]ulong channelid, [RemainingText, Description("Tags to filter. spaces are used to seperate tags so use _ if a tag has multiple words with spaces in it.")] string tagfilter)
        {
            var taglist = ProcessTags(tagfilter);

            if (taglist == null)
            {
                await ctx.RespondAsync("No tags were provided.");
                return;
            }


            if (!ctx.Guild.Channels.Any(c => c.Id == channelid))
            {
                await ctx.Channel.SendMessageAsync("That channel does not belong to this guild.");
                return;
            }

            var chan = _imagePoster.GetChannelByID(channelid);

            chan.SetTagFilter(taglist);
            await _imagePoster.SaveChannelData();
            await ctx.RespondAsync("Tag filter updated.");
        }

        [Command("edit-rating"), Description("edit rating of posts"), RequireUserPermissions(Permissions.ManageGuild)]
        public async Task EditRating(CommandContext ctx, [Description("ID of the channel to edit.")]ulong channelid, [Description("Rating of posts. `0` = safe, `1` = questionable, `2` = explicit, `3` = any.")] int rating = 0)
        {

            var _rating = Rating.NA;
            switch (rating)
            {
                case 1:
                    _rating = Rating.Q;
                    break;
                case 2:
                    _rating = Rating.E;
                    break;
                case 3:
                    _rating = Rating.A;
                    break;
                default:
                    _rating = Rating.S;
                    break;
            }

            if (!ctx.Guild.Channels.Any(c => c.Id == channelid))
            {
                await ctx.Channel.SendMessageAsync("That channel does not belong to this guild.");
                return;
            }

            var chan = _imagePoster.GetChannelByID(channelid);

            chan.SetRating(_rating);
            await _imagePoster.SaveChannelData();
            await ctx.RespondAsync("Rating set.");
        }


        private List<string> ProcessSources(string content)
        {
            if (content.Length == 0)
                return null;
            if (content.ToLower().Contains("non"))
                return new List<string>();
            return content.ToLower().Split(" ").ToList();
        }

        private List<string> ProcessTags(string content)
        {
            if (content == null)
                return null;

            if (content.ToLower().StartsWith("default"))
                return ConstantVars.FILTERS.Split(" ").ToList();

            return content.ToLower().Split(" ").ToList();
        }
    }
}
