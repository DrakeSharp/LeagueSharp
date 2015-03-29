using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SomethingOP
{
    internal class Program
    {

        private static bool havetp=true;
        public static Menu menu;
        private static bool order;
        private static float predictedHealth;
        private static float predictedMana ;
        private static float timeto;
        private static bool waiting;
        private static float lastmov;
        private static float movangle;
        private static Vector3 tgt;
        private static Vector2 dest;
        private static float radius;
        private static Vector3 tploc;
        private static Vector2 bas;
        private static Obj_AI_Hero me;
        private static SpellDataInst teleport;

        private static void Main(string[] args)
        {

            
            dest = new Vector2();
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }
        private static void GameOnOnGameLoad(EventArgs args)
        {
            me = ObjectManager.Player;
            if (me.Team.Equals(GameObjectTeam.Order))
            {
                tploc=new Vector3(191,422,185);
                order = true;
                bas = new Vector2(436.8432f, 455.4092f);
                radius = 1052.47f;
            }
            else
            {
                tploc = new Vector3(14314, 14566, 185);
                order = false;
                bas = new Vector2(14373.89f, 14417.92f);
                radius = 1130.122f;
            }

            teleport = me.Spellbook.GetSpell(me.GetSpellSlot("SummonerTeleport"));
            if (teleport == null || teleport.Slot == SpellSlot.Unknown)
                havetp=false;
            Game.PrintChat(havetp.ToString());
            menu = new Menu("Perfect start", "ps", true);
            menu.AddItem(new MenuItem("en", "Enabled").SetValue(true));
            menu.AddItem(new MenuItem("tp", "Teleport support").SetValue(true));
            menu.AddItem(new MenuItem("debug", "Debug").SetValue(false));
            menu.AddItem(new MenuItem("", "Have fun! Written by Drake."));
            menu.AddToMainMenu();
            Game.OnUpdate += onUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!havetp||!menu.Item("tp").GetValue<bool>()) return;
            if (me.Distance(tploc) < 500)
            {
                if (me.Distance(tploc) < 100)
                    Render.Circle.DrawCircle(tploc, 100, System.Drawing.Color.DeepPink, 5, true);
                else
                    Render.Circle.DrawCircle(tploc, 100, System.Drawing.Color.Blue, 5, true);
            }
        }


        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!menu.Item("en").GetValue<bool>() || !sender.IsMe || args.Order != GameObjectOrder.MoveTo) return;
            tgt = args.TargetPosition;
            movangle = GetAngleDegree(me.Position.To2D(), tgt.To2D());
            lastmov = Game.Time + .5f;
            waiting = false;
            if (!menu.Item("debug").GetValue<bool>()) return;
            Game.PrintChat(tgt.ToString() + "  " + Convert.ToString(movangle));
        }

        private static void onUpdate(EventArgs args)
        {
            if(!menu.Item("en").GetValue<bool>())return;
            if ((((order && (movangle < 0f || movangle > 4.5) ) || (!order && (movangle < 3.3 && movangle > 1.3))) && Math.Abs(me.Position.To2D().Distance(bas) - (radius-40)) < 30) && lastmov < Game.Time && !waiting)
            {
                waiting = true;
                dest = new Vector2(bas.X, bas.Y+radius).RotateAroundPoint(bas, GetAngleDegree(me.Position.To2D(), bas));
                me.IssueOrder(GameObjectOrder.MoveTo, dest.To3D(), false);
                timeto = me.Position.To2D().Distance(tgt.To2D())/me.MoveSpeed;
                predictedMana = me.PARRegenRate*timeto;
                predictedHealth = me.HPRegenRate * timeto;
            }
            if (havetp && menu.Item("tp").GetValue<bool>() && me.Distance(tploc) < 100 && me.MaxHealth * (.6f) < me.Health && me.MaxMana * (.6f) < me.Mana)
            {
                me.Spellbook.CastSpell(teleport.Slot, Hud.SelectedUnit);
            }
            if (!waiting || (!(me.MaxMana < 1) && !(predictedMana + me.Mana > me.MaxMana * (.97f ))) ||
                !(predictedHealth + me.Health > me.MaxHealth*(.97f))) return;
            waiting = false;
            me.IssueOrder(GameObjectOrder.MoveTo, tgt, false);

        }

        public static float GetAngleDegree(Vector2 origin, Vector2 target)
        {
            return (float) (Math.Atan2(origin.Y - target.Y, origin.X - target.X) + 1.57079633f);
        }
    }
}