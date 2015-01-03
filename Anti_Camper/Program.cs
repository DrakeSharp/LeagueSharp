using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Anti_Camper
{
    internal class Program
    {
        struct danger
        {
            public Vector3 pos;
            public float time;
            public Obj_AI_Hero hero;
            public bool count;
        }

        static float lastbeep;
        static int delay = 0;
        static float test;
        static MenuItem yy;
        static MenuItem xx;
        static MenuItem lb;
        static MenuItem sound;
        static int textx;
        static int texty;
        static int duration = 0;
        static int range = 0;
        static danger dangers;
        static List<Obj_AI_Hero> heroes;
        static char x;
        static ushort skip = 500;
        static List<Obj_AI_Minion> minionz;
        static Obj_AI_Hero myhero;
        static float[] lastexp = new float[12];
        static float[] lastgain = new float[12];
        static float[] expectedgain = new float[12];
        static float[] timestamp = new float[12];
        static Vector3[] miniondied = new Vector3[12];
        public static Menu Menu;
        private static void Main(string[] argss)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }
        static void OnGameLoad(EventArgs args)
        {
            if (Utility.Map.GetMap().Type != Utility.Map.MapType.SummonersRift) return;
            Menu = new Menu("Anti_Camper", "anti", true);
            Menu.AddItem(
            new MenuItem("range", "Detecting range").SetValue(
            new Slider(2500, 0, 8000)));
            Menu.AddItem(
            new MenuItem("duration", "Display time(seconds)").SetValue(
            new Slider(3, 0, 10)));

            xx = new MenuItem("x", "X").SetValue(
                    new Slider(
                        (int)(Drawing.Direct3DDevice.Viewport.Width), 0,
                        Drawing.Direct3DDevice.Viewport.Width));
            yy = new MenuItem("y", "Y").SetValue(
                    new Slider(
                        (int)(Drawing.Direct3DDevice.Viewport.Height), 0,
                        Drawing.Direct3DDevice.Viewport.Height));
            lb = new MenuItem("freq", "Sound not more often than every seconds").SetValue(
            new Slider(0, 0, 40));
            Menu.AddItem(lb);
            Menu.AddItem(yy);
            Menu.AddItem(xx);
            sound = new MenuItem("sound", "Play sound").SetValue(false);
            Menu.AddItem(sound);
            lb.ValueChanged += Changed;
            xx.ValueChanged += Changed;
            yy.ValueChanged += Changed;
            Menu.AddToMainMenu();
            textx = xx.GetValue<Slider>().Value;
            texty = yy.GetValue<Slider>().Value;
            heroes = new List<Obj_AI_Hero>();
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsAlly)
                {
                    heroes.Add(hero);
                }
                if (hero.IsMe)
                    myhero = hero;
            }
            x = myhero.Team == GameObjectTeam.Order ? 'O' : 'C';
            Game.OnGameUpdate += OnUpdate;
            Game.OnGameProcessPacket += OnProcessPacket;
            Drawing.OnDraw += OnDraw;
        }
        private static void Changed(object sender, OnValueChangeEventArgs e)
        {
            lastbeep = 0;
            test = Game.Time + 5;
            textx = xx.GetValue<Slider>().Value;
            texty = yy.GetValue<Slider>().Value;
        }
        private static void OnUpdate(EventArgs args)
        {
            int i = 0;
            foreach (Obj_AI_Hero hero in heroes)
            {
                if (hero.Experience != lastexp[i])
                {
                    lastgain[i] = hero.Experience - lastexp[i];
                    lastexp[i] = hero.Experience;
                    if (miniondied[i].X != 0)
                    {
                        if (lastgain[i] * 1.02f < expectedgain[i])
                        {
                            danger s;
                            s.pos = miniondied[i];
                            s.time = Game.Time + duration;
                            s.hero = hero;
                            s.count = lastgain[i]/expectedgain[i]<.63f;
                            Game.PrintChat(lastgain[i] +"  "+ expectedgain[i]);
                            dangers = s;
                            miniondied[i].X = 0;
                        }
                    }
                }
                if (hero.IsMe)
                    myhero = hero;
                i++;
            }
            if (skip == 500)
            {
                range = Menu.Item("range").GetValue<Slider>().Value;
                duration = Menu.Item("duration").GetValue<Slider>().Value;
                minionz = new List<Obj_AI_Minion>();
                foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>())
                    if (minion.BaseSkinName[4].Equals(x))
                        minionz.Add(minion);
                skip = 0;
            }

            skip++;
        }

        private static void OnDraw(EventArgs args)
        {
            if (test > Game.Time) Drawing.DrawText(textx, texty, Color.Red, "MORE THAN ONE Enemy detected");
            if (dangers.pos.X == 0) return;
            if (dangers.time < Game.Time || (skip % 5 == 0 && dangers.pos.Distance(dangers.hero.Position) > range + 1000))
            {
                delay = 0;
                dangers.pos.X = 0;
            }
            string g = "";
            if (dangers.count) g = "MORE THAN ONE ";
            if ((int)(Game.Time * 2) % 2 == 1) Drawing.DrawText(textx, texty, Color.Red, g+"Enemy detected");
            Drawing.DrawCircle(dangers.pos, 1400, Color.Crimson);
            delay++;
            if (delay == 10 && sound.GetValue<Boolean>() && lastbeep < Game.Time) ThreadPool.QueueUserWorkItem(ignoredState =>
                {
                    lastbeep = Game.Time + lb.GetValue<Slider>().Value;
                    using (var player = new SoundPlayer(Path.Combine(Environment.GetFolderPath(
                          Environment.SpecialFolder.ApplicationData), "LeagueSharp\\Repositories\\E0D6961C\\trunk\\Anti_Camper\\Resources\\b.wav")))
                    {
                        player.PlaySync();
                    }
                });
        }
        private static void OnProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != 34) return;
            int nId = BitConverter.ToInt32(args.PacketData, 2);
            foreach (Obj_AI_Minion min in minionz)
            {
                if (min.NetworkId != nId || myhero.Distance(min) > range) continue;
                float xx = 0;
                if (min.BaseSkinName[15].Equals('R'))
                    xx = (float)29.44;
                else if (min.BaseSkinName[15].Equals('M'))
                    xx = (float)58.88;
                else if (min.BaseSkinName[15].Equals('S'))
                    xx = (float)92;

                short champcount = 0;
                foreach (Obj_AI_Hero hero in heroes)
                {
                    if (!hero.IsDead && (hero.IsVisible) && hero.Distance(min) < 1430)
                    {
                        champcount++;
                    }
                }
                if (champcount == 0 || champcount == 5) break;
                if (champcount > 1) xx = xx * (float)0.657;
                if (champcount > 2) xx = xx * (float)0.658;
                if (champcount > 3) xx = xx * (float)0.658;
                int i = 0;
                foreach (Obj_AI_Hero hero in heroes)
                {
                    if (!hero.IsDead && hero.Distance(min) < 1401)
                    {
                        miniondied[i] = min.Position;
                        if (Game.Time == timestamp[i])
                        {
                            expectedgain[i] += xx;

                        }
                        else
                            expectedgain[i] = xx;
                        timestamp[i] = Game.Time;
                    }
                    i++;
                }
                break;
            }
        }
    }
}
