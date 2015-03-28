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
            menu.AddItem(new MenuItem("delayflat", "delay").SetValue(new Slider(120, 80, 200)));
            menu.AddItem(new MenuItem("delaymul", "delay multiplier/100").SetValue(new Slider(110, 100, 150)));
            menu.AddItem(new MenuItem("delayflat2", "time after spear becomes invisible").SetValue(new Slider(150, 110, 1200)));
            menu.AddItem(new MenuItem("move", "move to cursorWIP").SetValue(new Slider(150, 110, 1200)));
            menu.AddItem(new MenuItem("debug", "Write last delay(debug)").SetValue(false));
            menu.AddItem(new MenuItem("", "Have fun! Written by Drake."));
            menu.AddToMainMenu();
        }



                    

        private static void onUpdate(EventArgs args)
        {
            if (lck||!menu.Item("Q").GetValue<KeyBind>().Active||!Q.IsReady()) return;
            IEnumerable<Obj_AI_Base> a = ObjectManager.Get<Obj_AI_Base>().Where(obj =>obj.IsValidTarget()&&me.Distance(obj)<me.AttackRange);
            if (a.Any())
            {
                pos = Game.CursorPos;
                lck = true;
                int delay = (int)((me.AttackCastDelay + 1000 * me.Distance(a.First()) / me.BasicAttack.MissileSpeed) * (menu.Item("delaymul").GetValue<Slider>().Value / 100f) + menu.Item("delayflat").GetValue<Slider>().Value);


                me.IssueOrder(GameObjectOrder.AttackUnit, a.First());
                Utility.DelayAction.Add(delay, castQ);
                if (menu.Item("debug").GetValue<bool>())
                    Game.PrintChat(delay.ToString());
            }
            
        }
        private static void castQ()
        {
            Q.Cast(pos);
            Utility.DelayAction.Add(menu.Item("delayflat2").GetValue<Slider>().Value, () => me.IssueOrder(GameObjectOrder.HoldPosition, me.ServerPosition));
            lck = false;

        }
    }
}