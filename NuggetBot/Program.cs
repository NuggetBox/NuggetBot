using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrettioEtt
{
    public enum Suit { Hjärter, Ruter, Spader, Klöver };

    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            Game game = new Game();

            List<Player> players = new List<Player>();

            players.Add(new NuggetBot());
            players.Add(new Bot2Beat());
            players.Add(new BasicPlayer());
            players.Add(new ICommitDie());
            players.Add(new ICommitDie2());
            players.Add(new MathBot1());
            players.Add(new MathBot2());

            Console.WriteLine("Vilka två spelare skall mötas?");
            for (int i = 1; i <= players.Count; i++)
            {
                Console.WriteLine(i + ": {0}", players[i - 1].Name);
            }
            int p1 = int.Parse(Console.ReadLine());
            int p2 = int.Parse(Console.ReadLine());
            Player player1 = players[p1 - 1];
            Player player2 = players[p2 - 1];
            player1.Game = game;
            player1.PrintPosition = 0;
            player2.Game = game;
            player2.PrintPosition = 9;
            game.Player1 = player1;
            game.Player2 = player2;
            Console.WriteLine("Hur många spel skall spelas?");
            int numberOfGames = int.Parse(Console.ReadLine());
            Console.WriteLine("Skriva ut första spelet? (y/n)");
            string print = Console.ReadLine();
            Console.Clear();
            if (print == "y")
                game.Printlevel = 2;
            else
                game.Printlevel = 0;
            game.initialize();
            game.PlayAGame(true);
            Console.Clear();
            bool player1starts = true;

            for (int i = 1; i < numberOfGames; i++)
            {
                game.Printlevel = 0;
                player1starts = !player1starts;
                game.initialize();
                game.PlayAGame(player1starts);

                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(0, 3);
                Console.Write(player1.Name + ":");
                Console.ForegroundColor = ConsoleColor.Green;

                Console.SetCursorPosition((player1.Wongames * 100 / numberOfGames) + 15, 3);
                Console.Write("█");
                Console.SetCursorPosition((player1.Wongames * 100 / numberOfGames) + 16, 3);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(player1.Wongames);

                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(0, 5);
                Console.Write(player2.Name + ":");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition((player2.Wongames * 100 / numberOfGames) + 15, 5);
                Console.Write("█");
                Console.SetCursorPosition((player2.Wongames * 100 / numberOfGames) + 16, 5);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(player2.Wongames);

            }
            Console.ReadLine();

        }

    }

    class Card
    {
        public int Value { get; private set; } //Kortets värde enligt reglerna i Trettioett, t.ex. dam = 10
        public Suit Suit { get; private set; }
        private int Id; //Typ av kort, t.ex dam = 12

        public Card(int id, Suit suit)
        {
            Id = id;
            Suit = suit;
            if (id == 1)
            {
                Value = 11;
            }
            else if (id > 9)
            {
                Value = 10;
            }
            else
            {
                Value = id;
            }
        }

        public void PrintCard()
        {
            string cardname = "";
            if (Id == 1)
            {
                cardname = "ess ";
            }
            else if (Id == 10)
            {
                cardname = "tio ";
            }
            else if (Id == 11)
            {
                cardname = "knekt ";
            }
            else if (Id == 12)
            {
                cardname = "dam ";
            }
            else if (Id == 13)
            {
                cardname = "kung ";
            }
            if (Suit == Suit.Hjärter)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (Suit == Suit.Ruter)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (Suit == Suit.Spader)
                Console.ForegroundColor = ConsoleColor.Gray;
            else if (Suit == Suit.Klöver)
                Console.ForegroundColor = ConsoleColor.Green;

            Console.Write(" " + Suit + " " + cardname);
            if (cardname == "")
            {
                Console.Write(Id + " ");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

    }

    class Game
    {

        List<Card> CardDeck = new List<Card>();
        List<Card> DiscardPile = new List<Card>();
        public bool Lastround;
        int Cardnumber;
        public int Printlevel;
        public int Discardnumber;
        public Player Player1 { private get; set; }
        public Player Player2 { private get; set; }
        Random RNG = new Random();
        public int NbrOfRounds;

        public Game()
        {

        }

        public void initialize()
        {
            Lastround = false;
            Cardnumber = -1;
            Discardnumber = 52;
            CardDeck = new List<Card>();
            DiscardPile = new List<Card>();
            Player1.Hand = new List<Card>();
            Player2.Hand = new List<Card>();

            int id;
            int suit;
            for (int i = 0; i < 52; i++)
            {
                id = i % 13 + 1;
                suit = i % 4;
                CardDeck.Add(new Card(id, (Suit)suit));
            }
            Shuffle();
            for (int i = 0; i < 3; i++)
            {
                Player1.Hand.Add(DrawCard());
                Player2.Hand.Add(DrawCard());
            }
            Discard(DrawCard());
        }

        public void printHand(Player player)
        {
            Console.SetCursorPosition(0, player.PrintPosition);
            Console.WriteLine(player.Name + " har ");
            for (int i = 0; i < player.Hand.Count; i++)
            {
                player.Hand[i].PrintCard();
                Console.WriteLine();
            }
        }

        private int playARound(Player player, Player otherPlayer)
        {
            if (Printlevel > 1)
            {
                printHand(player);
                Console.SetCursorPosition(4, 6);
                Console.Write("På skräphögen ligger ");
                DiscardPile.Last().PrintCard();

            }
            otherPlayer.OpponentsLatestCard = null;
            if (NbrOfRounds > 1 && player.Knacka(NbrOfRounds) && !Lastround)
            {
                if (Printlevel > 1)
                {
                    Console.SetCursorPosition(20, player.PrintPosition + 2);
                    Console.Write(player.Name + " knackar!");
                }
                return Score(player);
            }
            else if (player.TaUppKort(DiscardPile.Last()))
            {

                player.Hand.Add(PickDiscarded());
                otherPlayer.OpponentsLatestCard = player.Hand.Last();
                if (Printlevel > 1)
                {
                    Console.SetCursorPosition(20, player.PrintPosition + 2);
                    Console.Write(player.Name + " plockar ");
                    player.Hand.Last().PrintCard();
                    Console.Write(" från skräphögen.");
                }
            }
            else
            {
                player.Hand.Add(DrawCard());
                if (Printlevel > 1)
                {
                    Console.SetCursorPosition(20, player.PrintPosition + 2);
                    Console.Write(player.Name + " drar ");
                    player.Hand.Last().PrintCard();
                }
            }
            Card discardcard = player.KastaKort();

            UpdateHand(player, discardcard);
            if (Printlevel > 1)
            {
                Console.SetCursorPosition(20, player.PrintPosition + 3);
                Console.Write(player.Name + " kastar bort ");
                discardcard.PrintCard();
                Console.Write("       Tryck ENTER");
                Console.ReadLine();
                Console.Clear();
                printHand(player);

            }

            Discard(discardcard);
            if (Score(player) == 31)
            {
                return 31;
            }
            else
            {
                return 0;
            }
        }

        private void UpdateHand(Player player, Card discardcard)
        {
            player.Hand.Remove(discardcard);

        }

        public int Score(Player player) //Reurnerar spelarens poäng av bästa färg. Uppdaterar player.bestsuit.
        {
            int[] suitScore = new int[4];
            if (player.Hand[0].Value == 11 && player.Hand[1].Value == 11 && player.Hand[2].Value == 11)
            {
                return 31;
            }

            for (int i = 0; i < player.Hand.Count; i++)
            {
                if (player.Hand[i] != null)
                    suitScore[(int)player.Hand[i].Suit] += player.Hand[i].Value;
            }
            int max = 0;

            for (int i = 0; i < 4; i++)
            {
                if (suitScore[i] > max)
                {
                    max = suitScore[i];
                    player.BestSuit = (Suit)i;
                }
            }
            return max;

        }

        public int SuitScore(List<Card> hand, Suit suit) //Reurnerar handens poäng av en viss färg
        {
            int sum = 0;
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i] != null && hand[i].Suit == suit)
                {
                    sum += hand[i].Value;
                }

            }
            return sum;


        }

        public int HandScore(List<Card> hand, Card excluded) //Reurnerar handens poäng av bästa färg. Undantar ett kort från beräkningen (null för att ta med alla kort)
        {
            int[] suitScore = new int[4];
            int aces = 0;
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i] != null && hand[i] != excluded)
                {
                    suitScore[(int)hand[i].Suit] += hand[i].Value;
                    if (hand[i].Value == 11)
                    {
                        aces++;
                    }
                }

            }
            if (aces == 3)
                return 31;
            int max = 0;

            for (int i = 0; i < 4; i++)
            {
                if (suitScore[i] > max)
                {
                    max = suitScore[i];
                }
            }
            return max;
        }

        public void PlayAGame(bool player1starts)
        {
            NbrOfRounds = 0;
            Player playerInTurn, playerNotInTurn, temp;
            if (player1starts)
            {
                playerInTurn = Player1;
                playerNotInTurn = Player2;
            }
            else
            {
                playerInTurn = Player2;
                playerNotInTurn = Player1;
            }
            while (Cardnumber < 51 && NbrOfRounds < 100)
            {
                NbrOfRounds++;
                int result = playARound(playerInTurn, playerNotInTurn);
                if (result == 31)
                {
                    if (Printlevel > 1)
                        printHand(playerNotInTurn);

                    playerInTurn.SpelSlut(true);
                    playerNotInTurn.SpelSlut(false);
                    if (Printlevel > 0)
                    {
                        Console.SetCursorPosition(15, playerInTurn.PrintPosition + 5);
                        Console.Write(playerInTurn.Name + " fick 31 och vann spelet!");
                        Console.ReadLine();
                    }
                    break;
                }
                else if (result > 0)
                {
                    Lastround = true;
                    playerNotInTurn.lastTurn = true;
                    playARound(playerNotInTurn, playerInTurn);
                    playerNotInTurn.lastTurn = false;
                    if (Printlevel > 1)
                    {
                        printHand(playerInTurn);
                        printHand(playerNotInTurn);
                    }


                    if (Printlevel > 0)
                    {
                        Console.SetCursorPosition(15, playerInTurn.PrintPosition + 5);
                        Console.Write(playerInTurn.Name + " knackade och har " + Score(playerInTurn) + " poäng");
                        Console.SetCursorPosition(15, playerNotInTurn.PrintPosition + 5);
                        Console.Write(playerNotInTurn.Name + " har " + Score(playerNotInTurn) + " poäng");
                    }
                    if (Score(playerInTurn) > Score(playerNotInTurn))
                    {
                        playerInTurn.SpelSlut(true);
                        playerNotInTurn.SpelSlut(false);
                        if (Printlevel > 0)
                        {
                            Console.SetCursorPosition(15, playerInTurn.PrintPosition + 6);
                            Console.WriteLine(playerInTurn.Name + " vann!");
                            Console.ReadLine();
                        }
                        break;
                    }
                    else
                    {
                        playerInTurn.SpelSlut(false);
                        playerNotInTurn.SpelSlut(true);
                        if (Printlevel > 0)
                        {
                            Console.SetCursorPosition(15, playerNotInTurn.PrintPosition + 6);
                            Console.WriteLine(playerNotInTurn.Name + " vann!");
                            Console.ReadLine();
                        }
                        break;
                    }
                }
                else
                {
                    temp = playerNotInTurn;
                    playerNotInTurn = playerInTurn;
                    playerInTurn = temp;
                }

            }
            if (Cardnumber >= 51 || NbrOfRounds >= 100)
            {
                if (Printlevel > 0)
                {
                    Console.SetCursorPosition(0, 20);
                    Console.WriteLine("Korten tog slut utan att någon spelare vann.");
                    Console.ReadLine();
                }
                playerInTurn.SpelSlut(false);
                playerNotInTurn.SpelSlut(false);

            }
        }

        private void Discard(Card card)
        {
            Discardnumber--;
            DiscardPile.Add(card);
        }

        private Card DrawCard()
        {
            Cardnumber++;
            Card card = CardDeck.First();
            CardDeck.RemoveAt(0);
            return card;
        }

        private Card PickDiscarded()
        {
            Card card = DiscardPile.Last();
            Discardnumber++;
            return card;
        }



        private void Shuffle()
        {
            for (int i = 0; i < 200; i++)
            {
                switchCards();
            }
        }

        private void switchCards()
        {
            int card1 = RNG.Next(CardDeck.Count);
            int card2 = RNG.Next(CardDeck.Count);
            Card temp = CardDeck[card1];
            CardDeck[card1] = CardDeck[card2];
            CardDeck[card2] = temp;
        }
    }

    abstract class Player
    {
        public string Name;
        // Dessa variabler får ej ändras

        public List<Card> Hand = new List<Card>();  // Lista med alla kort i handen. Bara de tre första platserna har kort i sig när rundan börjar, ett fjärde läggs till när man tar ett kort
        public Game Game;
        public Suit BestSuit; // Den färg med mest poäng. Uppdateras varje gång game.Score anropas
        public int Wongames;
        public int PrintPosition;
        public bool lastTurn; // True om motståndaren har knackat, annars false.
        public Card OpponentsLatestCard; // Det senaste kortet motståndaren tog. Null om kortet drogs från högen.

        public abstract bool Knacka(int round);
        public abstract bool TaUppKort(Card card);
        public abstract Card KastaKort();
        public abstract void SpelSlut(bool wonTheGame);
    }
    
    class NuggetBot : Player
    {
        List<Card> opponentEstHand = new List<Card>();

        Card scrapCard;

        int
            thisTurnNum = 0,
            amountKnownCards,
            lastTurnToGoWide = -13,
            lowestScrapValueToGoWide = 10,
            highestHandValueToGoWide = 17,
            highestLowValueCard = 3;
        float
            averageCardValue = 7.3f;

        public NuggetBot()
        {
            Name = "NuggetBot";
        }

        // Benjamin
        public override bool Knacka(int round) //Returnerar true om spelaren skall knacka, annars false
        {
            ++thisTurnNum;
            amountKnownCards = opponentEstHand.Count;
            SortHand();

            int whenToKnock = WhenToKnock(thisTurnNum);

            if (Game.Score(this) >= whenToKnock)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool TaUppKort(Card scrapCard) // Returnerar true om spelaren skall ta upp korten på skräphögen (card), annars false för att dra kort från leken. Kortet i parametern är skräphögskortet.
        {
            this.scrapCard = scrapCard;
            SortHand();
            int preGameScore = Game.Score(this);
            OpponentGuess(scrapCard);
            Card newWorstCard;
            newWorstCard = GetWorstCard(Hand);

            // Om vi har 2 ess redan så tar vi såklart upp ett tredje från skräphögen.
            if (GetNumOfAceInHand() >= 2 && scrapCard.Value == 11)
                return true;

            // Om de har knackat, eller om det har passerat rundan vi vill utvidga våra suits på så vill vi bara förbättra vår hand så mycket som möjligt.
            if (lastTurn || thisTurnNum > lastTurnToGoWide)
            {
                Hand.Add(scrapCard);
                Hand.Remove(newWorstCard);

                return IsTheNewHandBetter(preGameScore, scrapCard, newWorstCard);
            }
            // Om det är tidigt i spelet så vill vi hellre satsa på flera olika suits och inte öka vår hand med små poäng som 2 eller 3.
            else
            {
                Hand.Add(scrapCard);
                if (scrapCard.Value >= lowestScrapValueToGoWide && Game.Score(this) <= highestHandValueToGoWide && KastaKort() != scrapCard)
                {
                    Hand.Remove(scrapCard);
                    return true;
                }
                else
                {
                    Hand.Remove(newWorstCard);

                    return IsTheNewHandBetter(preGameScore, scrapCard, newWorstCard);
                }
            }
        }
        
        public override Card KastaKort()  // Returnerar det kort som skall kastas av de fyra som finns på handen. Game.Score(this) returnerar värdet av bestSuit bland alla 4 kort. 
        {
            // Benjamins sorterings kod som är väldigt användbar för att hitta det lägsta kortet.
            SortHand();
            Game.Score(this);

            // Måns
            // Användbara variabler.
            Card worstCard = null;
            Card worstBestSuitCard = null;
            Suit opponentSuit;
            List<Card> notBestSuits = new List<Card>();
            int aceCounter = GetNumOfAceInHand();

            //Om vi har tre ess (Hade vi haft fyra så borde vi redan vunnit) ska vi slänga det kortet som inte är ett ess så att vi vinner direkt.
            if (aceCounter == 3)
                worstCard = Hand[0];

            if (aceCounter <= 2)
            {
                // För att hitta det traditionelt sämsta kortet i handen.
                worstCard = GetWorstCard(Hand);

                // Följande är för att hitta undantagen.

                // Vi vill undvika att ge motsåndaren fler kort av den färgen han samlar på, därför ger vi motståndaren i de flesta fall det sämsta kortet utav de som varken är vår eller hans färg.
                opponentSuit = GetOpponentEstSuit();
                if (worstCard.Suit == opponentSuit)
                {
                    for (int i = 0; i < Hand.Count; i++)
                        if (Hand[i].Suit != BestSuit && Hand[i].Suit != opponentSuit)
                            notBestSuits.Add(Hand[i]);

                    if (notBestSuits.Count > 0)
                    {
                        if (thisTurnNum <= lastTurnToGoWide && worstCard.Value <= highestLowValueCard)
                        {
                            if (notBestSuits[0].Value <= highestLowValueCard)
                                worstCard = notBestSuits[0];
                        }
                        else
                        {
                            worstCard = notBestSuits[0];
                        }
                    }
                }
                
                // Om dessa kriterier uppfylls tror vi det är bättre att spela "wide". Alltså att slänga bort lite poäng från den bästa färgen
                // för att ha en del i flera färger så att om vi drar höga kort från högen så är det större chans att vi redan har av den färgen.
                // Detta har prioritet över tidigare undantag.
                worstBestSuitCard = GetWorstInBestSuit();
                if (!lastTurn && worstCard.Value >= lowestScrapValueToGoWide && Game.Score(this) <= highestHandValueToGoWide && worstBestSuitCard.Value <= highestLowValueCard && thisTurnNum <= lastTurnToGoWide)
                {
                    worstCard = worstBestSuitCard;
                }
            }

            return worstCard;
        }

        public override void SpelSlut(bool wonTheGame) // Anropas när ett spel tar slut. Wongames++ får ej ändras!
        {
            if (wonTheGame)
            {
                Wongames++;
            }
        }

        private int CardValue(Card card) // Hjälpmetod som kan användas för att värdera hur bra ett kort är
        {
            return card.Value;
        }

        //Metod för att hitta det traditionelt sämsta kortet. Koden i KastaKort() är huvudsakligen undantag.
        Card GetWorstCard(List<Card> hand)
        {
            Card worstCard = null;
            for (int i = 0; i < hand.Count; i++)
            {
                //Vi hittar det sämsta kortet bland korten som inte finns i best suit genom att ta det första som inte tillhör BestSuit. Korten är sorterade från lägsta till högsta värde. 
                if (hand[i].Suit != BestSuit)
                {
                    worstCard = hand[i];
                    i = hand.Count;
                }
            }
            //Enda fallet då worstCard kan vara null är om alla korten tillhör BestSuit, alltså samma suit. Handen är redan sorterad från lägst till högst värde så om alla är samma färg tar vi det första i handen.
            if (worstCard == null)
                worstCard = hand[0];

            return worstCard;
        }

        //Metod som returnerar det sämsta kortet i BestSuit.
        Card GetWorstInBestSuit()
        {
            for (int i = 0; i < Hand.Count; i++)
            {
                //Eftersom handen är sorterad från sämsta till högsta värde så är det första kortet i handen som tillhör BestSuit det sämsta kortet i BestSuit.
                if (Hand[i].Suit == BestSuit)
                    return  Hand[i];
            }
            //Så att VisualStudio funkar. Kommer aldrig hända. "All codepaths must return a value"
            return null;
        }

        //Metod som returnerar det vi tror är motståndarens bästa suit.
        Suit GetOpponentEstSuit()
        {
            int[] suitCounter = new int[4];

            for (int i = 0; i < opponentEstHand.Count; i++)
            {
                for (int j = 0; j < suitCounter.Length; j++)
                {
                    if (opponentEstHand[i].Suit == (Suit)j)
                        suitCounter[j] += opponentEstHand[i].Value;
                }
            }

            int opponentHandValue = Game.HandScore(opponentEstHand, null);

            for (int i = 0; i < suitCounter.Length; i++)
            {
                if (suitCounter[i] == opponentHandValue)
                    return (Suit)suitCounter[i];
            }

            // Kommer aldrig hända. Bara så att koden kan köras.
            return (Suit)0;
        }

        // Benjamin/Måns
        // Metod som kollar om skräphögskortet förbättrar vår hands poäng.
        bool IsTheNewHandBetter(int preGameScore, Card scrapCard, Card newWorstCard)
        {
            if (Game.Score(this) > preGameScore)
            {
                Hand.Remove(scrapCard);
                Hand.Add(newWorstCard);
                return true;
            }
            else
            {
                Hand.Remove(scrapCard);
                Hand.Add(newWorstCard);
                return false;
            }
        }

        // Returnerar antalet ess vi har i vår hand.
        int GetNumOfAceInHand()
        {
            int aceCounter = 0;
            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].Value == 11)
                    ++aceCounter;
            }
            return aceCounter;
        }

        // Benjamin
        // Uppdaterar listan av de kort vi vet finns i motståndarens hand.
        void OpponentGuess(Card card)
        {
            // Vår gissning av motsåndarens hand måste uppdateras om de slänger 1 kort.
            for (int i = 0; i < opponentEstHand.Count; ++i)
            {
                if (opponentEstHand[i] == card)
                {
                    opponentEstHand.Remove(card);
                }
            }

            // Om de tar upp från skräphögen så kan vi lägga till ett kort i vår gissning av motståndares hand.
            if (OpponentsLatestCard != null)
            {
                opponentEstHand.Add(OpponentsLatestCard);
            }
        }

        // Benjamin
        // Sorterar hela din hand endast beroende på värdet på kortet, är användbart i många situationer. 
        void SortHand()
        {
            Hand = Hand.OrderBy(x => x.Value).ToList();
        }

        // Benjamin
        // Metod som returnerar ett värde som vår hand måste vara större än för att vi ska knacka. Efter mycket testning så är 23 det bästa för våra TaUppKort och KastaKort metoder.
        int WhenToKnock(int turn)
        {
            return 23;
        }

        // Lägg gärna till egna hjälpmetoder här
    }

    class Bot2Beat : Player
    {
        List<Card> opponentEstHand = new List<Card>();

        Card scrapPileCard;

        int
            aceCount,
            amountKnownCards,
            turn = 0,
            round;
        float
            averageCardValue = 7.3f,
            moreThan50startHandValue = 14.6f,
            averageGayValue = 13f,
            averageStartHandValue = 12.1f;
        bool opponentTakePile;

        public Bot2Beat()
        {
            Name = "Bot2Beat";
        }

        public override bool Knacka(int round) //Returnerar true om spelaren skall knacka, annars false. Runda 1 är round = 2.
        {
            this.round = round;
            amountKnownCards = opponentEstHand.Count;
            SortHand();

            int whenToKnock = WhenToKnock(turn);
            ++turn;

            if (Game.Score(this) >= whenToKnock)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool TaUppKort(Card card) // Returnerar true om spelaren skall ta upp korten på skräphögen (card), annars false för att dra kort från leken. Card i parametern är skräphögskortet.
        {
            // Benjamin 
            scrapPileCard = card;
            SortHand();

            OpponentGuess(card);

            // Räknar antal ess vi har i vår hand.
            aceCount = 0;
            for (int i = 0; i < Hand.Count; ++i)
            {
                if (Hand[i].Value == 11)
                {
                    ++aceCount;
                }
            }
            // Om vi har 2 ess redan så tar vi såklart upp ett tredje från skräphögen.
            if (aceCount >= 2 && card.Value == 11)
            {
                return true;
            }

            int preGameScore = Game.Score(this);

            // Plocka upp så vi har ett så stort värde som möjligt, eller om skräphögskortet skulle resultera i en ny bestsuit.
            if (lastTurn)
            {
                Hand.Add(card);
                SortHand();

                if (Game.Score(this) > preGameScore)
                {
                    Hand.Remove(card);
                    for (int i = 0; i < Hand.Count; ++i)
                    {
                        if (card.Value > Hand[i].Value)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                if (Game.Score(this) <= preGameScore)
                {
                    Hand.Remove(card);
                    return false;
                }
            }
            if (!lastTurn)
            {
                Hand.Add(card);
                SortHand();

                // Om skräphögskortet resulterar i en bättre poäng.
                if (Game.Score(this) > preGameScore)
                {
                    Hand.Remove(card);
                    return true;
                }
                // Om skräphögskortet inte resulterar i en bättre poäng
                if (Game.Score(this) <= preGameScore)
                {
                    Hand.Remove(card);
                    return false;
                }
            }

            return false; //Temp return
        }

        public override Card KastaKort()  // Returnerar det kort som skall kastas av de fyra som finns på handen. Game.Score(this) returnerar värdet av bestSuit bland alla 4 kort.
        {
            SortHand();
            Game.Score(this);

            Card worstCard = null;

            int bestSuitScore = 0;

            for (int i = 0; i < Hand.Count; ++i)
            {
                if (Hand[i].Suit != BestSuit)
                {
                    if (Game.HandScore(Hand, Hand[i]) > bestSuitScore)
                    {
                        bestSuitScore = Game.HandScore(Hand, Hand[i]);
                        if (worstCard == null || Hand[i].Value < worstCard.Value)
                        {
                            worstCard = Hand[i];
                        }
                    }
                }
            }
            if (worstCard == null || aceCount == 3)
            {
                worstCard = Hand[0];
            }

            return worstCard;
        }

        public override void SpelSlut(bool wonTheGame) // Anropas när ett spel tar slut. Wongames++ får ej ändras!
        {
            if (wonTheGame)
            {
                Wongames++;
            }

        }

        private int CardValue(Card card) // Hjälpmetod som kan användas för att värdera hur bra ett kort är
        {
            return card.Value;
        }

        void OpponentGuess(Card card)
        {
            // Vår gissning av motsåndarens hand måste uppdateras om de slänger 1 kort.
            for (int i = 0; i < opponentEstHand.Count; ++i)
            {
                if (opponentEstHand[i] == card)
                {
                    opponentEstHand.Remove(card);
                }
            }

            // Om de tar upp från skräphögen så kan vi lägga till ett kort i vår gissning av motståndares hand.
            if (OpponentsLatestCard != null)
            {
                opponentTakePile = false;
                opponentEstHand.Add(OpponentsLatestCard);
            }
            // Om de tar upp från den vanliga högen så gillar de förmodligen inte de kortet.
            if (OpponentsLatestCard == null)
            {
                opponentTakePile = true;
            }
        }

        // Sorterar din hand endast beroende på värdet i handen.
        void SortHand()
        {
            Hand = Hand.OrderBy(x => x.Value).ToList();
        }

        int WhenToKnock(int turn)
        {
            return 23;
        }
    }

    class BasicPlayer : Player //Denna spelare fungerar exakt som MyPlayer. Ändra gärna i denna för att göra tester.
    {

        public BasicPlayer()
        {
            Name = "Basic Player";
        }

        public override bool Knacka(int round)
        {
            if (Game.Score(this) >= 30)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool TaUppKort(Card card)
        {
            if (card.Value == 11 || (card.Value == 10 && card.Suit == BestSuit))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public override Card KastaKort()
        {
            Game.Score(this);
            Card worstCard = Hand.First();
            for (int i = 1; i < Hand.Count; i++)
            {
                if (Hand[i].Value < worstCard.Value)
                {
                    worstCard = Hand[i];
                }
            }
            return worstCard;

        }

        public override void SpelSlut(bool wonTheGame)
        {
            if (wonTheGame)
            {
                Wongames++;
            }

        }
    }

    class ICommitDie : Player
    {
        public ICommitDie()
        {
            Name = "BalasBot";
        }

        public override bool Knacka(int round) // Returnerar true om spelaren skall knacka, annars false.
        {
            // Knackar om jag har 22 eller mer poäng.
            int knock = 22;

            if (Game.Score(this) >= knock)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool TaUppKort(Card card) // Ropar på en metod som avgör om man ska plocka upp ett kort eller ej.
        {
            // Anvnder en metod som avgör vilka kort som e bra.
            if (PickUpTrash(card))
            {
                return true;
            }
            if (!PickUpTrash(card))
            {
                return false;
            }
            return false;
        }

        public override Card KastaKort()  // Ropar på en metod som avgör vilket kort som ska kastas.
        {
            // Sämsta kortet i handen bestäms av metod.
            return ThrowCard();
        }

        public override void SpelSlut(bool wonTheGame) // Anropas när ett spel tar slut. Wongames++ får ej ändras!
        {
            if (wonTheGame)
            {
                Wongames++;
            }
        }

        private int CardValue(Card card) // Hjälpmetod som kan användas för att värdera hur bra ett kort är.
        {
            return card.Value;
        }

        private bool PickUpTrash(Card card) // Returnerar true om det är värt att plocka upp kortet från högen annars false.
        {
            // Kollar om kortet i skräphögen gör min hand bättre annars plockar jag ej upp det.
            int oldScore = Game.Score(this);
            Hand.Add(card);
            int newScore = Game.Score(this);
            Hand.RemoveAt(Hand.Count - 1);
            if (oldScore < newScore)
            {
                if (card.Value <= 5)
                {
                    return false;
                }
                return true;
            }

            // Om jag har 2 ess på handen och det finns ett ess i högen plockar jag de esset
            List<Card> aces = new List<Card>();
            for (int i = 0; i < 3; i++)
            {
                if (Hand[i].Value == 11)
                {
                    aces.Add(Hand[i]);
                }
            }
            if (aces.Count >= 2 && card.Value == 11)
            {
                return true;
            }

            return false;

        }

        private Card ThrowCard() // Returnerar det kort som är bäst att kasta.
        {
            // Behåller det korten som ger mig mest poäng.
            int[] tempScore = new int[4];
            int score = 0;
            Card badCard = null;
            List<int> badI = new List<int>();
            List<Card> testHand = new List<Card>();

            // Sparar poängen jag har om jag tar bort ett av korten från handen. DVS alla 4 olika poäng jag kan ha.
            for (int i = 0; i < tempScore.Length; i++)
            {
                tempScore[i] = Game.HandScore(Hand, Hand[i]);
            }

            // Kollar vilket kort jag ska ta bort för mest poäng.
            for (int i = 0; i < tempScore.Length; i++)
            {
                if (tempScore[i] >= score)
                {
                    score = tempScore[i];
                    badI.Add(i);

                    testHand.Add(Hand[i]);
                }
            }
            badCard = Hand[badI.Last()];

            // Om två kort ger mig lika mycket poäng när dem tas bort tar jag bort det med minst värde av dem.
            if (badI.Count == 2)
            {
                if (Hand[badI[0]].Suit == BestSuit && Hand[badI[1]].Suit == BestSuit)
                {
                    if (Hand[badI[0]].Value > Hand[badI[1]].Value)
                    {
                        badCard = Hand[badI[1]];
                    }
                    if (Hand[badI[0]].Value < Hand[badI[1]].Value)
                    {
                        badCard = Hand[badI[0]];
                    }
                }
            }

            // Om två kort ger mitt score lika mkt slänger jag kortet som e minst av dem.
            if (testHand.Count >= 3)
            {
                for (int i = 0; i < testHand.Count; i++)
                {
                    if (testHand[i].Value < badCard.Value && testHand[i].Suit != BestSuit)
                    {
                        badCard = testHand[i];
                    }
                }
            }

            #region Om jag har ess

            // Om jag har 2 eller fler ess på handen slänger jag det sämsta av de övriga korten.
            List<Card> aces = new List<Card>();
            List<Card> notAces = new List<Card>();


            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].Value == 11)
                {
                    aces.Add(Hand[i]);
                }
            }
            if (aces.Count >= 2)
            {
                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand[i].Value != 11)
                    {
                        notAces.Add(Hand[i]);
                    }
                }
            }
            if (notAces.Count == 2)
            {
                if (notAces[0].Value > notAces[1].Value)
                {
                    badCard = notAces[1];
                }
                if (notAces[0].Value < notAces[1].Value)
                {
                    badCard = notAces[0];
                }
            }

            #endregion

            return badCard;
        }
    }

    class ICommitDie2 : Player
    {
        public ICommitDie2()
        {
            Name = "BalasBot2";
        }

        public override bool Knacka(int round) // Returnerar true om spelaren skall knacka, annars false.
        {
            // Knackar om jag har 23 eller mer poäng.
            int knock = 22;

            if (Game.Score(this) >= knock)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool TaUppKort(Card card) // Ropar på en metod som avgör om man ska plocka upp ett kort eller ej.
        {
            // Anvnder en metod som avgör vilka kort som e bra.
            if (PickUpTrash(card))
            {
                return true;
            }
            if (!PickUpTrash(card))
            {
                return false;
            }
            return false;
        }

        public override Card KastaKort()  // Ropar på en metod som avgör vilket kort som ska kastas.
        {
            // Sämsta kortet i handen bestäms av metod.
            return ThrowCard();
        }

        public override void SpelSlut(bool wonTheGame) // Anropas när ett spel tar slut. Wongames++ får ej ändras!
        {
            if (wonTheGame)
            {
                Wongames++;
            }
        }

        private int CardValue(Card card) // Hjälpmetod som kan användas för att värdera hur bra ett kort är.
        {
            return card.Value;
        }

        private bool PickUpTrash(Card card) // Returnerar true om det är värt att plocka upp kortet från högen annars false.
        {
            // Kollar om kortet i skräphögen gör min hand bättre annars plockar jag ej upp det.
            int oldScore = Game.Score(this);
            Hand.Add(card);
            int newScore = Game.Score(this);
            Hand.RemoveAt(Hand.Count - 1);
            if (oldScore < newScore)
            {
                if (card.Value <= 5)
                {
                    return false;
                }
                return true;
            }

            // Om jag har 2 ess på handen och det finns ett ess i högen plockar jag de esset
            List<Card> aces = new List<Card>();
            for (int i = 0; i < 3; i++)
            {
                if (Hand[i].Value == 11)
                {
                    aces.Add(Hand[i]);
                }
            }
            if (aces.Count >= 2 && card.Value == 11)
            {
                return true;
            }

            return false;

        }

        private Card ThrowCard() // Returnerar det kort som är bäst att kasta.
        {
            // Behåller det korten som ger mig mest poäng.
            int[] tempScore = new int[4];
            int score = 0;
            Card badCard = null;
            List<int> badI = new List<int>();
            List<Card> testHand = new List<Card>();

            // Sparar poängen jag har om jag tar bort ett av korten från handen. DVS alla 4 olika poäng jag kan ha.
            for (int i = 0; i < tempScore.Length; i++)
            {
                tempScore[i] = Game.HandScore(Hand, Hand[i]);
            }

            // Kollar vilket kort jag ska ta bort för mest poäng.
            for (int i = 0; i < tempScore.Length; i++)
            {
                if (tempScore[i] >= score)
                {
                    score = tempScore[i];
                    badI.Add(i);

                    testHand.Add(Hand[i]);
                }
            }
            badCard = Hand[badI.Last()];

            // Om två kort ger mig lika mycket poäng när dem tas bort tar jag bort det med minst värde av dem.
            if (badI.Count == 2)
            {
                if (Hand[badI[0]].Suit == BestSuit && Hand[badI[1]].Suit == BestSuit)
                {
                    if (Hand[badI[0]].Value > Hand[badI[1]].Value)
                    {
                        badCard = Hand[badI[1]];
                    }
                }
            }

            // Om två kort ger mitt score lika mkt slänger jag kortet som e minst av dem.
            if (testHand.Count >= 3)
            {
                for (int i = 0; i < testHand.Count; i++)
                {
                    if (testHand[i].Value < badCard.Value && testHand[i].Suit != BestSuit)
                    {
                        badCard = testHand[i];
                    }
                }
            }

            #region Om jag har ess

            // Om jag har 2 eller fler ess på handen slänger jag det sämsta av de övriga korten.
            List<Card> aces = new List<Card>();
            List<Card> notAces = new List<Card>();


            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].Value == 11)
                {
                    aces.Add(Hand[i]);
                }
            }
            if (aces.Count >= 2)
            {
                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand[i].Value != 11)
                    {
                        notAces.Add(Hand[i]);
                    }
                }
            }
            if (notAces.Count == 2)
            {
                if (notAces[0].Value > notAces[1].Value)
                {
                    badCard = notAces[1];
                }
            }

            #endregion

            return badCard;
        }
    }

    class MathBot1 : Player
    {
        List<int> totalHandValues = new List<int>();
        List<float> averageHandValues = new List<float>();
        List<int> roundOccurence = new List<int>();

        Card scrapPileCard;

        int
            aceCount,
            games,
            turn = 0,
            fewestTurns = 0;
        float
            averageCardValue = 7.3f,
            moreThan50startHandValue = 14.6f,
            averageGayValue = 13f,
            averageStartHandValue = 12.1f;
        bool opponentTakePile;

        public MathBot1()
        {
            Name = "MathBot1";
        }

        public override bool Knacka(int round) //Returnerar true om spelaren skall knacka, annars false. Runda 1 är round = 2. Formatet för GeoGebra är (x, y), (x, y), 
        {
            int handValue = Game.Score(this);

            if (totalHandValues.Count <= turn)
            {
                totalHandValues.Add(0);
                roundOccurence.Add(0);
            }
            roundOccurence[turn]++;
            totalHandValues[turn] += handValue;

            ++turn;
            return false;
        }

        public override bool TaUppKort(Card card) // Returnerar true om spelaren skall ta upp korten på skräphögen (card), annars false för att dra kort från leken. Card i parametern är skräphögskortet.
        {
            // Benjamin 
            scrapPileCard = card;
            SortHand();

            // Räknar antal ess vi har i vår hand.
            aceCount = 0;
            for (int i = 0; i < Hand.Count; ++i)
            {
                if (Hand[i].Value == 11)
                {
                    ++aceCount;
                }
            }
            // Om vi har 2 ess redan så tar vi såklart upp ett tredje från skräphögen.
            if (aceCount >= 2 && card.Value == 11)
            {
                return true;
            }

            int preGameScore = Game.Score(this);

            // Plocka upp så vi har ett så stort värde som möjligt, om skräphögskortet skulle resultera i en ny bestsuit.
            if (lastTurn)
            {
                Hand.Add(card);
                SortHand();

                if (Game.Score(this) > preGameScore)
                {
                    Hand.Remove(card);
                    return true;
                }
                if (Game.Score(this) <= preGameScore)
                {
                    Hand.Remove(card);
                    return false;
                }
            }
            if (!lastTurn)
            {
                Hand.Add(card);
                SortHand();

                // Om skräphögskortet resulterar i en bättre poäng.
                if (Game.Score(this) > preGameScore)
                {
                    Hand.Remove(card);
                    return true;
                }
                // Om skräphögskortet inte resulterar i en bättre poäng
                if (Game.Score(this) <= preGameScore)
                {
                    Hand.Remove(card);
                    return false;
                }
            }

            return false; //Temp return
        }

        public override Card KastaKort()  // Returnerar det kort som skall kastas av de fyra som finns på handen. Game.Score(this) returnerar värdet av bestSuit bland alla 4 kort.
        {
            SortHand();
            Game.Score(this);

            Card worstCard = null;

            int bestSuitScore = 0;

            for (int i = 0; i < Hand.Count; ++i)
            {
                if (Hand[i].Suit != BestSuit)
                {
                    if (Game.HandScore(Hand, Hand[i]) > bestSuitScore)
                    {
                        bestSuitScore = Game.HandScore(Hand, Hand[i]);
                        if (worstCard == null || Hand[i].Value < worstCard.Value)
                        {
                            worstCard = Hand[i];
                        }
                    }
                }
            }
            if (worstCard == null || aceCount == 3)
            {
                worstCard = Hand[0];
            }

            return worstCard;
        }

        public override void SpelSlut(bool wonTheGame) // Anropas när ett spel tar slut. Wongames++ får ej ändras!
        {
            ++games;
            turn = 0;

            if (wonTheGame)
            {
                Wongames++;
            }

            if (games >= 9999)
            {
                string path = Environment.CurrentDirectory;
                Directory.CreateDirectory(path + "/File Saving Folder");
                FileWrite fileWrite = new FileWrite();

                for (int i = 0; i < totalHandValues.Count; ++i)
                {
                    averageHandValues.Add(0);
                    decimal curAverage = totalHandValues[i] / roundOccurence[i];
                    averageHandValues[i] = (float)Math.Round(curAverage, 3);
                }

                fileWrite.Write(@path, averageHandValues);
            }
        }

        private int CardValue(Card card) // Hjälpmetod som kan användas för att värdera hur bra ett kort är
        {
            return card.Value;
        }

        // Sorterar din hand endast beroende på värdet i handen.
        void SortHand()
        {
            Hand = Hand.OrderBy(x => x.Value).ToList();
        }
    }

    class MathBot2 : Player
    {
        List<Card> opponentEstHand = new List<Card>();
        List<Card> opponentDontLike = new List<Card>();
        List<Card> discardPile = new List<Card>();

        Card scrapPileCard;

        int
            aceCount,
            amountKnownCards,
            round;
        float
            averageCardValue = 7.3f,
            moreThan50startHandValue = 14.6f,
            averageGayValue = 13f,
            averageStartHandValue = 12.1f;
        bool opponentTakePile;

        public MathBot2()
        {
            Name = "MathBot2";
        }

        public override bool Knacka(int round) //Returnerar true om spelaren skall knacka, annars false. Runda 1 är round = 2.
        {
            return false;
        }

        public override bool TaUppKort(Card card) // Returnerar true om spelaren skall ta upp korten på skräphögen (card), annars false för att dra kort från leken. Card i parametern är skräphögskortet.
        {
            // Benjamin 
            scrapPileCard = card;
            SortHand();

            // Räknar antal ess vi har i vår hand.
            aceCount = 0;
            for (int i = 0; i < Hand.Count; ++i)
            {
                if (Hand[i].Value == 11)
                {
                    ++aceCount;
                }
            }
            // Om vi har 2 ess redan så tar vi såklart upp ett tredje från skräphögen.
            if (aceCount >= 2 && card.Value == 11)
            {
                return true;
            }

            int preGameScore = Game.Score(this);

            // Plocka upp så vi har ett så stort värde som möjligt, om skräphögskortet skulle resultera i en ny bestsuit.
            if (lastTurn)
            {
                Hand.Add(card);
                SortHand();

                if (Game.Score(this) > preGameScore)
                {
                    Hand.Remove(card);
                    return true;
                }
                if (Game.Score(this) <= preGameScore)
                {
                    Hand.Remove(card);
                    return false;
                }
            }
            if (!lastTurn)
            {
                Hand.Add(card);
                SortHand();

                // Om skräphögskortet resulterar i en bättre poäng.
                if (Game.Score(this) > preGameScore)
                {
                    Hand.Remove(card);
                    return true;
                }
                // Om skräphögskortet inte resulterar i en bättre poäng
                if (Game.Score(this) <= preGameScore)
                {
                    Hand.Remove(card);
                    return false;
                }

                discardPile.Add(card);
            }

            return false; //Temp return
        }

        public override Card KastaKort()  // Returnerar det kort som skall kastas av de fyra som finns på handen. Game.Score(this) returnerar värdet av bestSuit bland alla 4 kort.
        {
            SortHand();
            Game.Score(this);

            Card worstCard = null;

            int bestSuitScore = 0;

            for (int i = 0; i < Hand.Count; ++i)
            {
                if (Hand[i].Suit != BestSuit)
                {
                    if (Game.HandScore(Hand, Hand[i]) > bestSuitScore)
                    {
                        bestSuitScore = Game.HandScore(Hand, Hand[i]);
                        if (worstCard == null || Hand[i].Value < worstCard.Value)
                        {
                            worstCard = Hand[i];
                        }
                    }
                }
            }
            if (worstCard == null || aceCount == 3)
            {
                worstCard = Hand[0];
            }

            return worstCard;
        }

        public override void SpelSlut(bool wonTheGame) // Anropas när ett spel tar slut. Wongames++ får ej ändras!
        {
            if (wonTheGame)
            {
                Wongames++;
            }

        }

        private int CardValue(Card card) // Hjälpmetod som kan användas för att värdera hur bra ett kort är
        {
            return card.Value;
        }

        // Sorterar din hand endast beroende på värdet i handen.
        void SortHand()
        {
            Hand = Hand.OrderBy(x => x.Value).ToList();
        }

        int HandScore()
        {
            int handScore = 0;
            for (int i = 0; i < Hand.Count; ++i)
            {
                handScore += Hand[i].Value;
            }
            return handScore;
        }

        // Lägg gärna till egna hjälpmetoder här
    }

    class FileWrite
    {
        public void Write(string path, List<float> averageHandValues)
        {
            string printData = "";
            for (int i = 0; i < averageHandValues.Count; ++i)
            {
                printData += "(" + i + ", " + averageHandValues[i] + "), ";
            }
            File.WriteAllText(path + "/File Saving Folder/MyOwnData.txt", printData);
        }
    }
}
