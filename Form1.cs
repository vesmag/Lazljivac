using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Projekt251
{
    public partial class MainWindow : Form
    {
        private static System.Timers.Timer aTimer;

        static int numberOfCards = 5; //broj karata u rukama igrača na početku
        int playerTotal = numberOfCards; //ažurni broj karata u rukama igrača i računala: na početku je numberOfCards
        int computerTotal = numberOfCards;
        int turn = 1; //1 za igrača, 0 za komp

        Card[] Cards = new Card[52]; //ovo koristim zapravo samo dok generiram karte - nisam mogla shuffleati stog
        Card[] players = new Card[52]; //karte u rukama igrača
        Card[] computers = new Card[52]; //karte u rukama računala

        Stack<Card> shuffledDeck = new Stack<Card>(); //deck iz kojeg se izvlači
        Stack<Card> thrown = new Stack<Card>(); //bačene karte


        //grafičke varijable
        Button Deal = new Button();

        PictureBox Diamonds = new PictureBox();
        PictureBox Spades = new PictureBox();
        PictureBox Hearts = new PictureBox();
        PictureBox Clubs = new PictureBox();

        PictureBox lastCardonDeck = new PictureBox();
        //što su zadnje rekli da su bacili - 0 je clubs, 1 je diamonds, 2 je hearts, 3 je spades
        int playerSuit;
        int computerSuit;

        Label Lazes = new Label();
        Button Da = new Button();
        Button Ne = new Button();

        //pomoć za async
        int k = -1;
        int k2 = -1;

        public MainWindow()
        {
            InitializeComponent();

            SetTimer();
        }

        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(360000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, EventArgs e)
        {
            MessageBox.Show("Istekle su 3 minute. Računalo je pobijedilo!");
        }

        //generira se deck, miješaju karte i smiještaju u stack
        private void onLoad(object sender, EventArgs e)
        {
            BackgroundImage = new Bitmap(Properties.Resources.poker_table_background);

            //postavljanje gumba deal
            Deal.Text = "DEAL";
            Deal.Location = new Point(73, 410);
            Deal.Click += deal;
            Deal.Tag = "main";
            Deal.Visible = false;
            Controls.Add(Deal);

            // dodajemo gumbe za igrače kada govore je li računalo lagalo ili nije
            Lazes.Text = "LAZE?";
            Lazes.Size = new Size(150, 50);
            Lazes.Location = new Point(870, 252);
            Lazes.TextAlign = ContentAlignment.MiddleCenter;
            Lazes.Tag = "main";
            Lazes.Visible = false;
            Controls.Add(Lazes);
            Da.Text = "DA";
            Da.Size = new Size(70, 50);
            Da.Location = new Point(870, 305);
            Da.Click += turnLast;
            Da.Tag = "main";
            Da.Visible = false;
            Controls.Add(Da);
            Ne.Text = "NE";
            Ne.Size = new Size(70, 50);
            Ne.Location = new Point(950, 305);
            Ne.Click += backToPlayer;
            Ne.Tag = "main";
            Ne.Visible = false;
            Controls.Add(Ne);

            //dodajemo gumbe za igrača da kaže koju je kartu bacio: 
            //na svaku dodajemo click event koji zove playerSaid fju
            Diamonds.Name = "Diamonds";
            Diamonds.Image = Properties.Resources.Diamonds;
            Diamonds.SizeMode = PictureBoxSizeMode.Zoom;
            Diamonds.Size = new Size(130, 45);
            Diamonds.Location = new Point(1000, 435);
            Diamonds.Cursor = Cursors.Hand;
            Diamonds.Tag = "main";
            Diamonds.Visible = false;
            Controls.Add(Diamonds);
            Spades.Name = "Spades";
            Spades.Image = Properties.Resources.Spades;
            Spades.SizeMode = PictureBoxSizeMode.Zoom;
            Spades.Size = new Size(130, 45);
            Spades.Location = new Point(1000, 485);
            Spades.Cursor = Cursors.Hand;
            Spades.Tag = "main";
            Spades.Visible = false;
            Controls.Add(Spades);
            Hearts.Name = "Hearts";
            Hearts.Image = Properties.Resources.Hearts;
            Hearts.SizeMode = PictureBoxSizeMode.Zoom;
            Hearts.Size = new Size(130, 45);
            Hearts.Location = new Point(1000, 535);
            Hearts.Cursor = Cursors.Hand;
            Hearts.Tag = "main";
            Hearts.Visible = false;
            Controls.Add(Hearts);
            Clubs.Name = "Clubs";
            Clubs.Image = Properties.Resources.Clubs;
            Clubs.SizeMode = PictureBoxSizeMode.Zoom;
            Clubs.Size = new Size(130, 45);
            Clubs.Location = new Point(1000, 585);
            Clubs.Cursor = Cursors.Hand;
            Clubs.Tag = "main";
            Clubs.Visible = false;
            Controls.Add(Clubs);
           
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newGame_Click(object sender, EventArgs e)
        {
            // fileStrip, buttoni za Lazes i picBoxovi za znakove
            if (this.Controls.Count > 9)
            {
                int c = Controls.Count;
                for (int i = c - 1; i >= 0; --i)
                {
                    if (Controls[i].Tag == null || (Controls[i] != null && Controls[i].Tag.ToString() != "main"))
                        Controls.Remove(Controls[i]);
                }

                //  treba izbaciti ove kontrole jer se u playersTurn-u opet dodaju 
                Point l = new Point(60, 240);
                for (int x = 51; x >= 0; x--)
                {
                    if (players[x] != null)
                    {
                        players[x].picBox.Click -= throwCard;
                        flipBack(players[x]);
                        players[x].picBox.Location = l;
                    }
                    if (computers[x] != null)
                    {
                        computers[x].picBox.Click -= throwCard;
                        flipBack(computers[x]);
                        computers[x].picBox.Location = l;
                    }
                }

                Diamonds.Visible = false;
                Clubs.Visible = false;
                Hearts.Visible = false;
                Spades.Visible = false;

                while (thrown.Any())
                {
                    Card temp = thrown.Pop();
                    temp.picBox.Location = l;
                }

                players = new Card[52];
                computers = new Card[52];

                this.Refresh();

                shuffledDeck.Clear();
                thrown.Clear();

                playerTotal = numberOfCards;
                computerTotal = numberOfCards;

                //funkcije za početak igre
                Shuffle(Cards); //miješa karte (moraju biti u nizu da bismo ih mogli promiješati)
                storeInStack(Cards); //sprema karte u Stack jer nam je to puno bolja struktura
                Deal.Visible = true;
                Lazes.Visible = false;
                Da.Visible = false;
                Ne.Visible = false;     
            }
            else
            {
                //funkcije za početak igre
                generateDeck(); //stvara niz Cards[]
                Shuffle(Cards); //miješa karte (moraju biti u nizu da bismo ih mogli promiješati)
                storeInStack(Cards); //sprema karte u Stack jer nam je to puno bolja struktura
                Deal.Visible = true;
            }         
        }

        //stvara 52 karte i smješta ih u Array Cards
        private void generateDeck()
        {
            int ct = 0, cs = 0; //cardType, cardSuit
            for (int i = 0; i < 52; ++i)
            {
                PictureBox picBox = new PictureBox();
                ct = i / 4;
                cs = i % 4;
                string imName = "S" + cs + "T" + ct;
                Image im = (Image)Properties.Resources.ResourceManager.GetObject(imName);
                Point l = new Point(60, 240);
                picBox.Image = Properties.Resources.deck;
                picBox.SizeMode = PictureBoxSizeMode.Zoom;
                picBox.Size = new Size(100, 150);
                picBox.Location = l;
                Card C = new Card(picBox, im, ct, cs, l); //za svaku kartu spremili smo njen picBox, sliku, type, suit i lokaciju ----> lokacija moguće neće trebati
                Cards[i] = C;
            }
        }

        private void Shuffle(Card[] deck)
        {
            Random r = new Random();
            for (int n = deck.Length - 1; n > 0; --n)
            {
                int l = r.Next(n + 1);
                Card temp = deck[n];
                deck[n] = deck[l];
                deck[l] = temp;
            }
        }

        //premiještanje iz Cards u Stack shuffledDeck
        private void storeInStack(Card[] deck)
        {
            for (int i = 0; i < deck.Length; ++i)
            {
                Controls.Add(deck[i].picBox);
                shuffledDeck.Push(deck[i]);
            }
        }


        //na pritisak gumba Deal, karte se podijele
        private void deal(object sender, EventArgs e)
        {
            Deal.Visible = false;   //Deal gumb nam više nije potreban
            for (int i = 0; i < numberOfCards; ++i)
            {
                players[i] = shuffledDeck.Pop();
                players[i].picBox.Location = new Point(250 + 130 * i, 435);
                flipFront(ref players[i]); //playerove karte se okreću na pravu stranu
                Controls.Add(players[i].picBox);
                computers[i] = shuffledDeck.Pop();
                computers[i].picBox.Location = new Point(250 + 130 * i, 40);
                Controls.Add(computers[i].picBox);
            }

            Diamonds.Visible = true;
            Clubs.Visible = true;
            Hearts.Visible = true;
            Spades.Visible = true; 
            playersTurn(); //prvi na redu je uvijek igrač
        }

        //jednostavne funkcije koje okreću karte
        private void flipFront(ref Card C)
        {
            C.picBox.Image = C.im;
        }

        private void flipBack(Card C)
        {
            C.picBox.Image = Properties.Resources.deck;
        }

        //dajemo svakoj karti onClick event            
        private void playersTurn()
        {
            for (int i = 0; i < playerTotal; ++i)
            {
                if (players[i] != null)
                {
                    players[i].picBox.Cursor = Cursors.Hand;
                    players[i].picBox.Click += throwCard;
                }
            }
            turn = 1;
        }

        private void throwCard(object sender, EventArgs e)
        {
            if (turn == 1)
            {
                PictureBox temp = (PictureBox)sender; //dolazimo do PictureBoxa koji je pozvao funkciju

                //ovdje gledamo kojoj karti iz players[] odgovara karta koja je pozvala funkciju - nju izbacujemo iz players[],
                //dodajemo u thrown stack i updateamo poravnanje karata igrača
                for (int i = 0; i < playerTotal; ++i)
                {
                    if (players[i].im == temp.Image)
                    {
                        thrown.Push(players[i]);
                        for (int j = i; j < playerTotal - 1; ++j)
                            players[j] = players[j + 1];
                        --playerTotal;
                        break;
                    }
                }
                updatePlayer();

                if (playerTotal == 0) MessageBox.Show("Winner: player!");
                else
                {
                    //bačenu kartu mičemo na thrown lokaciju
                    temp.Image = Properties.Resources.deck;         
                    temp.Location = new Point(431, 240);
                    temp.Click -= throwCard;
                    temp.Cursor = Cursors.Arrow;

                    //nakon jednog klika na kartu, trebamo disabelati novi klik do sljedećeg igračevog reda
                    for (int i = 0; i < playerTotal; ++i)
                    {
                        if (players[i] != null)
                        {
                            players[i].picBox.Cursor = Cursors.Default;
                            players[i].picBox.Click -= throwCard;
                        }
                    }
                    Diamonds.Click += playerSaid;
                    Diamonds.Cursor = Cursors.Hand;
                    Clubs.Click += playerSaid;
                    Clubs.Cursor = Cursors.Hand;
                    Hearts.Click += playerSaid;
                    Hearts.Cursor = Cursors.Hand;
                    Spades.Click += playerSaid;
                    Spades.Cursor = Cursors.Hand;

                }

            }
        }

        //u ovisnosti o bačenoj karti, pokazuje se text bubble koji sadržava sliku karte koju igrač kaže da je bacio
        //bubble postaje nevidljiv nakon 1 sekunde
        async private void playerSaid(object sender, EventArgs e)
        {
            if (turn == 1)
            {
                String suit = ((PictureBox)sender).Name;
                if (suit == "Diamonds") playerSuit = 1;
                if (suit == "Spades") playerSuit = 3;
                if (suit == "Hearts") playerSuit = 2;
                if (suit == "Clubs") playerSuit = 0;
                String imName = "player" + suit;
                Image im = (Image)Properties.Resources.ResourceManager.GetObject(imName);
                PictureBox playersTextBubble = new PictureBox();
                playersTextBubble.Image = im;
                playersTextBubble.Size = new Size(150, 150);
                playersTextBubble.SizeMode = PictureBoxSizeMode.Zoom;
                playersTextBubble.Location = new Point(870, 250);
                playersTextBubble.BackColor = Color.Transparent;
                Controls.Add(playersTextBubble);
                await Task.Delay(1000);
                playersTextBubble.Visible = false;  

                //nakon što je igrač rekao koju kartu je bacio, mičemo mu mogućnost da stišće gumb dok je komp na redu
                Diamonds.Click -= playerSaid;
                Diamonds.Cursor = Cursors.Arrow;
                Clubs.Click -= playerSaid;
                Clubs.Cursor = Cursors.Arrow;
                Hearts.Click -= playerSaid;
                Hearts.Cursor = Cursors.Arrow;
                Spades.Click -= playerSaid;
                Spades.Cursor = Cursors.Arrow;
            }
            turn = 0;
            computersTurn();
        }

        //isto malo kompl
        async private void computersTurn()
        {
            if (turn == 0)
            {
                Random r = new Random();
               if (k == -1)
                {
                    // random odlucuje laze li igrac
                    int lazemoli = r.Next(0, 2);

                    if (lazemoli == 1)
                    {
                        //kaže da smo lagali
                        PictureBox weLie = new PictureBox();
                        weLie.Image = Properties.Resources.lazes;
                        weLie.Size = new Size(150, 150);
                        weLie.SizeMode = PictureBoxSizeMode.Zoom;
                        weLie.Location = new Point(700, 250);
                        weLie.BackColor = Color.Transparent;
                        Controls.Add(weLie);
                        await Task.Delay(1000);
                        weLie.Visible = false;

                        Card temp = thrown.Pop();        //zadnju koju smo bacili
                        //prikažemo koju smo bacili
                        lastCardonDeck.Image = temp.im;
                        lastCardonDeck.Size = new Size(100, 165);
                        lastCardonDeck.SizeMode = PictureBoxSizeMode.Zoom;
                        lastCardonDeck.Location = new Point(585, 240);
                        lastCardonDeck.Visible = true;
                        Controls.Add(lastCardonDeck);
                        await Task.Delay(1000);

                        if (playerSuit == temp.cSuit)
                        {     
                            //nismo lagali, kompjuteru idu sve karte iz throwna
                            lastCardonDeck.Visible = false;
                            computers[computerTotal] = temp;
                            computerTotal++;
                            while (thrown.Any())
                            {
                                computers[computerTotal] = thrown.Pop();
                                computerTotal++;
                            }
                            updateComputer();       
                            updatePlayer();
                        }
                        else
                        {   
                            //jesmo lagali, idu sve karte nama
                            lastCardonDeck.Visible = false;
                            players[playerTotal] = temp;
                            flipFront(ref players[playerTotal]);
                            playerTotal++;
                            while (thrown.Any())
                            {
                                players[playerTotal] = thrown.Pop();
                                flipFront(ref players[playerTotal]);
                                playerTotal++;
                            }
                            updatePlayer();
                            updateComputer();
                        }
                    }

                }
                if (computerTotal == 0) MessageBox.Show("Winner: computer!");
                else
                {
                    //baca kartu

                    k = r.Next(0, computerTotal);
                    thrown.Push(computers[k]);
                    computers[k].picBox.Image = Properties.Resources.deck;
                    computers[k].picBox.Location = new Point(431, 240);
                    computers[k].picBox.Click -= throwCard;     // mičemo bačenu

                    Card temp = computers[k];

                    for (int i = k; i < computerTotal - 1; ++i)
                    {
                        computers[i] = computers[i + 1];
                    }
                    --computerTotal;

                    k = -1;
                    updateComputer();       //on dalje baca random kartu i govori koju je bacio
                    Random r2 = new Random();       //kompjuter kaže random koju je kartu bacio
                    if (k2 == -1) k2 = r2.Next(0, 3);
                    string suit = "";
                    if (k2 == 1) suit = "Diamonds";
                    if (k2 == 0) suit = "Clubs";
                    if (k2 == 2) suit = "Hearts";
                    if (k2 == 3) suit = "Spades";
                    computerSuit = k2;
                    String imName = "computer" + suit;
                    Image im = (Image)Properties.Resources.ResourceManager.GetObject(imName);
                    PictureBox computersTextBubble = new PictureBox();
                    computersTextBubble.Image = im;
                    computersTextBubble.Size = new Size(150, 150);
                    computersTextBubble.SizeMode = PictureBoxSizeMode.Zoom;
                    computersTextBubble.Location = new Point(1000, 150);
                    computersTextBubble.BackColor = Color.Transparent;
                    Controls.Add(computersTextBubble);
                    await Task.Delay(1000);
                    computersTextBubble.Visible = false;
                    k2 = -1;
                    lazes();
                }
            }
        }

        //nakon što komp baci kartu, igrač ima priliku reći mu da laže
        //u fji lažeš postavljamo label Lazes i buttone Da/Ne
        private void lazes()
        {
            Lazes.Visible = true;
            Da.Visible = true;
            Ne.Visible = true;
        }

        private void backToPlayer(object sender, EventArgs e)
        {
            Lazes.Visible = false;
            Da.Visible = false;
            Ne.Visible = false;
            if (turn == 0)
            {
                if (shuffledDeck.Any())
                    dealOne();
                playersTurn();
            }
        }

        //ako je pritisnuto "DA" na upit laže li komp, potrebno je okrenuti zadnju kartu s thrown stacka
       async private void turnLast(object sender, EventArgs e)
        {
            Lazes.Visible = false;
            Da.Visible = false;
            Ne.Visible = false;
            if (thrown.Count != 0)
            {
                Card temp = thrown.Pop();
                lastCardonDeck.Image = temp.im;
                lastCardonDeck.Size = new Size(100, 165);
                lastCardonDeck.SizeMode = PictureBoxSizeMode.Zoom;
                lastCardonDeck.Location = new Point(585, 240);
                lastCardonDeck.Visible = true;
                Controls.Add(lastCardonDeck);
                await Task.Delay(1000);

                if (temp.cSuit != computerSuit)
                {
                    lastCardonDeck.Visible = false;
                    computers[computerTotal] = temp;
                    computerTotal++;
                    while (thrown.Any())
                    {
                        computers[computerTotal] = thrown.Pop();
                        computerTotal++;
                    }
                }
                else
                {
                    lastCardonDeck.Visible = false;
                    players[playerTotal] = temp;
                    flipFront(ref players[playerTotal]);
                    playerTotal++;
                    while (thrown.Any())
                    {
                        players[playerTotal] = thrown.Pop();
                        flipFront(ref players[playerTotal]);
                        playerTotal++;
                    }

                }
                updateComputer();
                updatePlayer();
                if (shuffledDeck.Any())
                    dealOne();
                playersTurn();

            }
        }

        private void dealOne()
        {
             players[playerTotal] = shuffledDeck.Pop();
             computers[computerTotal] = shuffledDeck.Pop();
             computerTotal++;
             playerTotal++;
             updateComputer();
             updatePlayer();
        }

        //poravnava playerove/computerove karte u ovisnosti o tome koliko ih ima
        private void updatePlayer()
        {
            if (playerTotal != 0)
            {
                int a = 700 / playerTotal;
                for (int i = 0; i < playerTotal; ++i)
                {
                        flipFront(ref players[i]);
                        players[i].picBox.Location = new Point(200 + a * i, 435);
                        players[i].picBox.BringToFront();
                    
                }
            }
        }

        private void updateComputer()
        {
            if (computerTotal != 0)
            {
                int a = 700 / computerTotal;
                for (int i = 0; i < computerTotal; ++i)
                {
                    computers[i].picBox.Location = new Point(250 + a * i, 40);
                    computers[i].picBox.BringToFront();               

                }
            }
        }
    }

    public class Card
    {
        public PictureBox picBox;
        public Image im;
        public int cType;
        public int cSuit;
        public Point Location;

        public Card(PictureBox p, Image i, int ct, int cs, Point l)
        {
            picBox = p;
            im = i;
            cType = ct;
            cSuit = cs;
            Location = l;
        }
    }
}
