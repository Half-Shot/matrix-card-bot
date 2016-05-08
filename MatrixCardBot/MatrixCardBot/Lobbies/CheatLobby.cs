using System;
using System.Collections.Generic;
using MatrixSDK.Client;
using MatrixSDK.Structures;
using System.Linq;
namespace MatrixCardBot
{
    public class CheatLobby : Lobby {
        List<PlayingCard> deck;
        Dictionary<string,MatrixRoom> user_rooms;
        Dictionary<string,List<PlayingCard>> hands;
        
        int currentPlayer;
        int lastPlayer;
        int expectedValue;
        bool playerLied;
        bool cheatCalled;
        public CheatLobby(MatrixRoom Room, MatrixUser Owner,MatrixClient Client) : base(Room,Owner,Client){
            room.SendMessage("This lobby is for 'Cheat'.");
            room.SendMessage(new MMessageCustomHTML(){
                body = "See https://en.wikipedia.org/wiki/Cheat_%28game%29#Gameplay for help",
                formatted_body = " See <a href=\"https://en.wikipedia.org/wiki/Cheat_%28game%29#Gameplay\">the wikipedia page</a> for help"
            });
            room.SendNotice("This game requires you to keep your hand hidden, so a secondary invite has also been sent");
            room.SendNotice("To call cheat on the current player, say 'Cheat! in this room.'");
            MinPlayers = 2;
            MaxPlayers = 8;
        }

        public override void GameStart ()
        {
            playerLied = false;
            currentPlayer = 0;
            lastPlayer = -1;
            expectedValue = 2;
            base.GameStart ();
            //Dish out cards to players
            deck = PlayingCard.GetStandardDeck();
            PlayingCard.ShuffleDeck(ref deck);
            hands = new Dictionary<string, List<PlayingCard>>();
            user_rooms = new Dictionary<string, MatrixRoom>();
            foreach(MatrixUser user in users){
                hands.Add(user.UserID,new List<PlayingCard>());
            }
            int i =0;
            while(deck.Count > 0){
                hands[users[i].UserID].Add(deck[0]);
                deck.RemoveAt(0);
                i = (i + 1) % users.Count;
            }

            foreach(KeyValuePair<string,List<PlayingCard>> deck in hands){
                //Create a room and send the player their cards.
                Console.WriteLine(deck.Key + "'s hand:" + string.Join<PlayingCard>(" ",deck.Value));
                MatrixRoom uroom = client.CreateRoom(new MatrixCreateRoom(){
                    invite = new string[1]{deck.Key},
                    name = "[Card:Hand] Hand for Cheat Lobby",
                    topic = "This room will pass you information",
                    visibility = EMatrixCreateRoomVisibility.Private
                });
                user_rooms.Add(deck.Key,uroom);
                SendDeckUpdate(deck.Key);
                uroom.SendNotice("To play a card, type [H,D,S,C] [2-10,J,Q,K,A] in your **private room**. You may type multiple cards seperated by a space.");
                uroom.SendNotice("To to check your hand, type 'hand'.");
                uroom.OnMessage += HandMessage;
                string name = (users.First(x => x.UserID == deck.Key)).DisplayName;
            }
        }

        private void SendDeckUpdate(string userid){
            MMessageCustomHTML msg = new MMessageCustomHTML();
            List<PlayingCard> hand = hands[userid];
            PlayingCard.SortDeckByValue(ref hand);
            msg.formatted_body = "Your hand:"+ PlayingCard.GetDeckHTML(hand);
            msg.body = "Your hand:"+ string.Join<PlayingCard>(" ",hand);
            user_rooms[userid].SendMessage(msg);
        }

        private void CallCheatOnPlayer(MatrixUser caller){
            room.SendNotice(caller.DisplayName + " called " + users[lastPlayer].DisplayName + " a cheat...");
            System.Threading.Thread.Sleep(1000);
            cheatCalled = true;
            string reciever;
            if(playerLied){
                room.SendNotice("They are correct! The cheat will recieve " + deck.Count + " cards.");
                reciever = users[lastPlayer].UserID;
            }
            else
            {
                room.SendNotice("They are incorrect! The caller will recieve " + deck.Count + " cards.");
                reciever = caller.UserID;
            }

            while(deck.Count > 0){
                hands[reciever].Add(deck[0]);
                deck.RemoveAt(0);
            }
            SendDeckUpdate(reciever);
        }

