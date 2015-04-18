using System;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;


namespace Template
{
    class Program
    {
        private static float[] minimapPositions;
        private static Vector3 kappa;
        private static Menu menu;
        private static Spell E;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            kappa = new Vector3();
            E = new Spell(SpellSlot.E);
            menu = new Menu("Get cancer kha", "jump", true);
            menu.AddItem(new MenuItem("j", "JUMP!").SetValue(new KeyBind('T', KeyBindType.Press)));
            menu.AddItem(new MenuItem("b", "RECALL!").SetValue(new KeyBind('Z', KeyBindType.Press)));
            menu.AddToMainMenu();
            setMinimap();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 1 || cursorOnMinimap()) return;
            Point point = new Point(args.LParam);
            Vector3 kappa2 = Drawing.ScreenToWorld(point.X, point.Y);
            Vector3 kappa3 = new Vector2(kappa2.X, kappa2.Y + 500).To3D();
            if (ObjectManager.Player.Position.Distance(kappa3) < 3000 && !ObjectManager.Player.IsDead) return;
            kappa = ObjectManager.Player.Position - (ObjectManager.Player.Position - kappa3) * (((ObjectManager.Player.Position - kappa3).Length() + 800) / (ObjectManager.Player.Position - kappa3).Length());
        }
        static void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.Position.Distance(kappa) < 3000&&!ObjectManager.Player.IsDead) return;
            line(kappa - (kappa - ObjectManager.Player.Position) * (1000 / (kappa - ObjectManager.Player.Position).Length()), kappa, Color.Green);
            Render.Circle.DrawCircle(kappa, 1000, Color.Red);
            Render.Circle.DrawCircle(kappa, 10, Color.Red);

        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (menu.Item("b").GetValue<KeyBind>().Active)
                toBase();
            if (!menu.Item("j").GetValue<KeyBind>().Active) return;
            E.Cast(kappa, true);
        }
        private static void setMinimap()
        {
            Vector2 corner = Drawing.WorldToMinimap(new Vector3(-500, 15500, 0));
            minimapPositions = new float[2];
            minimapPositions[0] = corner.X;
            minimapPositions[1] = corner.Y;
        }
        private static bool cursorOnMinimap()
        {

            Vector2 cp = Utils.GetCursorPos();
            return (cp.X > minimapPositions[0] && cp.Y > minimapPositions[1]);

        }
        private static void toBase()
        {

            if (isLeft())
            {
                if (ObjectManager.Player.Team == GameObjectTeam.Order)
                {
                    if (ObjectManager.Player.Level >= 6)
                    {
                        E.Cast(new Vector3(253, -843, 95), true);
                        kappa = new Vector3(253, -843, 95);
                    }
                    else
                    {
                        E.Cast(new Vector3(522, -533, 95), true);
                        kappa = new Vector3(522, -533, 95);
                    }
                }
                else
                {
                    if (ObjectManager.Player.Level >= 6)
                    {
                        E.Cast(new Vector3(15804, 14547, 95), true);
                        kappa = new Vector3(15804, 14547, 95);
                    }
                    else
                    {
                        E.Cast(new Vector3(15804, 14547, 95), true);
                        kappa = new Vector3(15804, 14547, 95);
                    }
                }
            }
            else
            {
                if (ObjectManager.Player.Team == GameObjectTeam.Order)
                {
                    if (ObjectManager.Player.Level >= 6)
                    {
                        E.Cast(new Vector3(-1057, 74, 95), true);
                        kappa = new Vector3(-1057, 74, 95);
                    }
                    else
                    {
                        E.Cast(new Vector3(-734, 592, 95), true);
                        kappa = new Vector3(-734, 592, 95);
                    }
                }
                else
                {
                    if (ObjectManager.Player.Level >= 6)
                    {
                        E.Cast(new Vector3(14662, 15766, 95), true);
                        kappa = new Vector3(14662, 15766, 95);
                    }
                    else
                    {
                        E.Cast(new Vector3(14662, 15766, 95), true);
                        kappa = new Vector3(14662, 15766, 95);
                    }
                }
            }
            Utility.DelayAction.Add(100, () => ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, ObjectManager.Player.ServerPosition));
        }
        private static bool isLeft()
        {
            Vector2 a = new Vector2(0, 0);
            Vector2 b = new Vector2(15000, 15000);
            Vector2 c = ObjectManager.Player.ServerPosition.To2D();
            return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0;
        }
        private static void line(Vector3 a, Vector3 b, Color c)
        {
            Vector2 aa = Drawing.WorldToScreen(a);
            Vector2 bb = Drawing.WorldToScreen(b);
            Drawing.DrawLine(aa, bb, 4, c);
        }
    }
}
