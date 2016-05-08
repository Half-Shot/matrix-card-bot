using System;
using System.Collections.Generic;
namespace MatrixCardBot
{

    public class PlayingCard
    {
        public readonly char suit = '♦';
        public readonly int value;
        
        public PlayingCard(char Suit,int Value){
            suit = Suit;
            value = Value;
        }
        
        public static int SuitToValue(char suit){
            switch(suit){
                case '♠':
                    return 1;
                case '♥':
                    return 2;
                case '♣':
                    return 3;
                case '♦':
                    return 4;
            }
            return -1;
        }

        public static char LetterToSuit(char letter){
            switch(letter){
                case 'S':
                    return '♠';
                case 'H':
                    return '♥';
                case 'C':
                    return '♣';
                case 'D':
                    return '♦';
            }
            return '/';
        }

        public static string ValueToString(int value){
            switch(value){
                case 11:
                    return "J";
                case 12:
                    return "Q";
                case 13:
                    return "K";
                case 1:
                    return "A";
                default:
                    return value.ToString();
            }
        }

        public static int StringToValue(string sval){
             if(sval == "J"){
                return 11;
            }
            else if(sval == "Q"){
                return 12;
            }
            else if(sval == "K"){
                return 13;
            }
            else if(sval == "A"){
                return 1;
            }
            else
            {
                return int.Parse(sval);
            }
        }

        public bool HigherThan(PlayingCard other){
            int val = other.value;
            if(value == 1){
                val = 14;
            }

            int oval = other.value;
            if(other.value == 1){
                oval = 14;
            }
        
            if(oval < val){
                return true;
            }
            else if (oval > val)
            {
                return false;
            }
            else
            {
                return (SuitToValue(this.suit)>SuitToValue(other.suit));
            }
        }

        public static int IndexOfCard(PlayingCard card,List<PlayingCard> hand){
            for(int i =0;i<hand.Count;i++){
                if(card.suit == hand[i].suit && card.value == hand[i].value){
                    return i;
                }
            }
            return -1;
        }

        public static bool TryParse(string cardset,out PlayingCard card){
            int val = 0;
            card = null;
            if(cardset.Length > 3){
                return false;
            }
            char suit = LetterToSuit(cardset[0]);
            val = StringToValue(cardset.Substring(1));
            if(val < 1 || val > 13 || SuitToValue(suit) == -1){
                return false;
            }
            card = new PlayingCard(suit,val);
            return true;
        }

        public string GetHTML(){
            string val;
            if(value == 11){
                val = "J";
            }
            else if(value == 12){
                val = "Q";
            }
            else if(value == 13){
                val = "K";
            }
            else if(value == 1){
                val = "A";
            }
            else
            {
             val = value.ToString();
            }

            

            return  string.Format ("[<font color=\"{2}\">{0}</font>{1}]",suit,val,suit == '♦' || suit == '♥' ? "FF0000" : "000000");
        }

        public override string ToString ()
        {
            string val;
            
            if(value == 11){
                val = "J";
            }
            else if(value == 12){
                val = "Q";
            }
            else if(value == 13){
                val = "K";
            }
            else if(value == 1){
                val = "A";
            }
            else
            {
             val = value.ToString();
            }
            return string.Format ("[{0}{1}]",suit,val);
        }

        public static void ShuffleDeck(ref List<PlayingCard> cards){
            Random random = new Random();
            for(int i = 0;i<random.Next(75,150);i++){
                int x = random.Next(0,cards.Count-1);
                int y = random.Next(0,cards.Count-1);
                PlayingCard card = new PlayingCard(cards[x].suit,cards[x].value);
                cards[x] = cards[y];
                cards[y] = card;
            }
        }

        public static void SortDeckByValue(ref List<PlayingCard> cards){
            cards.Sort( (x,y) => x.value-y.value);
        }

        public static string GetDeckHTML(List<PlayingCard> cards){
            string html = "<code>";
            cards.ForEach( x => html += " " + x.GetHTML());
            html += "</code>";
            return html;
        }

        public static List<PlayingCard>  GetStandardDeck(){
            return new List<PlayingCard>(56){
                new PlayingCard('♠',1),
                new PlayingCard('♠',2),
                new PlayingCard('♠',3),
                new PlayingCard('♠',4),
                new PlayingCard('♠',5),
                new PlayingCard('♠',6),
                new PlayingCard('♠',7),
                new PlayingCard('♠',8),
                new PlayingCard('♠',9),
                new PlayingCard('♠',10),
                new PlayingCard('♠',11),
                new PlayingCard('♠',12),
                new PlayingCard('♠',13),
                new PlayingCard('♥',1),
                new PlayingCard('♥',2),
                new PlayingCard('♥',3),
                new PlayingCard('♥',4),
                new PlayingCard('♥',5),
                new PlayingCard('♥',6),
                new PlayingCard('♥',7),
                new PlayingCard('♥',8),
                new PlayingCard('♥',9),
                new PlayingCard('♥',10),
                new PlayingCard('♥',11),
                new PlayingCard('♥',12),
                new PlayingCard('♥',13),
                new PlayingCard('♣',1),
                new PlayingCard('♣',2),
                new PlayingCard('♣',3),
                new PlayingCard('♣',4),
                new PlayingCard('♣',5),
                new PlayingCard('♣',6),
                new PlayingCard('♣',7),
                new PlayingCard('♣',8),
                new PlayingCard('♣',9),
                new PlayingCard('♣',10),
                new PlayingCard('♣',11),
                new PlayingCard('♣',12),
                new PlayingCard('♣',13),
                new PlayingCard('♦',1),
                new PlayingCard('♦',2),
                new PlayingCard('♦',3),
                new PlayingCard('♦',4),
                new PlayingCard('♦',5),
                new PlayingCard('♦',6),
                new PlayingCard('♦',7),
                new PlayingCard('♦',8),
                new PlayingCard('♦',9),
                new PlayingCard('♦',10),
                new PlayingCard('♦',11),
                new PlayingCard('♦',12),
                new PlayingCard('♦',13)
            };
        }
 
    }
}

