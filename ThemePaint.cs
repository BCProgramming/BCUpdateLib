using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BASeCamp.Updating
{
    public class ThemePaint
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr OpenThemeData(IntPtr hWnd, String classList);
        [DllImport("uxtheme", ExactSpelling = true)]
        private extern static Int32 DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int iPartId,
           int iStateId, ref RECT pRect, IntPtr pClipRect);
        [DllImport("uxtheme.dll", ExactSpelling = true)]
        public extern static Int32 CloseThemeData(IntPtr hTheme);

        public static Brush ProgressBarEmptyBackground = new SolidBrush(SystemColors.Window);
        public static Brush ProgressBarFilledBackground = new SolidBrush(Color.Teal);

        public enum PROGRESSPARTS
        {
            PP_BAR = 1,
            PP_BARVERT = 2,
            PP_CHUNK = 3,
            PP_CHUNKVERT = 4,
            PP_FILL = 5,
            PP_FILLVERT = 6,
            PP_PULSEOVERLAY = 7,
            PP_MOVEOVERLAY = 8,
            PP_PULSEOVERLAYVERT = 9,
            PP_MOVEOVERLAYVERT = 10,
            PP_TRANSPARENTBAR = 11,
            PP_TRANSPARENTBARVERT = 12,
        };
        public enum TRANSPARENTBARSTATES
        {
            PBBS_NORMAL = 1,
            PBBS_PARTIAL = 2,
        };
        public enum FILLSTATES
        {
            PBFS_NORMAL = 1,
            PBFS_ERROR = 2,
            PBFS_PAUSED = 3,
            PBFS_PARTIAL = 4,
        };

        public static void DrawProgress(IntPtr hWnd, Graphics Target, Rectangle Size, float Percentage, FILLSTATES FillState = FILLSTATES.PBFS_NORMAL)
        {
            Rectangle ValueRect = new Rectangle(Size.Location, new Size((int)((float)Size.Width * Percentage), Size.Height));

            IntPtr Themehandle = OpenThemeData(hWnd, "PROGRESS");
            if (Themehandle != IntPtr.Zero)
            {
                IntPtr hdc = Target.GetHdc();
                //draw the full size.
                RECT FullSize = Size;
                RECT ValueSize = ValueRect;
                //draw the progressbar background...
                DrawThemeBackground(Themehandle, hdc, (int)(PROGRESSPARTS.PP_TRANSPARENTBAR), (int)TRANSPARENTBARSTATES.PBBS_PARTIAL, ref FullSize, IntPtr.Zero);
                //now draw the foreground...
                DrawThemeBackground(Themehandle, hdc, (int)(PROGRESSPARTS.PP_FILL), (int)(FILLSTATES.PBFS_NORMAL), ref ValueSize, IntPtr.Zero);
                DrawThemeBackground(Themehandle, hdc, (int)(PROGRESSPARTS.PP_PULSEOVERLAY), 0, ref FullSize, IntPtr.Zero);


                //DrawThemeBackground(Themehandle, hdc, 8, 6, ref FullSize, IntPtr.Zero);
                CloseThemeData(Themehandle);
                Target.ReleaseHdc(hdc);
            }
            else
            {
                Target.FillRectangle(ProgressBarEmptyBackground, Size);
                Target.FillRectangle(ProgressBarFilledBackground, ValueRect);
            }
        }
        /*
         * 
         * PAINTSTRUCT ps;
HDC hDC = BeginPaint(hwnd,&ps);
RECT r;
HTHEME theme = OpenThemeData(hwnd,L"PROGRESS");
SetRect(&r,10,10,100,25);
DrawThemeBackground(theme,hDC,11, 2 ,&r,NULL);
SetRect(&r,10,10,50,25);
DrawThemeBackground(theme,hDC,5, 4 ,&r,NULL);
CloseThemeData(theme);
EndPaint(hwnd,&ps);*/



    }
}
