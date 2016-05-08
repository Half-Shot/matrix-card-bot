using System;
using MatrixSDK.Client;
using MatrixSDK.Structures;

namespace MatrixCardBot
{
    public class TicTacToe : Lobby
    {
        int[] brd = new int[9];
        int currentPlayer = 0;
        public TicTacToe (MatrixRoom Room, MatrixUser Owner,MatrixClient Client): base(Room,Owner,Client){
            MinPlayers = 2;
            MaxPlayers = 2;
           room.SendMessage("The rules are simple, get a straight set of Xs or 0s. Type A1 for top left and C3 for bottom right.");
        }

        public override void GameStart ()
        {
            currentPlayer = new Random(DateTime.Now.Second).Next(0,1);
            room.SendMessage(users[currentPlayer].DisplayName + " will go first (as X)");
            GameInSession = true;
        }

        public void CheckForWin(){
            int[,] rows = new int[8,3]{{0,1,2},{0,3,6},{0,4,8},{2,5,8},{1,4,7},{3,4,5},{6,7,8},{2,4,6}};
            int boned = 0;
            for(int i=0;i<8;i++){
                int a = brd[rows[i,0]];
                int b = brd[rows[i,1]];
                int c = brd[rows[i,2]];

                int res = a&b&c;
                boned += Convert.ToInt32((a != b && a != 0) || (c != b && c != 0) && b != 0);
                if(res != 0){
                    room.SendMessage("🎉 The winner is " + users[res-1].DisplayName + " 🎉");
                    GameInSession = false;
                    //room.LeaveRoom();
                }
                else if(boned == 8){
                    room.SendMessage("🕱 No more moves can be played. A draw. How dull 🕱");
                    GameInSession = false;
                    //room.LeaveRoom();
                }
            }
        }

        public string GetBoardString(){
            string str = "";
            for(int i =0;i<brd.Length;i++){
                if(brd[i] == 0){
                    str += "_ ";
                } 
                else if(brd[i] == 1){
                    str += "X ";
                }
                else if(brd[i] == 2){
                    str += "O ";
                }
                if((i+1) % 3 == 0){
                    str += '\n';
                }
            }
            return str;
        }
    
        public override bool PlayerMessage (MatrixUser user, MatrixMRoomMessage message)
        {
            if(!base.PlayerMessage (user, message)){
                if(GameInSession){
                    if(user.UserID != users[currentPlayer].UserID){
                        return true;
                    }
                    string cmd = message.body.Trim().ToLower();
                    if(cmd.Length == 2){
                        int a  = ((int)cmd[0])-96;
                        int b = int.Parse(new string(cmd[1],1));
                        if(a < 1 || a > 3 || b < 1 || b > 3){
                            room.SendMessage("Invalid position");
                        }

                        int square = ((a-1)*3) + (b-1);
                        if(brd[square] == 0){
                            brd[square] = currentPlayer+1;
                            room.SendMessage(GetBoardString());
                            currentPlayer = currentPlayer == 0 ? 1 : 0;
                        }
                        else
                        {
                            room.SendMessage("That spot is taken");
                        }
                        CheckForWin();
                    }
                }
            }
            return true;
        }
    }
}

