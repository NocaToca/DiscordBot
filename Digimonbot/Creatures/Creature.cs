using System;
using DSharpPlus;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using DSharpPlus.Entities;
using System.Collections.Generic;

/*
    The main creature abstract class. Derived classes will be the creatures themselves however creatures should hardly ever be declared


    Main parameters are self explained, but have a better definition above them
*/
public abstract class Creature{

    #pragma warning disable 8618
    //The ID of a creature - atm the main way to retrieve and discern creatures. Goes in the format ### (ID of 1 is 001)
    protected string id;

    //The name of the creature
    protected string name;

    //URL image of the creature; this is from a seperate channel in the main server
    protected string imageURL;

    //The stats within the creature
    public Stats stats;
    #pragma warning restore 8618

    //The list of all creatures. Use this for most creature visual options
    protected static List<Creature> creatures = new List<Creature>();

    //Returns the name of the creature
    public string GetName(){
        return name;
    }

    //Returns the id of the creature
    public string GetID(){
        return id;
    }

    //Creates a discord embed based off of the information of the creature. Should only be called by RespondAsync
    public DiscordEmbedBuilder GetInfoEmbed(){

        string desc = stats.MakeString();
        var embed = new DiscordEmbedBuilder{
            Title = name,
            ImageUrl = imageURL,
            Description = desc
        };

        return embed;
    }

    //Takes the damage inputted; returns true if the creature dies (goes below zero)
    public bool TakeDamage(float dmg){
        stats.hp = stats.hp - dmg;
        return stats.hp < 0;
    }

    //Abstract function to override. Duplicates the creature. To be used by storage retrieves
    #pragma warning disable 8603
    public virtual Creature Copy(){
        return null;

    }
    #pragma warning restore 8603

    //Gets a creature. Returns null if creature does not exist (or really if the ID is invalid)
    public static Creature GetCreature(int ID){
        Creature c;
        try{
            c = creatures[ID - 1];

        #pragma warning disable 168
        } catch(Exception e){
            c = null;
        }
        #pragma warning restore 168

        return c;
    }

    //Grabs the info and formats it for storage (being inputting within the text file)
    public string GetStorageInfo(){
        string info = id + "\n" + name + "\n" + stats.MakeString();
        return info;
    }


}

//The stats class. Handles levels, stat raises, other things
public class Stats{

    //Level of Creature
    public int level;

    //Attack of Creature
    public float attack;

    //Defense of creature
    public float defense;

    //Hp of creature; can be changed
    public float hp;

    //Max hp of the creature
    public float MAX_HP;

    //Current experience of the creature
    public float exp;

    /*
        Stats creation mostly used for first time obtians and wild creatures
    */
    public Stats(int level, float attack, float defense, float hp){
        this.level = level;
        this.attack = attack;
        this.defense = defense;
        this.hp = hp;
        MAX_HP = hp;
        exp = 0.0f;
    }

    /*
        Stats creation for user creatures. Storage uses this function a lot
    */
    public Stats(int level, float attack, float defense, float hp, float exp){
        this.level = level;
        this.attack = attack;
        this.defense = defense;
        this.hp = hp;
        MAX_HP = hp;
        this.exp = exp;
    }

    //Turns the stats into a string either for embeds or storage
    public string MakeString(){

        //Old code for back when stats was a struct. No need to change bc it's basically the same
        //Returns an empty string if the attack is zero, to treat as null
        if(attack == 0){
            return "";
        }
        /*
            In the end, the string will look like this:
            Level: <int>
            Attack: <float>
            Defense: <float>
            HP: <float>
            Experience: <float>

        */

        return "Level: " + level + "\nAttack: " + attack + "\nDefense: " + defense + "\nHP: " + hp + "\nExperience: " + exp;
    }

    public void ProcessNewExp(float exp){
        this.exp += exp;

        /*
            Growth curves will eventually be different for different creatures, but for now they will be uniform

            The actual exp to gain a level will be expodential with a rate of change per value being around .45x
            This makes our equation (e/.45)^.45x for leveling up
        */

        float levelExp = (float)Math.Exp((double)(level * .45f))/.45f;
        if(exp >= levelExp){
            exp = exp - levelExp;
            level++;
            ProcessNewLevel();
        }

    }

    //Handles all stat changes from level up
    private void ProcessNewLevel(){

        /*
            Attack/defense will be kind of logistic in terms of their additions.

            HP is linear 
        */
        attack += (float)Math.Log((double)level)/4;
        defense += (float)Math.Log((double)level)/4;

        MAX_HP = level * 3 + 12;
    }

}