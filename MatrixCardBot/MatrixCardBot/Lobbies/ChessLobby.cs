using System;

using MatrixSDK.Client;
using MatrixSDK.Structures;
namespace MatrixCardBot
{
    public class ChessLobb : Lobby {
        public ChessLobb(MatrixRoom Room, MatrixUser Owner,MatrixClient Client) : base(Room,Owner,Client){
            MinPlayers = 2;
            room.SendMessage("The rules are simple, get a straight set of Xs or 0s. Type A1 for top left and C3 for bottom right.");
        }
    }
}