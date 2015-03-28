using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SomethingOP
{
    internal class Program
    {
        public static Menu menu;
        public static Spell Q;
        private static Obj_AI_Hero me;

        private static void Main(string[] args)
        {


            Game.OnUpdate += firstUpdate;
                CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            menu = new Menu("Invisible spear", "NidaQ", true);
            menu.AddItem(new MenuItem("Q", "Throw invis spear").SetValue(new KeyBind('Z', KeyBindType.Press)));
            menu.AddToMainMenu();
        }

        private static void firstUpdate(EventArgs args)
        {
            me = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q);
            Game.OnUpdate -= firstUpdate;
            Game.OnUpdate += onUpdate;
        }



        private static void onUpdate(EventArgs args)
        {
            if (!menu.Item("Q").GetValue<KeyBind>().Active||!Q.IsReady()) return;
            List<Obj_AI_Base> a=MinionManager.GetMinions(me.AttackRange);
            if (a.Count > 0)
            {
                int delay = (int)((me.AttackCastDelay + 1000*me.Distance(a[0]) / me.BasicAttack.MissileSpeed)*1.5);
                Game.PrintChat(delay.ToString());
                me.IssueOrder(GameObjectOrder.AttackUnit, a[0]);
                Utility.DelayAction.Add(delay, () => Q.Cast(Game.CursorPos));
            }
            
        }
    }
}