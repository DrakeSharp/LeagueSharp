using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Drake.DLib
{
    internal static class DUtility
    {
        public static int version
        {
            get { return 2; }
        }
        private static int counter;
        private static float lastminionscoming;
        private static float lastavgdist;
        private static int lastminioncount;
        private static bool minionscomin;

        public static string dir
        {
            get
            {
                string folder = Config.AppDataDirectory + @"\Drake\";
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public static void DrawLine(Vector3 start, Vector3 end, int thickness, Color color)
        {
            Drawing.DrawLine(Drawing.WorldToScreen(start), Drawing.WorldToScreen(end), thickness, color);
        }
        public static Rect BestClear(Vector3 position, float range, float length, float width)
        {
            Rect ret=new Rect(new Vector2(), new Vector2(),0 );
            int maxcount = 0;
            var minions = MinionManager.GetMinions(position, range, MinionTypes.All, MinionTeam.All);
            var k = minions.Where(mini => mini.IsEnemy&& ObjectManager.Player.Distance(mini)<900);
            foreach (var minion in k)
            {
                foreach (var minion2 in k)
                {
                    var l = minion.Position.To2D().CalcLineV2(minion2.Position.To2D(), length);
                    var a = new Rect(l[0], l[1], width);
                    var c = k.Count(min => a.Distance(min.Position.To2D()) < 30);
                    if (c > maxcount)
                    {
                        maxcount = c;
                        ret = a;
                    }
                }

            }
            //var lel = ClearEffeciency(minions, maxcount);
            //E.st
            //if (lel > 65)
            //{
            //    int lol=1;
            //}
            return ret;
        }
        public static int ClearEffeciency(List<Obj_AI_Base> minionz, int inrect)
        {
            int ret = 0;


            if (Game.ClockTime > lastminionscoming)
                minionscoming(minionz, 900);
            if (minionz.Where(mini => mini.IsAlly && HealthPrediction.GetHealthPrediction(mini, 300) < 30).Any())
            {
                
                return 0;
            }
            int enemyminions = 0;
            foreach (var minio in minionz)
            {
                if (minio.IsAlly && HealthPrediction.GetHealthPrediction(minio, 300) < minio.MaxHealth*.2f) return 0;
                if (minio.IsEnemy && ObjectManager.Player.Distance(minio) < 900) enemyminions++;
            }
            if (enemyminions == 0) return 0;
            if (minionscomin) ret -= 20;
            return ((100*inrect)/enemyminions)+   ret;
        }
        public static void minionscoming(List<Obj_AI_Base> minionz, float distance)
        {

            float avgdistance = 0;
            int minioncount = 0;
            foreach (var mini in minionz)
            {
                if (mini.IsEnemy&&mini.Distance(ObjectManager.Player) > distance)
                {
                    avgdistance += mini.Distance(ObjectManager.Player);
                    minioncount++;
                }
            }
            avgdistance /= minioncount;
            if (Game.ClockTime - lastminionscoming > .5f)
            {
                lastminionscoming = Game.ClockTime + .3f;
                lastminioncount = minioncount;
                lastavgdist = avgdistance;
                return;
            }
            bool minionscominn = avgdistance+50 < lastavgdist || lastminioncount > minioncount;
            if (minionscominn)
            {
                minionscomin = true;
                counter = 0;
            }
            else
            {
                counter++;
            }
            if (counter > 2)
                minionscomin = false;
            lastminioncount = minioncount;
            lastavgdist = avgdistance;
            lastminionscoming = Game.ClockTime + .3f;
        }

        public static void DrawLine(Vector3 start, Vector3 end)
        {
            Drawing.DrawLine(Drawing.WorldToScreen(start), Drawing.WorldToScreen(end), 2, Color.FloralWhite);
        }
        public static float GetAngleDiff(float angle1, float angle2)
        {
            double delta = Math.Abs(angle1 * (180.0 / Math.PI) - angle2 * (180.0 / Math.PI));
            if (180 < delta)
                delta = 360 - delta;
            return (float)(Math.Abs(Math.PI / 180) * delta);
        }

        private static float Angle(this Vector2 origin, Vector2 target)
        {
            return (float)(Math.Atan2(origin.Y - target.Y, origin.X - target.X));
        }
        public static Vector2 NearestFree(this Vector2 pos, float angleres = .04f, float distres = 1f)
        {
            if (!NavMesh.GetCollisionFlags(pos.X, pos.Y).HasFlag(CollisionFlags.Wall)) return pos;
            float rotation = 0;
            float distance = 5;
            while (true)
            {
                var newpos = new Vector2(pos.X, pos.Y + distance).RotateAroundPoint(pos, rotation);
                if (!NavMesh.GetCollisionFlags(newpos.X, newpos.Y).HasFlag(CollisionFlags.Wall))
                    return newpos;
                rotation += angleres;
                if (!(rotation > 6.28318531)) continue;
                distance += distres;
                rotation = 0;
            }

        }
        public static float[] CalcLine(this Vector2 c1, Vector2 c2, float length)
        {
            float angle = Angle(c1, c2);
            Vector2 middle = new Vector2((c1.X + c2.X) / 2, (c1.Y + c2.Y) / 2);
            Vector2 up = new Vector2(middle.X - length / 2, middle.Y).RotateAroundPoint(middle, angle);
            Vector2 down = new Vector2(middle.X + length / 2, middle.Y).RotateAroundPoint(middle, angle);
            return new[] { up.X, up.Y, down.X, down.Y };
        }
        public static Vector2[] CalcLineV2(this Vector2 c1, Vector2 c2, float length)
        {
            float angle = Angle(c1, c2);
            Vector2 middle = new Vector2((c1.X + c2.X) / 2, (c1.Y + c2.Y) / 2);
            Vector2 up = new Vector2(middle.X - length / 2, middle.Y).RotateAroundPoint(middle, angle);
            Vector2 down = new Vector2(middle.X + length / 2, middle.Y).RotateAroundPoint(middle, angle);
            return new[] { up, down };
        }
        public static bool Compare(this Vector2 c1, Vector2 c2, float tolerance=1)
        {
            if (Math.Abs(c1.X - c2.X) > tolerance) return false;
            return (Math.Abs(c1.Y - c2.Y) < tolerance);
        }
        public static bool Compare(this Vector3 c1, Vector3 c2, float tolerance = 1)
        {
            if (Math.Abs(c1.Z - c2.Z) > tolerance) return false;
            if (Math.Abs(c1.X - c2.X) > tolerance) return false;
            return (Math.Abs(c1.Y - c2.Y) < tolerance);
        }
        public static bool IsLeft(this Vector2 pointPosition, Vector2 leftLower, Vector2 rightUpper)
        {
            return ((rightUpper.X - leftLower.X) * (pointPosition.Y - leftLower.Y) - (rightUpper.Y - leftLower.Y) * (pointPosition.X - leftLower.X)) > 0;
        }
        static float DotProduct(float[] pointA, float[] pointB, float[] pointC)
        {
            float[] AB = new float[2];
            float[] BC = new float[2];
            AB[0] = pointB[0] - pointA[0];
            AB[1] = pointB[1] - pointA[1];
            BC[0] = pointC[0] - pointB[0];
            BC[1] = pointC[1] - pointB[1];
            float dot = AB[0] * BC[0] + AB[1] * BC[1];

            return dot;
        }

        public static float CrossProduct(float[] pointA, float[] pointB, float[] pointC)
        {
            float[] AB = new float[2];
            float[] AC = new float[2];
            AB[0] = pointB[0] - pointA[0];
            AB[1] = pointB[1] - pointA[1];
            AC[0] = pointC[0] - pointA[0];
            AC[1] = pointC[1] - pointA[1];
            float cross = AB[0] * AC[1] - AB[1] * AC[0];

            return cross;
        }

        public static float Distance(float[] pointA, float[] pointB)
        {
            float d1 = pointA[0] - pointB[0];
            float d2 = pointA[1] - pointB[1];

            return (float)Math.Sqrt(d1 * d1 + d2 * d2);
        }

        static float LineToPointDistance2D(float[] pointA, float[] pointB, float[] pointC,
            bool isSegment)
        {
            float dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                float dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                float dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }
            return Math.Abs(dist);
        }
        public static float LineToPointDistance2D(Vector2 pointA, Vector2 pointB, Vector2 pointC,
            bool isSegment=true )
        {
            return LineToPointDistance2D(new[] { pointA.X, pointA.Y }, new[] { pointB.X, pointB.Y }, new[] { pointC.X, pointC.Y }, isSegment);
        } 
        public static Vector2 Normal(Vector2 p1, Vector2 p2)
        {

            return new Vector2(-(p2.Y - p1.Y), p2.X - p1.X);
        }
        public static Vector2[] TranslateLine(Vector2 p1, Vector2 p2, float distance)
        {
            Vector2 normal = Normal(p1, p2);
            normal.Normalize();
            return new[] { p1 + normal * distance, p2 + normal * distance };
        }
        public static int Distance(this string original, string modified)
        {
            int len_orig = original.Length;
            int len_diff = modified.Length;

            var matrix = new int[len_orig + 1, len_diff + 1];
            for (int i = 0; i <= len_orig; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= len_diff; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= len_orig; i++)
            {
                for (int j = 1; j <= len_diff; j++)
                {
                    int cost = modified[j - 1] == original[i - 1] ? 0 : 1;
                    var vals = new int[] {
				matrix[i - 1, j] + 1,
				matrix[i, j - 1] + 1,
				matrix[i - 1, j - 1] + cost
			};
                    matrix[i, j] = vals.Min();
                    if (i > 1 && j > 1 && original[i - 1] == modified[j - 2] && original[i - 2] == modified[j - 1])
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost);
                }
            }
            return matrix[len_orig, len_diff] - (len_orig - len_diff);
        }

        public static string RemoveLast(this string str)
        {
            if(str.Length>0)
                str = str.Remove(str.Length - 1);
            return str;
        }
    }
}
