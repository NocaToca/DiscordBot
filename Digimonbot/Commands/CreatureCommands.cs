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
    Main Creature commands. Main job is to parse inputs. Battles are done through the Battle class
*/
public class CreatureCommands : BaseCommandModule{

    #pragma warning disable 8618
    public Battle currentBattle;
    #pragma warning restore 8618

    //Returns the name of the creature
    [Command("name")]
    public async Task FindName(CommandContext ctx, [Description("Creature name")] params int[] args){

        /*
            Most of this was to just test my understanding of how this was different than other things I've done.
            Really just saying "please don't have multiple commands"

            Pretty good bc it helps the user know just what they can do
        */
        if(args.Length > 1){
            await ctx.RespondAsync("Too many arguments; can only read one ID at a time!");
            return;
        } else
        if(args.Length == 0){
            await ctx.RespondAsync("Too little arguments. Need an ID to read!");
            return;
        }

        if(args.Length == 1){
            Creature c = Creature.GetCreature(args[0]);

            if(c == null){
                await ctx.RespondAsync("ID: " + args[0] + " not found!");
                return;
            }

            await ctx.RespondAsync(c.GetName());
        } else {
            await ctx.RespondAsync("Unknown ID!");
            return;
        }
    }

    //Returns the discord embed of the given creature
    [Command("info")]
    public async Task GetInfo(CommandContext ctx, int ID){
        Creature c = Creature.GetCreature(ID);

        if(c == null){
            await ctx.RespondAsync("ID: " + ID + " not found!");
            return;
        }

        await ctx.RespondAsync(c.GetInfoEmbed());
    }

    /*
        Gets the file name and throws a user not registered exception if the file does not exist
    */
    private string GetFileName(DiscordMember member){
        string filename = member.Username + ".txt";
        if(!File.Exists(filename)){
            throw new UserNotRegisteredException();
        }

        return filename;
    }

    /*
        Starts the fight only if there is no current battle going on. Otherwise we return a proper error message
    */
    [Command("fight")]
    public async Task Fight(CommandContext ctx, int id){
        if(currentBattle != null){
            if(currentBattle.member == ctx.Member){
                await ctx.RespondAsync("You are already in battle! Finish your current one before starting a new one!");
                return;
            }
            await ctx.RespondAsync("There is currently another battle going on! Wait for that one to finish before starting your own!");
            return;
        }

        string filename;
        try{
            filename = GetFileName(ctx.Member);
        #pragma warning disable 168
        } catch(UserNotRegisteredException e){
            await ctx.RespondAsync("User has not registered! Use !register to register!");
            return;
        }
        #pragma warning restore 168
        

        //Grabs the user's specified creature from storage, and returns a proper error message if it can't find or retrieve it
        Creature cOne;
        try{
            cOne = StorageCommands.fileManager.RetrieveCreature(filename, id);
        } catch (Exception e){
            if(e is CreatureNotFoundException){
                await ctx.RespondAsync("User does not own that creature!");
            } else {
                await ctx.RespondAsync("Creature id does not exist!");
            }
            return;
        }

        //Atm, just creates a basic enemy and starts the battle
        Creature cTwo = new Slimemon(1);
        currentBattle = new Battle(ctx.Member, cOne, cTwo);
        await ctx.RespondAsync(ctx.Member.Username + " has started a new battle with " + cOne + " verses " + cTwo + "\nUse the d!battle command to see battle commands!");

    }

    /*
        Basic battle help
    */
    [Command("battle")]
    public async Task BattleHelp(CommandContext ctx){
        await ctx.RespondAsync("Use d!attack to attack \nUse d!magic to attack with magic");
    }

