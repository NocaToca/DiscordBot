using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/*
    The good ol' bot!

    Most of this code is ripped from the DSharp example bots, but it basically just turns on the bot

*/
public class Bot{
    #pragma warning disable 8618
    //The client of the bot
    public DiscordClient client {get; private set;}

    //The commands extension so the bot can use commands
    public CommandsNextExtension commands {get; private set;}
    #pragma warning restore 8618

    //Runs the bot on a thread
    public async Task RunBotAsync(){

        //JSON var
        var json = string.Empty;

        //Reads the config file, so we can easily configure our bot settings
        using(var fs = File.OpenRead("config.json"))
            using(var sr = new StreamReader(fs, new UTF8Encoding(false)))
            json = await sr.ReadToEndAsync().ConfigureAwait(false);

        //The actual read configuration, taken as a config reader
        var configJson = JsonConvert.DeserializeObject<ConfigReader>(json);

        //The discord bot configuration, configures our discord bot's settings
        DiscordConfiguration config = new DiscordConfiguration(){
            Token = configJson.token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
        };

        client = new DiscordClient(config);

        //Adding an OnReady event listener
        client.Ready += OnReady;

        //Commands settings - can use commands in dms with bot 
        CommandsNextConfiguration commandsConfig =  new CommandsNextConfiguration{

            StringPrefixes = new string[] {configJson.prefix},
            EnableMentionPrefix = true,
            EnableDms = true
        };

        //Registers our commands
        commands = client.UseCommandsNext(commandsConfig);
        commands.RegisterCommands<Ping>();
        commands.RegisterCommands<CreatureCommands>();
        commands.RegisterCommands<StorageCommands>();


        await client.ConnectAsync();

        //Makes it so the discord bot never ends (unless force exited)
        await Task.Delay(-1);
    }

    private Task OnReady(DiscordClient sender, ReadyEventArgs e){

        Console.WriteLine("Bot Ready!");

        return Task.CompletedTask;

    }

}