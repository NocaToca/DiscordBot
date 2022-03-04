using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

public class Slimemon : Creature{

    private void init(){
        id = "002";
        name = "Slimemon";

        imageURL = "https://media.discordapp.net/attachments/948446188200419369/948781067530305556/89321781-d1ba-4a72-9fac-06e6c4b2cdc1.__CR014528801781_PT0_SX970_V1___.jpeg";
    }
    public Slimemon(){
        init();
    }

    public Slimemon(int level){
        init();
        stats = new Stats(level, 3.0f, 3.0f, 12.0f);

    }

    public override Creature Copy(){
        Slimemon c = new Slimemon(1);
        return c;
    }    

    public static void Start(){
        creatures.Add(new Slimemon());
    }

}