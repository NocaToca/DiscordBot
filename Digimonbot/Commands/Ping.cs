using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/*
    Test command. Keeping it bc it could actually be useful to tell if the bot is on
*/
public class Ping : BaseCommandModule{

    [Command("ping")]
    public async Task PingCommand(CommandContext ctx){
        await ctx.Channel.SendMessageAsync("Pong!").ConfigureAwait(false);   
    }

}