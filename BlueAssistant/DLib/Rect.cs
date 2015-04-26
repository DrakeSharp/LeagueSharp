using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Drake.DLib
{
    class Rect
    {
        public Vector2[] rekt;
        public int minionsin = 0;
        public Rect(Vector2 start, Vector2 end, float width)
        {
            rekt=new Vector2[6];
            var l = DUtility.TranslateLine(start, end, width/2);
            var r = DUtility.TranslateLine(start, end, -width / 2);
            rekt[0] = l[0];
            rekt[1] = l[1];
            rekt[2] = r[0];
            rekt[3] = r[1];
            rekt[4] = start;
            rekt[5] = end;
        }

        public void Draw(int thickness, Color color)
        {

            DUtility.DrawLine(rekt[0].To3D(), rekt[1].To3D(), thickness, color);
            DUtility.DrawLine(rekt[1].To3D(), rekt[3].To3D(), thickness, color);
            DUtility.DrawLine(rekt[2].To3D(), rekt[3].To3D(), thickness, color);
            DUtility.DrawLine(rekt[0].To3D(), rekt[2].To3D(), thickness, color);
        }
        public void DrawRainbow(int thickness)
        {

            DUtility.DrawLine(rekt[0].To3D(), rekt[1].To3D(), thickness, Color.Red);
            DUtility.DrawLine(rekt[1].To3D(), rekt[3].To3D(), thickness, Color.Green);
            DUtility.DrawLine(rekt[2].To3D(), rekt[3].To3D(), thickness, Color.Blue);
            DUtility.DrawLine(rekt[0].To3D(), rekt[2].To3D(), thickness, Color.Yellow);

            DUtility.DrawLine(rekt[0].To3D(), rekt[3].To3D(), thickness, Color.Purple);
            DUtility.DrawLine(rekt[1].To3D(), rekt[2].To3D(), thickness, Color.SaddleBrown);




        }
        public void Draw(int thickness = 4)
        {
            Draw(thickness, Color.RoyalBlue);
        }
        public float Distance(Vector2 p)
        {
            if(Inside(p)) return 0;
            if (ObjectManager.Player.Position.To2D().IsLeft(rekt[0], rekt[3]))
                return ObjectManager.Player.Position.To2D().IsLeft(rekt[1], rekt[2]) ? DUtility.LineToPointDistance2D(rekt[1], rekt[3], p) : DUtility.LineToPointDistance2D(rekt[0], rekt[1], p);
            return ObjectManager.Player.Position.To2D().IsLeft(rekt[1], rekt[2]) ? DUtility.LineToPointDistance2D(rekt[2], rekt[3], p) : DUtility.LineToPointDistance2D(rekt[0], rekt[2], p);
        }

        public bool Inside(Vector2 p)
        {
            if (p.IsLeft(rekt[0], rekt[1])) return false;
            if (p.IsLeft(rekt[1], rekt[3])) return false;
            if (p.IsLeft(rekt[3], rekt[2])) return false;
            return !p.IsLeft(rekt[2], rekt[0]);
        }
    }
}
