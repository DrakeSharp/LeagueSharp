using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Drake.DLib
{
    internal class CursorMinimap
    {
        private readonly float[] minimapPositions;
        CursorMinimap()
        {
            Vector2 corner = Drawing.WorldToMinimap(new Vector3(-500, 15500, 0));
            minimapPositions = new float[2];
            minimapPositions[0] = corner.X;
            minimapPositions[1] = corner.Y;
        }
        public bool OnMinimap()
        {

            Vector2 cp = Utils.GetCursorPos();
            return (cp.X > minimapPositions[0] && cp.Y > minimapPositions[1]);

        }
    }
}
