﻿//MIT, 2014-2016, WinterDev   

using System;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing.Fonts;

namespace Win32
{

    class WinGdiFont : ActualFont
    {
        float emSize;
        float emSizeInPixels;
        float ascendInPixels;
        float descentInPixels;

        static BasicGdi32FontHelper basGdi32FontHelper = new BasicGdi32FontHelper();

        int[] charWidths;
        Win32.NativeTextWin32.FontABC[] charAbcWidths;
        FontGlyph[] fontGlyphs; 

        IntPtr memHdc;
        IntPtr dib;
        IntPtr ppvBits;
        IntPtr hfont;
        int bmpWidth = 200;
        int bmpHeight = 50;

        //eg.
        Encoding fontEncoding = Encoding.GetEncoding(874);
        PixelFarm.Drawing.Font f;
        public WinGdiFont(PixelFarm.Drawing.Font f)
        {
            this.f = f; 
            bmpWidth = 10;
            bmpHeight = 10;
            memHdc = Win32.Win32Utils.CreateMemoryHdc(
                IntPtr.Zero,
                bmpWidth,
                bmpHeight,
                out dib,
                out ppvBits);

            //this will create 
            InitFont(f.Name, (int)f.EmSize);
            Win32.MyWin32.SetTextColor(memHdc, 0);


            //create gdi font from font data
            this.emSize = f.EmSize;
            this.emSizeInPixels = PixelFarm.Drawing.Font.ConvEmSizeInPointsToPixels(this.emSize);
            ////
            ////build font matrix
            basGdi32FontHelper.MeasureCharWidths(hfont, out charWidths, out charAbcWidths);
            //int emHeightInDzUnit = f.FontFamily.GetEmHeight(f.Style);

            //this.ascendInPixels = Font.ConvEmSizeInPointsToPixels((f.FontFamily.GetCellAscent(f.Style) / emHeightInDzUnit));
            //this.descentInPixels = Font.ConvEmSizeInPointsToPixels((f.FontFamily.GetCellDescent(f.Style) / emHeightInDzUnit));

            ////--------------
            ////we build font glyph, this is just win32 glyph
            ////
            //int j = charAbcWidths.Length;
            //fontGlyphs = new FontGlyph[j];
            //for (int i = 0; i < j; ++i)
            //{
            //    FontGlyph glyph = new FontGlyph();
            //    glyph.horiz_adv_x = charWidths[i] << 6;
            //    fontGlyphs[i] = glyph;
            //}

        }
        void InitFont(string fontName, int emHeight)
        {
            Win32.MyWin32.LOGFONT logFont = new Win32.MyWin32.LOGFONT();
            Win32.MyWin32.SetFontName(ref logFont, fontName);
            logFont.lfHeight = emHeight;
            logFont.lfCharSet = 1;//default
            logFont.lfQuality = 0;//default
            hfont = Win32.MyWin32.CreateFontIndirect(ref logFont);
            Win32.MyWin32.SelectObject(memHdc, hfont);
        }

        public System.IntPtr ToHfont()
        {   /// <summary>
            /// Set a resource (e.g. a font) for the specified device context.
            /// WARNING: Calling Font.ToHfont() many times without releasing the font handle crashes the app.
            /// </summary>
            return this.hfont;
        }
        public override float EmSize
        {
            get { return emSize; }
        }
        public override float EmSizeInPixels
        {
            get { return emSizeInPixels; }
        }
        protected override void OnDispose()
        {

            //TODO: review here 
            Win32.Win32Utils.DeleteObject(dib);
            Win32.Win32Utils.DeleteObject(hfont);
            Win32.Win32Utils.DeleteDC(memHdc);
            dib = IntPtr.Zero;
            hfont = IntPtr.Zero;
            memHdc = IntPtr.Zero;

        }
        public override FontGlyph GetGlyphByIndex(uint glyphIndex)
        {
            throw new NotImplementedException();
        }
        public override FontGlyph GetGlyph(char c)
        {
            //convert c to glyph index
            //temp fix  
            throw new NotImplementedException();
        }

        public override void GetGlyphPos(char[] buffer, int start, int len, ProperGlyph[] properGlyphs)
        {
            //get gyph pos
            throw new NotImplementedException();
        }

        char[] singleCharArray = new char[1];
        byte[] codePoints = new byte[2];

        public override float GetAdvanceForCharacter(char c)
        {
            //check if we have width got this char or not
            //temp fix
            //TODO: review here again ***
            singleCharArray[0] = c;
            fontEncoding.GetBytes(singleCharArray, 0, 1, codePoints, 0);
            return charWidths[codePoints[0]];
        }

        public override float GetAdvanceForCharacter(char c, char next_c)
        {
            throw new NotImplementedException();
        }



        public override FontFace FontFace
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public override float AscentInPixels
        {
            get
            {
                return ascendInPixels;
            }
        }

        public override float DescentInPixels
        {
            get
            {
                return descentInPixels;
            }
        }



    }
}