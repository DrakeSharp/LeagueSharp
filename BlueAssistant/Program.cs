using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Drake.DLib;
using Color = System.Drawing.Color;

namespace Drake
{
    enum State
    {
        Waiting,
        Valid,
        Moving2,
        Moving3
    };
    class Program
    {
        private static MenuItem enabled;
        private static bool o;
        private static Obj_AI_Base blue;
        private static State state;
        private static Obj_AI_Hero player;
        private static readonly Vector3 order = new Vector3(3872, 7900, 51.9025f);
        private static readonly Vector3 order2 = new Vector3(3770.784f,8400.467f, 51.9025f);
        private static readonly Vector3 order3 = new Vector3(3783.809f, 8623.649f, 51.9025f);
        private static readonly Vector3 chaos = new Vector3(10931.73f, 6990.844f, 51.72291f);
        private static readonly Vector3 chaos2 = new Vector3(11110.34f,6383.18f, 51.72291f);
        private static readonly Vector3 chaos4 = new Vector3(11072f,6258f, 51.72291f);
        private static readonly Vector3 chaos3 = new Vector3(11000.14f, 6263.923f, 51.72291f);
        static void Main()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            player = ObjectManager.Player;
            Menu menu = new Menu("Blue Assistant", "b", true);
            enabled = new MenuItem("Enabled", "Enabled").SetValue(true);
            menu.AddItem(enabled);
            menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            
            
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (!enabled.GetValue<Boolean>()) return;
            if (args.Msg != 513) return;
            if (state == State.Valid && Game.CursorPos.Distance(o? order2:chaos2, true) < 4900)
            {
                if(player.Distance(o? order2:chaos2, true) < 900)
                {
                    player.IssueOrder(GameObjectOrder.MoveTo,( o ? order2 : chaos2)+new Vector3(0, 90, 0));
                    Utility.DelayAction.Add(400, Start);
                }else Start();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!enabled.GetValue<Boolean>()) return;
            if (state==State.Valid)
                Render.Circle.DrawCircle(o ? order2 : chaos2, 70,
                    Game.CursorPos.Distance(o ? order2 : chaos2, true) < 4900 ? Color.GreenYellow : Color.OrangeRed);
            Render.Circle.DrawCircle(new Vector3(7500, 7500, 50), 70,
                    Game.CursorPos.Distance(o ? order2 : chaos2, true) < 4900 ? Color.GreenYellow : Color.OrangeRed);
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (!enabled.GetValue<Boolean>()) return;
            var selected = (Obj_AI_Base) Hud.SelectedUnit;
            if (state==State.Waiting&&selected.BaseSkinName == "SRU_Blue"&&HasSlaves(selected))
            {
                blue = selected;
                if (selected.Position.Compare(chaos, 5))
                {
                    Utility.DelayAction.Add(5000, () => { state = State.Waiting; });
                    o = false;
                    Drawing.OnDraw += Drawing_OnDraw;
                    Game.OnWndProc += Game_OnWndProc;
                    state = State.Valid;
                }
                else if (selected.Position.Compare(order, 5))
                {
                    o = true;
                    Drawing.OnDraw += Drawing_OnDraw;
                    Game.OnWndProc += Game_OnWndProc;
                    state = State.Valid;
                }

            }
            else if ((state == State.Moving2) && Arrived())
            {
                Utility.DelayAction.Add(1500, () => { state = State.Waiting; });
                state = State.Moving3;
                player.IssueOrder(GameObjectOrder.AttackUnit, blue);
                Utility.DelayAction.Add((o ? 400 : 750)+ (int)((player.MoveSpeed - 330) * .4), Move);
            }
            else if ((state == State.Moving3) && Arrived())
            {
                
                player.IssueOrder(GameObjectOrder.AttackUnit, blue);
                state = State.Waiting;
            }
        }
        static bool HasSlaves(Obj_AI_Base golem)
        {
            return
                ObjectManager.Get<Obj_AI_Minion>().Count(slave => slave.Team == GameObjectTeam.Neutral && golem.Distance(slave) < 270)==3;
        }
        static bool Arrived()
        {
            switch (state)
            {
                case State.Moving2:
                    return player.ServerPosition.Compare(o ? order2 : chaos2, 2);
                case State.Moving3:
                    return player.ServerPosition.Compare(o ? order3 : chaos4, 2);
            }
            return false;
        }
        static void Start()
        {
            state = State.Moving2;
            Move();
            Game.OnWndProc -= Game_OnWndProc;
            Drawing.OnDraw -= Drawing_OnDraw;
        }
        static void Move()
        {
            switch (state)
            {
                case State.Moving2:
                    player.IssueOrder(GameObjectOrder.MoveTo, o ? order2 : chaos2);
                    break;
                case State.Moving3:
                    player.IssueOrder(GameObjectOrder.MoveTo, o ? order3 : chaos3);
                    break;
            }
        }

    }

}
