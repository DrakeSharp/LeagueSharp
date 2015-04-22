using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Drake
{
    internal class Program
    {
        private static Obj_AI_Hero player, annie;

        private static void Main()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            player = ObjectManager.Player;
            annie = ObjectManager.Get<Obj_AI_Hero>().First(champ => champ.IsEnemy&&champ.ChampionName == "Annie");
            if (annie == null) return;
            Drawing.OnDraw += Drawing_OnDraw;
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            if (player.Distance(annie, true) > 2890000 || !annie.IsVisible) return;
            foreach (var buff in annie.Buffs)
            {
                if (buff.Name == "pyromania")
                {
                    if(buff.Count == 2)
                        Render.Circle.DrawCircle(annie.Position, 60, Color.Yellow);
                    else if (buff.Count == 3)
                        Render.Circle.DrawCircle(annie.Position, 60, Color.OrangeRed);
                    return;
                }
                if (buff.Name != "pyromania_particle") continue;
                Render.Circle.DrawCircle(annie.Position, 60, Color.Red);
                return;
            }
        }
    }
}