        private void CheckForWinner(){
            if(lastPlayer != -1){
                if(hands[users[lastPlayer].UserID].Count == 0){
                    MatrixUser user = users[lastPlayer];
                    room.SendNotice("🎉" + user.DisplayName + " has cleared their hand!");
                    users.Remove(user);
                    if(users.Count == 1){
                        GameInSession = true;
                        room.SendNotice("🎉 Game Over! Everyone wins except " + users[0].DisplayName);
                        Dispose();
                    }
                }
            }
        }

        private void NextTurn(){
                lastPlayer = currentPlayer;
                currentPlayer = (currentPlayer + 1) % users.Count;
                expectedValue++;
                if(expectedValue > 13){
                    expectedValue = 1;
                }
                cheatCalled = false;
                room.SendNotice(users[currentPlayer].DisplayName + " is up next, expecting cards valued at " + PlayingCard.ValueToString(expectedValue));
        }

        public override bool PlayerMessage (MatrixUser user, MatrixMRoomMessage message)
        {
            if(!base.PlayerMessage (user, message)){
                if(GameInSession){
                    if(message.body.ToLower() == "count"){
                        string msg = "Card counts:\n";
                        foreach(MatrixUser u in users){
                            msg += user.DisplayName + " has " + hands[user.UserID].Count + " cards\n"; 
                        }
                        room.SendMessage(msg);
                    }
                    else if(message.body.ToLower() == "deck"){
                        room.SendMessage("The deck contains " + deck.Count + " cards");
                    }
                    else if( lastPlayer != -1)
                    {
                        if(message.body.ToLower() == "cheat!" && user.UserID != users[lastPlayer].UserID){
                            if(!cheatCalled){
                                CallCheatOnPlayer(user);
                                CheckForWinner();
                            }
                            else
                            {
                                room.SendNotice("Cheat has already been called this round!");
                            }
                        }
                    }
                }
            }
            return false;
        }

        void HandMessage (MatrixRoom uroom, MatrixEvent evt)
        {
            if(GameInSession){
                string data = ((MatrixMRoomMessage)evt.content).body;
                if(data.ToLower() == "hand")
                {
                    SendDeckUpdate(evt.sender);
                    return;
                }
                if(users[currentPlayer].UserID == evt.sender){
                    //Do hand stuff
                    string[] cards = data.ToUpper().Trim().Split(' ');
                    if(cards.Length > 4){
                        uroom.SendNotice("Dude, think it over. How can you have more than 4 cards of the same value?");
                        return;
                    }
                    List<PlayingCard> outdeck = new List<PlayingCard>();
                    foreach(string scard in cards){
                        PlayingCard card;
                        if(!PlayingCard.TryParse(scard,out card)){
                            uroom.SendNotice("I could not recognise all the cards there, please revise.");
                            return;
                        }
                        outdeck.Add(card);
                    }
                    List<PlayingCard> player_hand = hands[evt.sender];
                    bool all = outdeck.All( x => player_hand.Any(y => (x.suit == y.suit && x.value == y.value) ));
                    if(all){
                        room.SendNotice(users[currentPlayer].DisplayName + " has put forward " + outdeck.Count + " cards. They have " + (player_hand.Count - outdeck.Count) + " cards remaining" );
                        playerLied = !outdeck.All( x => x.value == expectedValue);
                        deck.AddRange(outdeck);
                        CheckForWinner();
                        foreach(PlayingCard card in outdeck){
                            player_hand.RemoveAll( x => x.suit == card.suit && x.value == card.value);
                        }
                        NextTurn();

                    }
                    else
                    {
                        uroom.SendNotice("Some of the cards you listed aren't in your hand, please revise.");
                    }
                }
            }
            else
            {
                room.LeaveRoom();
            }
        }

        public override void Dispose ()
        {
            if(user_rooms != null){
                foreach(MatrixRoom room in user_rooms.Values){
                    room.LeaveRoom();
                }
            }
            base.Dispose ();
        }
    } 
}

