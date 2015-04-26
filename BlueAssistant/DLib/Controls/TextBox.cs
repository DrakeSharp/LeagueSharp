using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.XInput;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace Drake.DLib.Controls
{

    class TextBox : Control
    {
        public event TextChanged textchanged;
        public event Focus focus;
        public delegate void TextChanged(TextBox t);
        public delegate void Focus(TextBox t, bool focused);
        private float time;
        private bool cursorActive;
        private bool active;
        private int width;
        private Render.Rectangle rect;
        private Render.Rectangle curs;
        private Color color;
        private int x;
        private int y;
        private int strx;
        private int stry;
        private string value;
        public TextBox(int x, int y, string value, Color color, int width=200)
        {
            this.width = width;
            rect = new Render.Rectangle(x, y, width, 20, new ColorBGRA(.4f, .4f, .4f,.5f));
            curs = new Render.Rectangle(x+1, y+2, 1, 16, new ColorBGRA(1f, 1f, 1f, 1f));
            rect.Layer = 1;
            SetPosition(x, y);
            SetText(value);
            this.color = color;
            Drawing.OnDraw += Drawing_OnDraw;
            

        }



        public TextBox(int x, int y, string value) : this(x, y, value, Color.White) { }

        public void SetColor(Color color)
        {
            this.color = color;
        }
        public void SetWidth(int width)
        {
            this.width = width;
            rect.Width = width;
        }
        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
            rect.X = x;
            rect.Y = y;
            stry = y + 2;
            strx = x + 3;
        }
        public void SetText(string value)
        {
            using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                SizeF size = graphics.MeasureString(value + "i", new Font("Verdana", 10, FontStyle.Bold, GraphicsUnit.Point));
                int offset = -5 + (int)(size.Width * .945);
                if (offset > width - 3)
                {
                    Game.PrintChat("TextBox error:string too long");
                    return;
                }
                    curs.X = offset + x;
                this.value = value;
            }
        }
        public void Show()
        {
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
            rect.Add();
        }
        public void Hide()
        {
            rect.Remove();
            curs.Remove();
            cursorActive = false;
            active = false;
            Drawing.OnDraw -= Drawing_OnDraw;
            Game.OnWndProc -= Game_OnWndProc;
            Game.OnInput -= Game_OnInput;
            Spellbook.OnCastSpell -= Spellbook_OnCastSpell;
            Obj_AI_Base.OnIssueOrder -= Obj_AI_Base_OnIssueOrder;
        }
        private void Blink()
        {
            if (!active||Game.ClockTime < time) return;
            time = Game.ClockTime + .5f;
            curs.Remove();
            cursorActive = !cursorActive;
            if (cursorActive&&active)
                curs.Add();
            else
                curs.Remove();
        }
        private void MouseIn(WndEventArgs args)
        {
            Point point = new Point(args.LParam);
            if (point.X >= x && point.X <= x + width && point.Y >= y && point.Y <= y + 20)
            {
                if (active) return;
                active = true;
                time = 0;
                Game.OnInput += Game_OnInput;
                Spellbook.OnCastSpell += Spellbook_OnCastSpell;
                Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
                focus(this, true);
            }
            else if(active)
            {
                cursorActive = false;
                active = false;
                curs.Remove();
                Game.OnInput -= Game_OnInput;
                Spellbook.OnCastSpell -= Spellbook_OnCastSpell;
                Obj_AI_Base.OnIssueOrder -= Obj_AI_Base_OnIssueOrder;
                focus(this, false);
            }
        }
        private void Drawing_OnDraw(EventArgs args)
        {
            Blink();
            Drawing.DrawText(strx, stry, color, value);
        }
        void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 513)
                MouseIn(args);
            if (!active) return;
            if (args.Msg != 258) return;
            if (args.WParam >= 32 && args.WParam <= 126)
                value += (char)args.WParam;
            else if (args.WParam == 8)
                value = value.RemoveLast();
            using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                SizeF size = graphics.MeasureString(value + "i", new Font("Verdana", 10, FontStyle.Bold, GraphicsUnit.Point));
                int offset = -5 + (int)(size.Width * .945);
                if (offset > width - 3)
                    value = value.RemoveLast();
                else
                {
                    curs.X = offset + x;
                    textchanged(this);
                }
            }
        }
        void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
                args.Process = false;
        }
        void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
                args.Process = false;
        }
        void Game_OnInput(GameInputEventArgs args)
        {
                args.Process = false;
        }
    }
}
