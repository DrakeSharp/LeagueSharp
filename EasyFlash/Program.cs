using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Drake
{
    internal class Program
    {
        private static float skillanglemax = .3f;
        private static float angleres = .2f;
        private static int distres = 5;
        private static int t;
        private static Menu menu;
        private static Obj_AI_Hero me;
        private static SpellSlot flashSlot;
        private static Spell flash;
        private static float angl;
        private static float turnoff;


        private static void Main()
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            me = ObjectManager.Player;
            flashSlot = me.GetSpellSlot("SummonerFlash");

            menu = new Menu("Easy Flash", "ef", true);
            if (flashSlot == SpellSlot.Unknown)
            {
                menu.AddItem(new MenuItem("", "Flash not detected. Easy Flash is disabled."));
                menu.AddToMainMenu();
                return;
            }
            flash = new Spell(flashSlot, 425);
            menu.AddItem(new MenuItem("bf", "Block failed flashes.").SetValue(true));
            menu.AddItem(new MenuItem("af", "Assisted flash.").SetValue(true));
            menu.AddItem(new MenuItem("fr", "Always flash on the max range.").SetValue(true));
            menu.AddItem(new MenuItem("range", "Flash range for calculations.").SetValue(new Slider(415, 390, 425)));
            menu.AddToMainMenu();
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }


        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot != flashSlot) return;
            Vector2 mypos = new Vector2(me.Position.X, me.Position.Y);




            Vector2 flashpos = new Vector2(me.Position.X, me.Position.Y - (menu.Item("range").GetValue<Slider>().Value)).RotateAroundPoint(mypos,
getAngleDegree(mypos, args.StartPosition.To2D()));

            Vector2 nearestfree = nearestFree(flashpos);
            if (nearestfree == flashpos || mypos.Distance(nearestfree) > mypos.Distance(flashpos))
            {
                if (!menu.Item("fr").GetValue<bool>()) return;
                args.Process = false;
                me.Spellbook.CastSpell(flashSlot, new Vector2(me.Position.X, me.Position.Y - (500)).RotateAroundPoint(mypos,
                    getAngleDegree(mypos, args.StartPosition.To2D())).To3D(), false);
            }
            else if (menu.Item("af").GetValue<bool>())
            {
                angl = getAngleDegree(mypos, args.StartPosition.To2D());
                WallOut w = calcWall(mypos, angl);
                me.IssueOrder(GameObjectOrder.MoveTo, w.wallStart.To3D(), false);
                Game.OnUpdate += Game_OnUpdate;
                Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
                turnoff = Game.ClockTime + 4;
                args.Process = false;
            }
            else if (menu.Item("bf").GetValue<bool>())
            {
                args.Process = false;
            }




        }

        static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            Obj_AI_Base.OnIssueOrder -= Obj_AI_Base_OnIssueOrder;
            Game.OnUpdate -= Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Game.ClockTime > turnoff)
            {
                Game.OnUpdate -= Game_OnUpdate; Obj_AI_Base.OnIssueOrder -= Obj_AI_Base_OnIssueOrder;
            }
            longestJump(flash, me.Position.To2D(), angl);
            t++;
        }

        private static float getAngleDiff(float angle1, float angle2)
        {
            double delta = Math.Abs(angle1 * (180.0 / Math.PI) - angle2 * (180.0 / Math.PI));
            if (180 < delta)
                delta = 360 - delta;
            return (float)(Math.Abs(Math.PI / 180) * delta);
        }

        private static float getAngleDegree(Vector2 origin, Vector2 target)
        {
            return (float)(Math.Atan2(origin.Y - target.Y, origin.X - target.X) + 1.57079633f);
        }

        private static void longestJump(Spell spell, Vector2 mypos, float angle)
        {
            float angleadd = angleres * (t % 9);
            while (true)
            {

                Vector2 DD = new Vector2(mypos.X, mypos.Y - (menu.Item("range").GetValue<Slider>().Value)).RotateAroundPoint(mypos,
                    angle + angleadd);
                Vector2 nDD = nearestFree(DD);
                if (getAngleDiff(getAngleDegree(mypos, nDD), angle) < .4f &&
                    mypos.Distance(DD) * 1.3f < mypos.Distance(nDD))
                {
                    me.Spellbook.CastSpell(flashSlot, new Vector2(mypos.X, mypos.Y - 1000).RotateAroundPoint(mypos, angle + angleadd).To3D(), false);
                    Game.OnUpdate -= Game_OnUpdate;
                    Obj_AI_Base.OnIssueOrder -= Obj_AI_Base_OnIssueOrder;
                    break;
                }

                DD = new Vector2(mypos.X, mypos.Y - (menu.Item("range").GetValue<Slider>().Value)).RotateAroundPoint(mypos,
                    angle - angleadd);
                nDD = nearestFree(DD);
                if (getAngleDiff(getAngleDegree(mypos, nDD), angle) < .4f &&
                    mypos.Distance(DD) * 1.3f < mypos.Distance(nDD))
                {
                    me.Spellbook.CastSpell(flashSlot, new Vector2(mypos.X, mypos.Y - 1000).RotateAroundPoint(mypos, angle - angleadd).To3D(), false);
                    Game.OnUpdate -= Game_OnUpdate;
                    Obj_AI_Base.OnIssueOrder -= Obj_AI_Base_OnIssueOrder;
                    break;
                }

                angleadd += angleres * 9;
                if (angleadd > skillanglemax) break;
            }
        }

        private static Vector2 nearestFree(Vector2 pos)
        {
            if (!NavMesh.GetCollisionFlags(pos.X, pos.Y).HasFlag(CollisionFlags.Wall)) return pos;
            float rotation = 0;
            float distance = 5;
            while (true)
            {
                var newpos = new Vector2(pos.X, pos.Y + distance).RotateAroundPoint(pos, rotation);
                if (!NavMesh.GetCollisionFlags(newpos.X, newpos.Y).HasFlag(CollisionFlags.Wall))
                    return newpos;
                rotation += 0.04f;
                if (!(rotation > 6.28318531)) continue;
                distance += 1f;
                rotation = 0;
            }

        }

        private static WallOut calcWall(Vector2 mypos, float angle)
        {
            WallOut ret;
            int i = 0;
            ret.thickness = 0;
            ret.lastgreen = false;
            ret.wallEnd = new Vector2();
            bool firstred = false;
            ret.wallStart = new Vector2();
            while (i < 750)
            {

                Vector2 XD = new Vector2(mypos.X, mypos.Y - i).RotateAroundPoint(mypos, angle);
                if (!NavMesh.GetCollisionFlags(XD.X, XD.Y).HasFlag(CollisionFlags.Wall))
                {
                    if (!ret.lastgreen)
                        ret.wallEnd = XD;
                    ret.lastgreen = true;
                }
                else
                {
                    if (!firstred)
                    {
                        firstred = true;
                        ret.wallStart = XD;
                    }
                    ret.thickness += distres;
                    ret.lastgreen = false;

                }
                i += distres;
            }
            return ret;
        }


    }
}