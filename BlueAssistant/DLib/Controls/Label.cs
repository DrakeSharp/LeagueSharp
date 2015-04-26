using System;
using System.Drawing;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Direct3D9;

namespace Drake.DLib.Controls
{
    class Label : Control
    {
        private Color color;
        private int x;
        private int y;
        private string value;

        public Label(int x, int y, string value, Color color)
        {
            SetPosition(x, y);
            SetText(value);
            SetColor(color);
            Drawing.OnDraw += Drawing_OnDraw;
        }
        public Label(int x, int y, string value) : this(x, y, value, Color.PapayaWhip) { }

        public void SetColor(Color color)
        {
            this.color = color;
        }
        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public void SetText(string value)
        {
            this.value = value;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(x, y, color, value);
        }
        public void Show()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }
        public void Hide()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }
    }
}
