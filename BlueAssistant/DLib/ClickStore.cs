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
    class ClickStore
    {
        public Vector2[] clicks{ get; private set; }
        private int tolerance;
        private float lastclick;
        public bool capture = true;
        public bool print = true;
        public ClickStore(bool print=false, int capacity = 2, int tolerance = 50)
        {
            this.print = print;
            this.tolerance = tolerance;
            clicks=new Vector2[capacity];
            Game.OnWndProc += Game_OnWndProc;
        }

        void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg!=517 || !capture || lastclick>Game.ClockTime) return;
            lastclick = Game.ClockTime + tolerance/1000f;
            for (int i = 0; i < clicks.Length-1; i++)
            {
                clicks[i] = clicks[i + 1];
            }
            clicks[clicks.Length-1]=Game.CursorPos.To2D();
            if (print)
            {
                string pr="";
                for (int i = 0; i < clicks.Length; i++)
                {
                    pr += clicks[i].ToString();
                }
                Game.PrintChat(pr);
            }

        }

    }
}
