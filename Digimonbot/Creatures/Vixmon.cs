using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

public class Vixmon : Creature{

    private void init(){
        id = "001";
        name = "Vixmon";

        imageURL = "https://media.discordapp.net/attachments/948446188200419369/948446301949939722/Vixmon.jpg?width=473&height=676";
    }
    public Vixmon(){
        init();
    }
    public Vixmon(int level){
        init();
        
        stats = new Stats(level, 3.0f, 3.0f, 12.0f);

    }

    public override Creature Copy(){
        Vixmon c = new Vixmon(1);
        return c;
    }

    public static void Start(){

        creatures.Add(new Vixmon());

    }

}