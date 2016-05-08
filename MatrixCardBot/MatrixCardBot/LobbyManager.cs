using System;
using System.Collections.Generic;
using MatrixSDK;
using MatrixSDK.Client;
using MatrixSDK.Structures;
namespace MatrixCardBot
{
	public class LobbyManager : IDisposable
	{
		MatrixClient client;
        List<Lobby> lobbies;
		public LobbyManager (MatrixClient client)
		{
			this.client = client;
            lobbies = new List<Lobby>();
		}

        public void FindUserInRoom(){
            
        }

        public void CreateCheatLobby(string sender){
            MatrixUser user = client.GetUser(sender);
            MatrixRoom room = CreateLobbyRoom(user,"Cheat");
            
            Lobby lobby = new CheatLobby(room,user,client);
            lobbies.Add(lobby);
        }

        public void CreateTicTacToeLobby(string sender){
            MatrixUser user = client.GetUser(sender);
            MatrixRoom room = CreateLobbyRoom(user,"Tic Tac Toe");
            
            Lobby lobby = new TicTacToe(room,user,client);
            lobbies.Add(lobby);
        }

        private MatrixRoom CreateLobbyRoom(MatrixUser sender,string gamename){
			MatrixRoom room = client.CreateRoom(new MatrixSDK.Structures.MatrixCreateRoom(){
				invite = new string[1]{sender.UserID},
				name = "[Card] "+gamename+" Lobby",
                topic = gamename + " Lobby for " + sender.DisplayName,
				visibility = EMatrixCreateRoomVisibility.Public
			});

            room.ApplyNewPowerLevels( new MatrixSDK.Structures.MatrixMRoomPowerLevels(){
                events_default = 10,
                users = new System.Collections.Generic.Dictionary<string, int>(){ { "@cardbot:half-shot.uk",100 },{ sender.UserID, 80 } },
                state_default = 80,
                redact = 20,
                ban = 80,
                kick = 80,
                users_default = 10
            });
            return room;
		}
        
        public void Dispose(){
            foreach(Lobby lobby in lobbies){
                lobby.Dispose();
            }
        }
	}

	public abstract class Lobby : IDisposable {
        protected MatrixClient client;
		public MatrixRoom room;
		protected MatrixUser owner;
        protected List<MatrixUser> users;
        protected int MinPlayers;
        protected int MaxPlayers;
        protected bool GameInSession = false;

        public Lobby(MatrixRoom Room, MatrixUser Owner,MatrixClient Client){
            room = Room;
            owner = Owner;
            client = Client;
            room.SendMessage("You've created a new lobby, the game will commence once the owner has said 'START'");
            room.OnEvent += Room_OnEvent;
            users = new List<MatrixUser>();
        }

        public virtual void PlayerLeave(MatrixUser user){
            int before = users.Count;
            users.RemoveAll( x => x.UserID == user.UserID);
            if(users.Count == 0){
                this.Dispose();
            }
            if(GameInSession && before != users.Count){
                room.SendMessage("The game was abandoned by " + user.DisplayName);
                this.Dispose();
            }
        }

        public virtual void PlayerJoin(MatrixUser user){
            if(users.Count < MaxPlayers && !GameInSession && user.UserID != "@cardbot:half-shot.uk"){
                users.Add(user);
            }
        }

        public virtual void GameStart(){
            room.SendMessage("The game begins!");
            GameInSession = true;
        }

        public virtual bool PlayerMessage(MatrixUser user,MatrixMRoomMessage message){
            if(user.UserID == "@cardbot:half-shot.uk")
                return true;
            if(message.ToString() == "START" && user.UserID == owner.UserID){
                if(users.Count < MinPlayers){
                    room.SendMessage("Cannot start, you need " + (MinPlayers - users.Count).ToString() + " more players");
                }
                else
                {
                    GameStart();
                }
                return true;
            }
            return false;
        }

        void Room_OnEvent (MatrixRoom room, MatrixEvent evt)
        {
            if(evt.sender == null)
                return;
            MatrixUser user = client.GetUser(evt.sender);
            //Deal with players leaving or joining
            if(evt.type == "m.room.member"){
                EMatrixRoomMembership memtype = ((MatrixMRoomMember)evt.content).membership;
                if(memtype == EMatrixRoomMembership.Leave){
                    PlayerLeave(user);
                }
                else if(memtype == EMatrixRoomMembership.Join)
                {
                    PlayerJoin(user);
                }
            }
            else if(evt.type == "m.room.message"){
                PlayerMessage(user,(MatrixMRoomMessage)evt.content);
            }
        }

        public virtual void Dispose(){
            room.LeaveRoom();
        }
	}

}

