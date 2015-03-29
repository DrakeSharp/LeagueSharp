using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SomethingOP
{
    internal class Program
    {
        private static Vector3 pos;
        private static bool lck;
        public static Menu menu;
        public static Spell Q;
        private static Obj_AI_Hero me;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            me = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q);
            Game.OnUpdate += onUpdate;
            menu = new Menu("Invisible spear", "NidaQ", true);
            menu.AddItem(new MenuItem("Q", "Throw invis spear").SetValue(new KeyBind('Z', KeyBindType.Press)));
            menu.AddItem(new MenuItem("delayflat", "delay").SetValue(new Slider(135, 80, 200)));
            menu.AddItem(new MenuItem("delaymul", "delay multiplier/100").SetValue(new Slider(110, 100, 150)));
            menu.AddItem(new MenuItem("delayflat2", "time after spear becomes invisible").SetValue(new Slider(150, 110, 1200)));
            menu.AddItem(new MenuItem("move", "don't stop after cast").SetValue(true));
            menu.AddItem(new MenuItem("draw", "draw attack range").SetValue(true));
            menu.AddItem(new MenuItem("debug", "Write last delay(debug)").SetValue(false));
            menu.AddItem(new MenuItem("", "Have fun! Written by Drake."));
            menu.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("draw").GetValue<bool>())
                Render.Circle.DrawCircle(me.Position, me.AttackRange, System.Drawing.Color.Blue, 3, false);
        }

        private static void onUpdate(EventArgs args)
        {

            if (lck || !menu.Item("Q").GetValue<KeyBind>().Active || !Q.IsReady()) return;
            IEnumerable<Obj_AI_Base> a = ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.IsValidTarget() && me.Distance(obj) < me.AttackRange && HealthPrediction.GetHealthPrediction(obj,(int)((me.AttackCastDelay + 1000 * me.Distance(obj) / me.BasicAttack.MissileSpeed) * (menu.Item("delaymul").GetValue<Slider>().Value / 100f) + menu.Item("delayflat").GetValue<Slider>().Value))>me.CalcDamage(obj, Damage.DamageType.Physical, me.GetAutoAttackDamage(obj)) * me.PercentArmorPenetrationMod);
            if (!a.Any()) return;
            Obj_AI_Base tgt=a.OrderByDescending(obj=>me.Distance(obj)).Last();
            pos = Game.CursorPos;
            lck = true;
            int delay = (int)((me.AttackCastDelay + 1000 * me.Distance(tgt) / me.BasicAttack.MissileSpeed) * (menu.Item("delaymul").GetValue<Slider>().Value / 100f) + menu.Item("delayflat").GetValue<Slider>().Value);
            me.IssueOrder(GameObjectOrder.AttackUnit, tgt);
            Utility.DelayAction.Add(delay, castQ);
        }

        private static void castQ()
        {
            Q.Cast(pos);
            Utility.DelayAction.Add(menu.Item("delayflat2").GetValue<Slider>().Value, hold);
            lck = false;
        }
        private static void hold()
        {
            me.IssueOrder(GameObjectOrder.HoldPosition, me.ServerPosition);
            if (menu.Item("move").GetValue<bool>())
                me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            lck = false;
        }
    }
}