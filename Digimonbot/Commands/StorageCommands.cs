using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/*
    Probably one of the more complex classes, but handles all interactions with storage

    Currently the file format is just a bunch of text files, but future format will include directories.
        DiscordBot
            -NocaToca
                -Vixmon.txt (will be able to contain multiple Vixmon)
*/
public class StorageCommands : BaseCommandModule{

    #pragma warning disable 8618
    //The file manager
    public static FileManager fileManager;
    #pragma warning restore 8618

    //Registers the user and creates their specific file
    [Command("register")]
    public async Task RegisterUser(CommandContext ctx){
        string filename = ctx.Member.Username + ".txt";
        if(File.Exists(filename)){
            await ctx.RespondAsync("User already registered!");
            return;
        }

        File.Create(filename);
        await ctx.RespondAsync(ctx.Member.Username + " registered!");
    }

    /*
        Stores the specified creature within the data base.

        Currently can store any creature as long as they know the id and there is no storage limit.
        Command will eventually be obsolete once a capturing mechanic is put in place
    */
    [Command("store")]
    public async Task StoreCreature(CommandContext ctx, int id){

        string filename = ctx.Member.Username + ".txt";
        if(!File.Exists(filename)){
            await ctx.RespondAsync("User has not registered! Use !register to register!");
            return;
        }

        //Creates a brand new creature to store
        Creature c = Creature.GetCreature(id).Copy();
        if(fileManager == null){
            fileManager = new FileManager();
        }

        //Name is misleading, but if the creature is found then we can't add it twice. Will be able to later, but not atm as I don't want to deal with it
        try{
            await fileManager.WriteCreatureIntoFile(filename, c);
            await ctx.RespondAsync("Creature stored!");

        #pragma warning disable 168
        } catch (CreatureNotFoundException e){
            await ctx.RespondAsync("Creature already stored! Cannot store multiple!");

        }
        #pragma warning restore 168

    }

    /*
        Reads the user's storage to see if they have the specified creatue

        Obviously errors if they do not
    */
    [Command("read")]
    public async Task ReadCreature(CommandContext ctx, int id){
        
        string filename = ctx.Member.Username + ".txt";
        if(!File.Exists(filename)){
            await ctx.RespondAsync("User has not registered! Use !register to register!");
            return;
        }

        if(fileManager == null){
            fileManager = new FileManager();
        }

        try{
            await ctx.RespondAsync(fileManager.ReadCreature(filename, id));

        } catch(Exception e){
            if((e is CreatureNotFoundException )){
                await ctx.RespondAsync("You do not own that creature!");

            } else {
                await ctx.RespondAsync("That creature does not exist!");

            }

        }

    }


}

/*
    File manager class. Manages the file
*/
public class FileManager{

    //This class can only be created internally
    internal FileManager(){

    }

    //Writes the specified creature into the file
    public async Task WriteCreatureIntoFile(string filename, Creature c){

        //If the creature already exists, we just throw this exception. I will fix the name later
        if(DoesCreatureAlreadyExistInFile(filename, c)){
            throw new CreatureNotFoundException();
        }

        //using streamwriter we just append the creature and tack it on
        using StreamWriter file = new(filename, append: true);
        await file.WriteLineAsync(c.GetStorageInfo());
    }

    /*
        Finds out whether or not the creature is already within the file
    */
    internal bool DoesCreatureAlreadyExistInFile(string filename, Creature c){
        string[] lines = System.IO.File.ReadAllLines(filename);

        return DoesCreatureAlreadyExistInFile(lines, c);
    }
    internal bool DoesCreatureAlreadyExistInFile(string[] lines, Creature c){
        
        //iterates through to see if the string ids match
        foreach(string line in lines){
            if(line == c.GetID()){
                return true;
            }
        }

        return false;
    }

    /*
        Retrieves the creature from storage. Realistically it creatures a new creature class, but it's the same thing
    */
    public Creature RetrieveCreature(string filename, int id){
        Creature c = Creature.GetCreature(id);
        string[] lines = System.IO.File.ReadAllLines(filename);

        //If our creature doesn't exist, throw an exception
        if(!DoesCreatureAlreadyExistInFile(lines, c)){
            throw new CreatureNotFoundException();
        }

        //We iterate through the list till we find the id
        List<string> s = new List<string>();
        for(int i = 0; i < lines.Length; i++){

            //Once we found the id, we really only need the necessary information which is currently uniform for all creatures
            if(lines[i] == c.GetID()){
                for(int j = 0; j < 7; j++){
                    s.Add(lines[i+j]);
                }
                c = CreateCreature(s);

                break;
            }
        }

        return c;
    }

    //Reads out the creature's embed. 
    public DiscordEmbedBuilder ReadCreature(string filename, int id){
        Creature c = RetrieveCreature(filename, id);

        return c.GetInfoEmbed();
    }

    //Parses the string correctly in order to create the right creature
    internal Creature CreateCreature(List<string> s){
        int id = int.Parse(s[0]);
        Creature c = Creature.GetCreature(id).Copy();

        s[2] = s[2].Substring(6); //Level
        s[3] = s[3].Substring(7); //Attack
        s[4] = s[4].Substring(8); //Defense
        s[5] = s[5].Substring(3); //HP
        s[6] = s[6].Substring(11); //Experience
        Stats stats = new Stats(int.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]), float.Parse(s[5]), float.Parse(s[6]));

        c.stats = stats;

        return c;

    }

    /*
        Saves the creature into our storage

        This is for when a creature's information gets updated (most likely experience)
    */
    public async Task SaveCreature(DiscordMember member, Creature c){
        string filename = member.Username + ".txt";

        string[] lines = System.IO.File.ReadAllLines(filename);

        List<string> newLines = new List<string>();
        for(int i = 0; i < lines.Length; i++){
            if(lines[i] != c.GetID()){
                newLines.Add(lines[i]);
            } else {
                newLines.Add(c.GetStorageInfo());
                i += 7;
            }
        }

        string finalString = "";
        foreach(string line in newLines){
            finalString += line + "\n";
        }

        await File.WriteAllTextAsync(filename, finalString);

    }

}

public class CreatureNotFoundException : Exception{
    public CreatureNotFoundException(){

    }

}
