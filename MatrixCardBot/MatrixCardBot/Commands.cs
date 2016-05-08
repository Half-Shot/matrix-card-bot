using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using MatrixSDK.Client;
using MatrixSDK.Structures;
namespace MatrixCardBot
{
	[AttributeUsage(AttributeTargets.Method)]
	public class BotCmd : Attribute{
		public readonly string CMD;
		public readonly string[] BeginsWith;
        public BotCmd(string cmd,params string[] beginswith){
			CMD = cmd;
			BeginsWith = beginswith;
		}

	}

    [AttributeUsage(AttributeTargets.Method)]
    public class BotHelp : Attribute {
        public readonly string HelpText;
        public BotHelp(string help){
            HelpText = help;
        }
    }

	public class Commands
	{
		[BotCmd("ping")]
        [BotHelp("Pings the bot and returns the time on the system")]
		public static void Ping(string cmd, string sender, MatrixRoom room){
			room.SendMessage ("Pong: " + DateTime.Now.ToShortTimeString());
		}

		[BotCmd("help")]
        [BotHelp("This help text.")]
		public static void Help(string cmd, string sender, MatrixRoom room){
            string helptext = "";
            foreach(MethodInfo method in typeof(Commands).GetMethods(BindingFlags.Static|BindingFlags.Public)){
                BotCmd c = method.GetCustomAttribute<BotCmd> ();
                BotHelp h= method.GetCustomAttribute<BotHelp> ();
                if (c != null) {
                    helptext += String.Format("<p><strong>{0}</strong> {1}</p>",c.CMD, h != null ? h.HelpText : "");
                }
            }
            MMessageCustomHTML htmlmsg = new MMessageCustomHTML();
            htmlmsg.body = helptext.Replace("<strong>","").Replace("</strong>","").Replace("<p>","").Replace("</p>","\n");
            htmlmsg.formatted_body = helptext;
            room.SendMessage(htmlmsg);
       }

        [BotCmd("testcards")]
        [BotHelp("Print a deck of shuffled cards")]
        public static void TestCards(string cmd, string sender, MatrixRoom room){
            List<PlayingCard> deck = PlayingCard.GetStandardDeck();
            PlayingCard.ShuffleDeck(ref deck);
            string cards = PlayingCard.GetDeckHTML(deck);
            MMessageCustomHTML htmlmsg = new MMessageCustomHTML();
            htmlmsg.formatted_body = cards;
            htmlmsg.body = string.Join<PlayingCard>(" ",deck);
            room.SendMessage(htmlmsg);
        }

        [BotCmd("ttt","tictactoe","tic-tac-toe","tictacto")]
        [BotHelp("Play Tic Tac Toe!")]
        public static void TTTLobby(string cmd, string sender, MatrixRoom room){
            Program.LobbyManager.CreateTicTacToeLobby(sender);
        }

		[BotCmd("cheat","bullshit","I doubt it")]
        [BotHelp("Play Cheat! (experimental)")]
		public static void CheatLobby(string cmd, string sender, MatrixRoom room){
            Program.LobbyManager.CreateCheatLobby(sender);
		}
	}
}