    /*
        Processes the messages and the enemy AI's turn, since it's all random for it in the end (at least at the moment)
    */
    private async Task Turn(CommandContext ctx, float dmgDealt){

        string message = "Your " + currentBattle.userCreature.GetName() + " dealt " + dmgDealt + " to the enemy " + currentBattle.enemyCreature.GetName() + "!";

        //We have to check if the player killed the enemy creature
        if(currentBattle.battleEnded){
            message += "\nYour " + currentBattle.userCreature.GetName() + " won! You gained 2 exp!";
            await ctx.RespondAsync(message);
            currentBattle.ProcessEndBattle(0);
            currentBattle = null;
            return;
        }

        float dmgTaken = currentBattle.ProcessEnemy();

        message += "\nThe enemy " + currentBattle.enemyCreature.GetName() + " dealt " + dmgTaken + " to your " + currentBattle.userCreature.GetName() + "!";

        //And here we check if the enemy killed the play
        if(currentBattle.battleEnded){
            message += "\nThe enemy " + currentBattle.enemyCreature.GetName() + " won!";
            await ctx.RespondAsync(message);
            currentBattle = null;
            return;
        }

        //Prints basic battle information; will be changed to edits
        await ctx.RespondAsync(message);
        await ctx.RespondAsync(currentBattle.userCreature.GetInfoEmbed());
        await ctx.RespondAsync(currentBattle.enemyCreature.GetInfoEmbed());
    }

    /*
        Attacks physically. Does basically the same thing as magic but inputs different values into the Battle class
    */
    [Command("attack")]
    public async Task Attack(CommandContext ctx){

        if(ctx.Member != currentBattle.member){
            await ctx.RespondAsync("You are currently not participating in the current battle!");
            return;
        }

        float dmgDealt = currentBattle.ProcessAlly(0);

        await Turn(ctx, dmgDealt);
    }

    /*
        Attacks magically. Does basically the same thing as attack but inputs different values into the Battle class
    */
    [Command("magic")]
    public async Task Magic(CommandContext ctx){

        if(ctx.Member != currentBattle.member){
            await ctx.RespondAsync("You are currently not participating in the current battle!");
            return;
        }

        float dmgDealt = currentBattle.ProcessAlly(1);

        await Turn(ctx, dmgDealt);
    }



}

/*
    The main Battle class. Has the main member and the creatures in battle along with a bool to tell if the battle has ended
*/
public class Battle{
    public DiscordMember member;

    public Creature userCreature;

    public Creature enemyCreature;

    public bool battleEnded;

    public Battle(DiscordMember member, Creature userCreature, Creature enemyCreature){

        this.member = member;
        this.enemyCreature = enemyCreature;
        this.userCreature = userCreature;

        //Battles never end when they just started!
        battleEnded = false;
    }
    
    /*
        Processes the actual turn having the attacking creature attack the defending creature
    */
    private float Process(Creature attackingCreature, Creature defendingCreature){
        System.Random _rand = new System.Random();

        float trueDamage = ((float)_rand.Next()/(float)int.MaxValue) * (2.5f * attackingCreature.stats.attack);

        float actualDmg = (trueDamage - defendingCreature.stats.defense * .5f > 0) ? trueDamage - defendingCreature.stats.defense * .5f : 0;

        if(defendingCreature.stats.hp - actualDmg <= 0){
            battleEnded = true;
            return defendingCreature.stats.hp;
        }

        defendingCreature.TakeDamage(actualDmg);
        return actualDmg;
    }

    //Processes the ally turn's
    public float ProcessAlly(int moveType){
        if(moveType == 0){
            //It's an attack (I mean why would you use defend)

        }
        //Though atm idc same thing for both
        return Process(userCreature, enemyCreature);
    }

    //Process the enemy's turn
    public float ProcessEnemy(){
        //Just flip ally and enemy
        return Process(userCreature, enemyCreature);
    }

    //Ends the battle and saves the creature's gained stats
    public void ProcessEndBattle(int victor){
        if(victor == 1){
            return;
        }

        //Adds the gained xp
        userCreature.stats.ProcessNewExp(2.0f);

        //resets the cerature's hp
        userCreature.stats.hp = userCreature.stats.MAX_HP;

        //Saves the creature
        StorageCommands.fileManager.SaveCreature(member, userCreature);

    }

}

//Exception class
public class UserNotRegisteredException : Exception{
    public UserNotRegisteredException(){

    }

}