using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
namespace Timers
{
    internal class Program
    {
        struct monster
        {
            public State state;
            public List<int> oldIds;
            public int currentid;
            public float lastupd;
            public float lastp;
            public short[] last3;
            public float deathtime;
            public string text;
            public bool show;
        }
        enum State
        {
            Alive,
            AliveNoTrack,
            Dead,
            Attacked,
            UnderAttack,
            Disengaged,
            DisengagedOrDead,
            Unknown
        };

        private static bool showstatus = false;
        private static float showstatust;
        private static monster dragon;
        private static MenuItem timerx;
        private static int timx;
        private static int timy;
        private static int statx;
        private static int staty;
        private static MenuItem timery;
        private static MenuItem statusx;
        private static MenuItem statusy;
        private static MenuItem timerkey;
        public static Menu Menu;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }
        private static void OnGameLoad(EventArgs args)
        {

            Menu = new Menu("Timers", "tim", true);
            timerx = new MenuItem("timerx", "------------------------Timer X------------------------").SetValue(
                new Slider(
                    (int) (Drawing.Direct3DDevice.Viewport.Width), 0,
                    Drawing.Direct3DDevice.Viewport.Width));
            timery = new MenuItem("timery", "Timer Y").SetValue(
                new Slider(
                    (int) (Drawing.Direct3DDevice.Viewport.Height), 0,
                    Drawing.Direct3DDevice.Viewport.Height));
            statusx = new MenuItem("statusx", "Status X").SetValue(
                new Slider(
                    (int) (Drawing.Direct3DDevice.Viewport.Width), 0,
                    Drawing.Direct3DDevice.Viewport.Width));
            statusy = new MenuItem("statusy", "Status Y").SetValue(
                new Slider(
                    (int) (Drawing.Direct3DDevice.Viewport.Height), 0,
                    Drawing.Direct3DDevice.Viewport.Height));
            Menu.AddItem(timerx);
            Menu.AddItem(timery);
            Menu.AddItem(statusx);
            Menu.AddItem(statusy);
            timerx.ValueChanged += StatusChanged;
            timery.ValueChanged += StatusChanged;
            statusx.ValueChanged += StatusChanged;
            statusy.ValueChanged += StatusChanged;
            timerkey=new MenuItem("timerkey", "Show info").SetValue(new KeyBind(9, KeyBindType.Press));
            Menu.AddItem(timerkey);
            Game.OnGameUpdate += OnUpdate;
            Game.OnGameProcessPacket += OnProcessPacket;
            Drawing.OnDraw += OnDraw;
            dragon.oldIds = new List<int> {0};
            dragon.state=State.Unknown;
            dragon.text = "Unknown, not tracked";
            dragon.show = false;
            dragon.last3 = new short[] { 0, 0, 0 };
            Menu.AddToMainMenu();
            timx = timerx.GetValue<Slider>().Value;
            timy = timery.GetValue<Slider>().Value;
            statx = statusx.GetValue<Slider>().Value;
            staty = statusy.GetValue<Slider>().Value;
            Game.PrintChat("<font color='#FF0000'>Timers by Drake loaded.</font>");
        }
        private static void OnUpdate(EventArgs args)
        {
            DragonUpdate();

        }
        private static void OnDraw(EventArgs args)
        {
            DragonDraw();    
        }
        private static void OnProcessPacket(GamePacketEventArgs args)
        {
            DragonPackets(args);
        }
        private static void DragonDraw()
        {



            if (timerkey.GetValue<KeyBind>().Active||showstatus)
            {
                if (showstatust < Game.Time) showstatus = false;
                string c = dragon.deathtime - Game.Time < 0 ? "Alive" : (dragon.deathtime - Game.Time).ToString();
                Drawing.DrawText(timx, timy,Color.DarkOrange, "Dragon timer: " +c);
                Drawing.DrawText(statx, staty, Color.DarkOrange, "Dragon status: " + dragon.text);
            }
            if (dragon.show)
            {
                Drawing.DrawText(statx, staty, Color.DarkOrange, "Dragon status: " + dragon.text);
            }
            
        }
        private static void DragonUpdate()
        {
            if (dragon.state == State.Dead && dragon.deathtime < Game.Time)
            {
                dragon.state = State.AliveNoTrack;
                dragon.text = "Alive, not tracked";
                dragon.show = false;
            }
            if (dragon.state == State.Disengaged && dragon.lastp + 8 < Game.Time)
            {
                dragon.state=State.Alive;
                dragon.text = "Alive";
                dragon.show = false;
            }
            else if (dragon.state == State.UnderAttack || dragon.state == State.Attacked||dragon.state==State.DisengagedOrDead)
            {
                if (dragon.lastp < Game.Time)
                {
                    dragon.text = "Disengaged or dead";
                    dragon.show = true;
                    dragon.state = State.DisengagedOrDead;
                }
                if (dragon.lastp+8 < Game.Time)
                {
                    dragon.oldIds.Add(dragon.currentid);
                    dragon.deathtime = dragon.lastp+360;
                    dragon.text = "Dead";
                    dragon.show = false;
                    dragon.state = State.Dead;
                }
            }

            if (dragon.lastupd > Game.Time) return;
            if (showstatus && showstatust < Game.Time) showstatus = false;
            dragon.lastupd = Game.Time + 1;
            if ((dragon.state == State.Unknown || dragon.state == State.Dead || dragon.state == State.AliveNoTrack) && !dragon.oldIds.Contains(GetDragonId()))
            {
                dragon.currentid = GetDragonId();
                dragon.text = "Alive";
                dragon.show = false;
                dragon.state=State.Alive;
            }

        }
        private static void DragonPackets(GamePacketEventArgs args)
        {

            short header = BitConverter.ToInt16(args.PacketData, 0);
            if ((header == 154 || header == 146 || header == 34) && BitConverter.ToInt32(args.PacketData, 2) == dragon.currentid)
            {
                if (header == 34)
                {
                    dragon.oldIds.Add(dragon.currentid);
                    dragon.deathtime = dragon.lastp+360;
                    dragon.state = State.Dead;
                    dragon.text = "Dead";
                    dragon.show = false;
                    dragon.last3[0] = 0;
                    dragon.last3[1] = 0;
                    dragon.last3[2] = 0;
                    return;
                }
                dragon.last3[0] = dragon.last3[1];
                dragon.last3[1] = dragon.last3[2];
                dragon.last3[2] = header;

            

            if (dragon.last3[2] == 154)
            {
                dragon.state=State.Attacked;
                dragon.text = "Just attacked";
                dragon.show = true;
            }
            if (dragon.last3[0] == 146 && dragon.last3[1] == 146 && dragon.last3[2] == 146)
            {
                dragon.state = State.UnderAttack;
                dragon.text = "Under attack";
                dragon.show = true;
            }
            if (dragon.last3[0] == 146 && dragon.last3[1] == 154 && dragon.last3[2] == 146)
            {
                dragon.text = "Disengaged";
                dragon.show = true;
                dragon.state = State.Disengaged;
                dragon.last3[0] = 0;
                dragon.last3[1] = 0;
                dragon.last3[2] = 0;
            }
            if (dragon.last3[0] == 154 && dragon.last3[1] == 154 && dragon.last3[2] == 146)
            {
                dragon.text = "Disengaged";
                dragon.show = true;
                dragon.state = State.Disengaged;
                dragon.last3[0] = 0;
                dragon.last3[1] = 0;
                dragon.last3[2] = 0;
            }

            dragon.lastp = Game.Time+1;
            }
        }
        private static int GetDragonId()
        {
            foreach (Obj_AI_Minion m in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (m.CampNumber == 6&&!m.IsDead)
                    return m.NetworkId;
            }
            return 0;
        }
        private static void StatusChanged(object sender, OnValueChangeEventArgs e)
        {
            showstatus = true;
            showstatust = Game.Time+5;
            statx = statusx.GetValue<Slider>().Value;
            staty = statusy.GetValue<Slider>().Value;
            timx = timerx.GetValue<Slider>().Value;
            timy = timery.GetValue<Slider>().Value;
        }
    }
}
