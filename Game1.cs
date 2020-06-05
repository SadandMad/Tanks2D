using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Tanks2D
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D PlayerCorp, PlayerTowr;
        Texture2D BrickT, ShotT;
        Texture2D Grn, GrnB;
        Texture2D Red, RedB;
        Vector2 TextureAttitude;
        Tank Player, Enemy;
        int PlayerTeam, EnemyTeam;
        bool ShotFired = false;
        IPEndPoint enemyIP;
        UdpClient listener;
        List<Brick> Bricks = new List<Brick>(144) { //Top
                                                    new Brick(0, 0, 500), new Brick(120, 0, 500), new Brick(240, 0, 500), new Brick(360, 0, 500),
                                                    new Brick(480, 0, 500), new Brick(600, 0, 500), new Brick(720, 0, 500), new Brick(840, 0, 500),
                                                    new Brick(960, 0, 500), new Brick(1080, 0, 500), new Brick(1200, 0, 500), new Brick(1320, 0, 500),
                                                    new Brick(1440, 0, 500), new Brick(1560, 0, 500), new Brick(1680, 0, 500), new Brick(1800, 0, 500),
                                                    // Left
                                                    new Brick(0, 120, 500), new Brick(0, 240, 500), new Brick(0, 360, 500), new Brick(0, 480, 500),
                                                    new Brick(0, 600, 500), new Brick(0, 720, 500), new Brick(0, 840, 500),
                                                    //Player Left
                                                    new Brick(480, 120, 300), new Brick(480, 240, 300), new Brick(480, 360, 300), new Brick(480, 480, 300), new Brick(480, 600, 300),
                                                    // Center
                                                    new Brick(840, 360, 200), new Brick(840, 480, 200), new Brick(960, 480, 200), new Brick(960, 600, 200), 
                                                    //Player Right
                                                    new Brick(1320, 360, 300), new Brick(1320, 480, 300), new Brick(1320, 600, 300), new Brick(1320, 720, 300), new Brick(1320, 840, 300),
                                                    // Right
                                                    new Brick(1800, 120, 500), new Brick(1800, 240, 500), new Brick(1800, 360, 500), new Brick(1800, 480, 500),
                                                    new Brick(1800, 600, 500), new Brick(1800, 720, 500), new Brick(1800, 840, 500),
                                                    //Bottom
                                                    new Brick(0, 960, 500), new Brick(120, 960, 500), new Brick(240, 960, 500), new Brick(360, 960, 500),
                                                    new Brick(480, 960, 500), new Brick(600, 960, 500), new Brick(720, 960, 500), new Brick(840, 960, 500),
                                                    new Brick(960, 960, 500), new Brick(1080, 960, 500), new Brick(1200, 960, 500), new Brick(1320, 960, 500),
                                                    new Brick(1440, 960, 500), new Brick(1560, 960, 500), new Brick(1680, 960, 500), new Brick(1800, 960, 500)};

        public Game1(int input, IPEndPoint iep)
        {
            enemyIP = iep;
            PlayerTeam = input;
            EnemyTeam = 3 - input;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = true;
            var hAttitude = graphics.PreferredBackBufferHeight / 1080.0f;
            TextureAttitude = new Vector2(hAttitude, hAttitude);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            PlayerCorp = Content.Load<Texture2D>("MBTCorpSprite");
            PlayerTowr = Content.Load<Texture2D>("MBTTowrSprite");
            BrickT = Content.Load<Texture2D>("Brick");
            ShotT = Content.Load<Texture2D>("ShotSprite");
            Grn = Content.Load<Texture2D>("Grn");
            GrnB = Content.Load<Texture2D>("GrnB");
            Red = Content.Load<Texture2D>("Red");
            RedB = Content.Load<Texture2D>("RedB");
            Player = new Tank(500, 2, PlayerTeam, 110, new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            Enemy = new Tank(500, 2, EnemyTeam, 110, new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            listener = new UdpClient();
            listener.Connect(enemyIP);
            Packet msg;
            if (PlayerTeam == 2)
            {
                msg = new Packet(4);
                listener.Send(msg.getBytes(), msg.MsgLength);
            }
            else
            {
                do
                {
                    IPEndPoint ip = null;
                    msg = new Packet(listener.Receive(ref ip));
                }
                while (msg.Type != 4);
            }
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || Player.curHP <= 0 || Enemy.curHP <= 0)
                Exit();
            while (listener.Available > 0)
            {
                IPEndPoint ip = null;
                Packet receive = new Packet(listener.Receive(ref ip));
                if (receive.Type == 5)
                {
                    Vector2 coord;
                    float dir;
                    float time;
                    receive.GetShot(out coord, out dir, out time);
                    Shot shot = new Shot();
                    shot.direction = dir;
                    shot.StartPosition = coord;
                    shot.StartTime = time;
                    Enemy.Shots.Add(shot);
                }
                else if (receive.Type == 6)
                {
                    receive.GetCoord(out Enemy.Position, out Enemy.CorpRotation, out Enemy.TowrRotation);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Vector2 LastPosition = Player.Position;
                Player.Position.Y += (float)(Math.Sin(Player.CorpRotation + Math.PI / 2));
                Player.Position.X += (float)(Math.Cos(Player.CorpRotation + Math.PI / 2));
                if (Collisions(Enemy.Position, Enemy.CorpRotation, 100, 88, PlayerCorp.Width / 2,
                    Player.Position, Player.CorpRotation, 100, 88, PlayerCorp.Width / 2))
                {
                    Player.Position = LastPosition;
                }
                foreach (Brick brick in Bricks)
                {
                    if (Collisions(Player.Position, Player.CorpRotation, 100, 88, PlayerCorp.Width / 2, new Vector2(brick.Position.X + 60, brick.Position.Y + 60), 0, 60, 60, 60))
                    {
                        Player.Position = LastPosition;
                    }
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Vector2 LastPosition = Player.Position;
                Player.Position.Y -= (float)(Math.Sin(Player.CorpRotation + Math.PI / 2));
                Player.Position.X -= (float)(Math.Cos(Player.CorpRotation + Math.PI / 2));
                if (Collisions(Enemy.Position, Enemy.CorpRotation, 100, 88, PlayerCorp.Width / 2,
                    Player.Position, Player.CorpRotation, 100, 88, PlayerCorp.Width / 2))
                {
                    Player.Position = LastPosition;
                }
                foreach (Brick brick in Bricks)
                {
                    if (Collisions(Player.Position, Player.CorpRotation, 100, 88, PlayerCorp.Width / 2, new Vector2(brick.Position.X + 60, brick.Position.Y + 60), 0, 60, 60, 60))
                    {
                        Player.Position = LastPosition;
                    }
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                float LastRotation = Player.CorpRotation;
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                    Player.CorpRotation -= 0.005f;
                else
                    Player.CorpRotation += 0.005f;
                if (Collisions(Enemy.Position, Enemy.CorpRotation, 100, 88, PlayerCorp.Width / 2,
                    Player.Position, Player.CorpRotation, 100, 88, PlayerCorp.Width / 2))
                {
                    Player.CorpRotation = LastRotation;
                }
                foreach (Brick brick in Bricks)
                {
                    if (Collisions(Player.Position, Player.CorpRotation, 100, 88, PlayerCorp.Width / 2, new Vector2(brick.Position.X + 60, brick.Position.Y + 60), 0, 60, 60, 60))
                    {
                        Player.CorpRotation = LastRotation;
                    }
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                float LastRotation = Player.CorpRotation;
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                    Player.CorpRotation += 0.005f;
                else
                    Player.CorpRotation -= 0.005f;
                if (Collisions(Enemy.Position, Enemy.CorpRotation, 100, 88, PlayerCorp.Width / 2,
                Player.Position, Player.CorpRotation, 100, 88, PlayerCorp.Width / 2))
                {
                    Player.CorpRotation = LastRotation;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.E) || Keyboard.GetState().IsKeyDown(Keys.Right))
                Player.TowrRotation += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.Q) || Keyboard.GetState().IsKeyDown(Keys.Left))
                Player.TowrRotation -= 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !ShotFired)
                if (Player.Fired == 0)
                {
                    ShotFired = true;
                    Shot shot = new Shot(Player, gameTime);
                    Player.Shots.Add(shot);
                    Player.Fired = Player.Reload;
                    Packet newShot = new Packet(5, shot.StartPosition, shot.direction, shot.StartTime);
                    listener.Send(newShot.getBytes(), newShot.MsgLength);
                }
            if (Keyboard.GetState().IsKeyUp(Keys.Space) && ShotFired)
            {
                ShotFired = false;
            }
            if (Player.Fired > 0)
                Player.Fired--;

            Packet newCoord = new Packet(6, Player.Position, Player.CorpRotation, Player.TowrRotation);
            listener.Send(newCoord.getBytes(), newCoord.MsgLength);
            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkOliveGreen);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(PlayerCorp, new Vector2(Enemy.Position.X, Enemy.Position.Y), null, null, new Vector2(PlayerCorp.Width / 2, 100), Enemy.CorpRotation, TextureAttitude, Color.Red);
            spriteBatch.Draw(PlayerCorp, new Vector2(Player.Position.X, Player.Position.Y), null, null, new Vector2(PlayerCorp.Width / 2, 100), Player.CorpRotation, TextureAttitude, null);
            spriteBatch.Draw(PlayerTowr, new Vector2(Enemy.Position.X, Enemy.Position.Y), null, null, new Vector2(PlayerTowr.Width / 2, 80), Enemy.CorpRotation + Enemy.TowrRotation, TextureAttitude, Color.Red);
            spriteBatch.Draw(PlayerTowr, new Vector2(Player.Position.X, Player.Position.Y), null, null, new Vector2(PlayerTowr.Width / 2, 80), Player.CorpRotation + Player.TowrRotation, TextureAttitude, null);

            List<Shot> toRemove = new List<Shot>();
            foreach (Shot shot in Player.Shots)
            {
                if (Collisions(Enemy.Position, Enemy.CorpRotation, 100, 88, PlayerCorp.Width / 2,
                    new Vector2((float)(shot.StartPosition.X - (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Sin(shot.direction)),
                                (float)(shot.StartPosition.Y + (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Cos(shot.direction))),
                               shot.direction, ShotT.Height / 2, ShotT.Height / 2, ShotT.Width / 2))
                {
                    Enemy.curHP -= shot.Dmg;
                    toRemove.Add(shot);
                }
                foreach (Brick brick in Bricks)
                {
                    if (Collisions(new Vector2((float)(shot.StartPosition.X - (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Sin(shot.direction)),
                                               (float)(shot.StartPosition.Y + (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Cos(shot.direction))),
                                   shot.direction, ShotT.Height / 2, ShotT.Height / 2, ShotT.Width / 2, new Vector2(brick.Position.X + 60, brick.Position.Y + 60), 0, 60, 60, 60))
                    {
                        brick.curHP -= shot.Dmg;
                        toRemove.Add(shot);
                    }
                }                
            }
            foreach (Shot shot in toRemove)
            {
                Player.Shots.Remove(shot);
            }
            toRemove.Clear();

            foreach (Shot shot in Enemy.Shots)
            {
                if (Collisions(Player.Position, Player.CorpRotation, 100, 88, PlayerCorp.Width / 2,
                    new Vector2((float)(shot.StartPosition.X - (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Sin(shot.direction)),
                                (float)(shot.StartPosition.Y + (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Cos(shot.direction))),
                               shot.direction, ShotT.Height / 2, ShotT.Height / 2, ShotT.Width / 2))
                {
                    Player.curHP -= shot.Dmg;
                    toRemove.Add(shot);
                }
                foreach (Brick brick in Bricks)
                {
                    if (Collisions(new Vector2((float)(shot.StartPosition.X - (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Sin(shot.direction)),
                                               (float)(shot.StartPosition.Y + (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Cos(shot.direction))),
                                   shot.direction, ShotT.Height / 2, ShotT.Height / 2, ShotT.Width / 2, new Vector2(brick.Position.X + 60, brick.Position.Y + 60), 0, 60, 60, 60))
                    {
                        brick.curHP -= shot.Dmg;
                        toRemove.Add(shot);
                    }
                }
            }
            foreach (Shot shot in toRemove)
            {
                Enemy.Shots.Remove(shot);
            }
            toRemove.Clear();

            foreach (Shot shot in Player.Shots) 
                spriteBatch.Draw(ShotT, new Vector2((float)(shot.StartPosition.X - (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Sin(shot.direction)),
                                                    (float)(shot.StartPosition.Y + (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Cos(shot.direction))),
                                 null, null, new Vector2(ShotT.Width / 2, ShotT.Height / 2), shot.direction, TextureAttitude, null);
            foreach (Shot shot in Enemy.Shots)
                spriteBatch.Draw(ShotT, new Vector2((float)(shot.StartPosition.X - (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Sin(shot.direction)),
                                                    (float)(shot.StartPosition.Y + (((float)gameTime.TotalGameTime.TotalMilliseconds - shot.StartTime) * 60 / 1000 + 25) * shot.speed * Math.Cos(shot.direction))),
                                 null, null, new Vector2(ShotT.Width / 2, ShotT.Height / 2), shot.direction, TextureAttitude, null);
            List<Brick> toRem = new List<Brick>();
            foreach (Brick brick in Bricks)
            {
                if (brick.curHP > 0)
                    spriteBatch.Draw(BrickT, brick.Position, Color.White);
                else
                    toRem.Add(brick);
            }
            foreach(Brick brick in toRem)
            {
                Bricks.Remove(brick);
            }
            toRem.Clear();

            spriteBatch.Draw(GrnB, new Vector2(20, 20), Color.White);
            spriteBatch.Draw(Grn, new Rectangle(new Point(20, 20), new Point(Grn.Width * Player.curHP / 500, 56)), Color.White);
            spriteBatch.Draw(RedB, new Vector2(1606, 20), Color.White);
            spriteBatch.Draw(Red, new Rectangle(new Point(1606, 20), new Point(Red.Width * Enemy.curHP / 500, 56)), new Rectangle(new Point(0, 0), new Point(Red.Width * Enemy.curHP / 500, 56)), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public bool Collisions(Vector2 firstPosition, float firstAngle, int firstForw, int firstBack, int firstSide, Vector2 secondPosition, float secondAngle, int secondForw, int secondBack, int secondSide)
        {
            Point[] firstPoints = new Point[4];
            Point[] secondPoints = new Point[4];
            firstPoints[0] = new Point((int)Math.Round(firstPosition.X + firstSide / Math.Cos(firstAngle) + (firstForw - firstSide * Math.Tan(firstAngle)) * Math.Sin(firstAngle)), (int)Math.Round(firstPosition.Y + (firstForw - firstSide * Math.Tan(firstAngle)) * Math.Cos(firstAngle)));
            firstPoints[1] = new Point((int)Math.Round(firstPosition.X + firstSide / Math.Cos(firstAngle) + (firstForw - firstSide * Math.Tan(firstAngle)) * Math.Sin(firstAngle)), (int)Math.Round(firstPosition.Y - firstBack * Math.Cos(firstAngle) + firstSide * Math.Sin(firstAngle)));
            firstPoints[2] = new Point((int)Math.Round(firstPosition.X - firstSide / Math.Cos(firstAngle) - (firstBack - firstSide * Math.Tan(firstAngle)) * Math.Sin(firstAngle)), (int)Math.Round(firstPosition.Y - firstBack * Math.Cos(firstAngle) + firstSide * Math.Sin(firstAngle)));
            firstPoints[3] = new Point((int)Math.Round(firstPosition.X - firstSide / Math.Cos(firstAngle) - (firstBack - firstSide * Math.Tan(firstAngle)) * Math.Sin(firstAngle)), (int)Math.Round(firstPosition.Y + (firstForw - firstSide * Math.Tan(firstAngle)) * Math.Cos(firstAngle)));
            secondPoints[0] = new Point((int)Math.Round(secondPosition.X + secondSide / Math.Cos(secondAngle) + (secondForw - secondSide * Math.Tan(secondAngle)) * Math.Sin(secondAngle)), (int)Math.Round(secondPosition.Y + (secondForw - secondSide * Math.Tan(secondAngle)) * Math.Cos(secondAngle)));
            secondPoints[1] = new Point((int)Math.Round(secondPosition.X + secondSide / Math.Cos(secondAngle) + (secondForw - secondSide * Math.Tan(secondAngle)) * Math.Sin(secondAngle)), (int)Math.Round(secondPosition.Y - secondBack * Math.Cos(secondAngle) + secondSide * Math.Sin(secondAngle)));
            secondPoints[2] = new Point((int)Math.Round(secondPosition.X - secondSide / Math.Cos(secondAngle) - (secondBack - secondSide * Math.Tan(secondAngle)) * Math.Sin(secondAngle)), (int)Math.Round(secondPosition.Y - secondBack * Math.Cos(secondAngle) + secondSide * Math.Sin(secondAngle)));
            secondPoints[3] = new Point((int)Math.Round(secondPosition.X - secondSide / Math.Cos(secondAngle) - (secondBack - secondSide * Math.Tan(secondAngle)) * Math.Sin(secondAngle)), (int)Math.Round(secondPosition.Y + (secondForw - secondSide * Math.Tan(secondAngle)) * Math.Cos(secondAngle)));

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    int i2 = (i + 1) % 4;
                    int j2 = (j + 1) % 4;
                    if (Collision(firstPoints[i], firstPoints[i2], secondPoints[j], secondPoints[j2]))
                        return true;
                }
            return false;
        }
        public bool Collision(Point F1, Point F2, Point S1, Point S2)
        {
            if (F2.X < F1.X)
            {
                Point tmp = F1;
                F1 = F2;
                F2 = tmp;
            }
            if (S2.X < S1.X)
            {

                Point tmp = S1;
                S1 = S2;
                S2 = tmp;
            }
            if ((Math.Max(F1.X, F2.X) < Math.Min(S1.X, S2.X)) || (Math.Min(F1.X, F2.X) > Math.Max(S1.X, S2.X)) || (Math.Max(F1.Y, F2.Y) < Math.Min(S1.Y, S2.Y)) || (Math.Min(F1.Y, F2.Y) > Math.Max(S1.Y, S2.Y)))
            {
                return false;
            }
            if ((F2.X - F1.X == 0) && (S2.X - S1.X == 0))
            {
                if (F1.X == S1.X)
                    return true;
                else
                    return false;
            }
            if (F2.X - F1.X == 0)
            {
                double A = (S2.Y - S1.Y) / (S2.X - S1.X);
                double b = S1.Y - A * S1.X;
                double crossX = F1.X;
                double crossY = A * crossX + b;

                if (S1.X <= crossX && S2.X >= crossX && Math.Min(F1.Y, F2.Y) <= crossY && Math.Max(F1.Y, F2.Y) >= crossY)
                    return true;
                else
                    return false;
            }
            if (S2.X - S1.X == 0)
            {
                double A = (F2.Y - F1.Y) / (F2.X - F1.X);
                double b = F1.Y - A * F1.X;
                double crossX = S1.X;
                double crossY = A * crossX + b;

                if (F1.X <= crossX && F2.X >= crossX && Math.Min(S1.Y, S2.Y) <= crossY && Math.Max(S1.Y, S2.Y) >= crossY)
                    return true;
                else
                    return false;
            }

            double FA = (F2.Y - F1.Y) / (F2.X - F1.X);
            double SA = (S2.Y - S1.Y) / (S2.X - S1.X);
            double Fb = F1.Y - FA * F1.X;
            double Sb = S1.Y - SA * S1.X;
            if (FA == SA)
            {
                if (Fb == Sb)
                    return true;
                return false;
            }
            else
            {
                double crossX = (Sb - Fb) / (SA - FA);
                if ((crossX < Math.Max(F1.X, S1.X)) || (crossX > Math.Min(F2.X, S2.X)))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }

    public class Tank
    {
        public int curHP;
        public int maxHP;
        public int Speed;
        public Vector2 Position;
        public float CorpRotation;
        public float TowrRotation;
        public List<Shot> Shots;
        public int Fired;
        public int Reload;

        public Tank(int HP, int Sp, int Pl, int Rld, Point Field)
        {
            curHP = maxHP = HP;
            Speed = Sp;
            Reload = Rld;
            Fired = 0;
            Shots = new List<Shot>();
            switch (Pl)
            {
                case 1:
                    CorpRotation = (float)(-Math.PI / 2);
                    TowrRotation = (float)(0);
                    Position = new Vector2(250, Field.Y / 2);
                    break;
                case 2:
                    CorpRotation = (float)(Math.PI / 2);
                    TowrRotation = (float)(0);
                    Position = new Vector2(Field.X - 250, Field.Y / 2);
                    break;
            }
        }
    }

    public class Shot
    {
        public int Dmg = 100;
        public float speed = 5;
        public float direction;
        public Vector2 StartPosition;
        public float StartTime;

        public Shot(Tank Source, GameTime curTime)
        {
            StartPosition.X = Source.Position.X;
            StartPosition.Y = Source.Position.Y;
            direction = Source.CorpRotation + Source.TowrRotation;
            StartTime = (float)curTime.TotalGameTime.TotalMilliseconds;
        }
        public Shot()
        {
        }
    }

    public class Brick
    {
        public int curHP;
        public int maxHP;
        public Vector2 Position;

        public Brick(float X, float Y, int HP)
        {
            curHP = maxHP = HP;
            Position = new Vector2(X, Y);
        }
    }

    internal class Packet
    {
        public byte Type;
        public UInt16 MsgLength;
        public byte[] Data;

        public Packet(int T, Vector2 Pos, float corpRot, float towrRot)
        {
            if (T == 6 || T == 5)
            {
                Type = 6;
                byte[] data = new byte[sizeof(float) * 4];
                MsgLength = 3 + sizeof(float) * 4;
                Buffer.BlockCopy(BitConverter.GetBytes(Pos.X), 0, data, 0, sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(Pos.Y), 0, data, sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(corpRot), 0, data, sizeof(float) * 2, sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(towrRot), 0, data, sizeof(float) * 3, sizeof(float));
                Data = new byte[MsgLength];
                Data = data;
            }
        }
        public Packet(byte[] D)
        {
            Type = D[0];
            MsgLength = BitConverter.ToUInt16(D, 1);
            Data = new byte[MsgLength - 3];
            Buffer.BlockCopy(D, 3, Data, 0, MsgLength - 3);
        }
        public Packet(byte T)
        {
            Type = T;
            MsgLength = 3;
        }
        public byte[] getBytes()
        {
            byte[] data = new byte[MsgLength];
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(MsgLength), 0, data, 1, 2);
            if (MsgLength > 3)
                Buffer.BlockCopy(Data, 0, data, 3, MsgLength - 3);
            return data;
        }
        public void GetCoord(out Vector2 Pos, out float corpRot, out float towrRot)
        {
            Pos = new Vector2(BitConverter.ToSingle(Data, 0), BitConverter.ToSingle(Data, sizeof(float)));
            corpRot = BitConverter.ToSingle(Data, sizeof(float) * 2);
            towrRot = BitConverter.ToSingle(Data, sizeof(float) * 3);
        }
        public void GetShot(out Vector2 Pos, out float dir, out float time)
        {
            Pos = new Vector2(BitConverter.ToSingle(Data, 0), BitConverter.ToSingle(Data, sizeof(float)));
            dir = BitConverter.ToSingle(Data, sizeof(float) * 2);
            time = BitConverter.ToSingle(Data, sizeof(float) * 3);
        }
    }
}