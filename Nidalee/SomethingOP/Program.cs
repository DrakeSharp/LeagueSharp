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
            menu.AddItem(new MenuItem("delayflat", "delay").SetValue(new Slider(0, -100)));
            menu.AddItem(new MenuItem("delaymul", "delay multiplier*100").SetValue(new Slider(163, 100, 200)));
            menu.AddItem(new MenuItem("debug", "Write last delay").SetValue(false));
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
                int delay = (int)((me.AttackCastDelay + 1000 * me.Distance(a[0]) / me.BasicAttack.MissileSpeed) * (menu.Item("delaymul").GetValue<Slider>().Value / 100f) + menu.Item("delayflat").GetValue<Slider>().Value);

                
                me.IssueOrder(GameObjectOrder.AttackUnit, a[0]);
                Utility.DelayAction.Add(delay, () => Q.Cast(Game.CursorPos));
                if (menu.Item("debug").GetValue<bool>())
                    Game.PrintChat(delay.ToString());
            }
            
        }
    }
}