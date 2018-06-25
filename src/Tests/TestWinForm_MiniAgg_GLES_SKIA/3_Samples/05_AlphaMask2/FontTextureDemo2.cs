﻿//BSD, 2014-present, WinterDev
//MatterHackers

#define USE_CLIPPING_ALPHA_MASK

using System;
using System.Collections.Generic;
using Mini;
using PixelFarm.Drawing.WinGdi;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.PixelProcessing;

using Typography.OpenFont;
using Typography.Rendering;
using Typography.TextLayout;
using PixelFarm.Drawing.Fonts;

namespace PixelFarm.CpuBlit.Sample_LionAlphaMask
{


    [Info(OrderCode = "00")]
    public class FontTextureDemo2 : DemoBase
    {
        AggPainter _maskBufferPainter;

        LayoutFarm.OpenFontTextService _textServices;
        BitmapFontManager<ActualBitmap> _bmpFontMx;
        SimpleFontAtlas _fontAtlas;
        RequestFont _font;
        ActualBitmap _fontBmp;
        ActualBitmap _alphaBmp;
        ActualBitmap _glyphBmp;
        float _finalTextureScale = 1;

        public FontTextureDemo2()
        {
            string glyphBmp = @"Data\tahoma -488129008.info.png";
            if (System.IO.File.Exists(glyphBmp))
            {
                _glyphBmp = DemoHelper.LoadImage(glyphBmp);
            }
            this.Width = 800;
            this.Height = 600;

        }


        public override void Init()
        {
            //steps : detail ... 
            //1. create a text service (or get it from a singleton class)       

            _textServices = new LayoutFarm.OpenFontTextService();

            //2. create manager
            _bmpFontMx = new BitmapFontManager<ActualBitmap>(
                TextureKind.StencilLcdEffect,
                _textServices,
                atlas =>
                {
                    GlyphImage totalGlyphImg = atlas.TotalGlyph;
                    return new ActualBitmap(totalGlyphImg.Width, totalGlyphImg.Height, totalGlyphImg.GetImageBuffer());
                }
            );
            _bmpFontMx.SetCurrentScriptLangs(new ScriptLang[]
            {
                ScriptLangs.Latin
            });

            //3.  
            _font = new RequestFont("tahoma", 10);
            _fontAtlas = _bmpFontMx.GetFontAtlas(_font, out _fontBmp);
        }
        //void DrawBack(Painter p)
        //{
        //    if (p is GdiPlusPainter)
        //    {
        //        return;
        //    }

        //    //
        //    AggPainter painter = (AggPainter)p;
        //    painter.Clear(Color.White);

        //    int width = painter.Width;
        //    int height = painter.Height;
        //    //change value *** 
        //    SetupMaskPixelBlender(width, height);
        //    //painter.DestBitmapBlender.OutputPixelBlender = maskPixelBlender; //change to new blender
        //    painter.DestBitmapBlender.OutputPixelBlender = maskPixelBlenderPerCompo; //change to new blender 
        //    //4.
        //    painter.FillColor = Color.Black;
        //    //this test lcd-effect => we need to draw it 3 times with different color component, on the same position
        //    //(same as we do with OpenGLES rendering surface)
        //    maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.B;
        //    maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.B;
        //    painter.FillRect(0, 0, 200, 100);
        //    maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.G;
        //    maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.G;
        //    painter.FillRect(0, 0, 200, 100);
        //    maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.R;
        //    maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.R;
        //    painter.FillRect(0, 0, 200, 100);
        //}
        bool _pixelBlenderSetup = false;

        public void DrawString(Painter p, string text, double x, double y)
        {
            if (text != null)
            {
                DrawString(p, text.ToCharArray(), 0, text.Length, x, y);
            }

        }
        public void DrawString(Painter p, char[] buffer, int startAt, int len, double x, double y)
        {
            if (p is GdiPlusPainter)
            {
                return;
            }

            AggPainter painter = (AggPainter)p;
            int width = painter.Width;
            int height = painter.Height;
            if (!_pixelBlenderSetup)
            {
                SetupMaskPixelBlender(width, height);
                _pixelBlenderSetup = true;
            }

            int j = buffer.Length;
            //create temp buffer span that describe the part of a whole char buffer
            TextBufferSpan textBufferSpan = new TextBufferSpan(buffer, startAt, len);
            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font
            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(ref textBufferSpan, _font);

            float scale = _fontAtlas.TargetTextureScale;
            int recommendLineSpacing = _fontAtlas.OriginalRecommendLineSpacing;
            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again
            y -= ((_fontAtlas.OriginalRecommendLineSpacing) * scale);

            // 
            float scaleFromTexture = _finalTextureScale;
            TextureKind textureKind = _fontAtlas.TextureKind;

            float g_x = 0;
            float g_y = 0;
            int baseY = (int)Math.Round(y);
            int n = glyphPlanSeq.len;
            int endBefore = glyphPlanSeq.startAt + n;
            //------------------------------------- 

            float acc_x = 0;
            float acc_y = 0;
            UnscaledGlyphPlanList glyphPlanList = GlyphPlanSequence.UnsafeGetInteralGlyphPlanList(glyphPlanSeq);

            int lineHeight = (int)_font.LineSpacingInPx;//temp
            //painter.DestBitmapBlender.OutputPixelBlender = maskPixelBlenderPerCompo; //change to new blender 
            painter.DestBitmapBlender.OutputPixelBlender = maskPixelBlenderPerCompo; //change to new blender  

            for (int i = glyphPlanSeq.startAt; i < endBefore; ++i)
            {
                UnscaledGlyphPlan glyph = glyphPlanList[i];
                TextureFontGlyphData glyphData;
                if (!_fontAtlas.TryGetGlyphDataByGlyphIndex(glyph.glyphIndex, out glyphData))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //-------------------------------------- 
                int srcX, srcY, srcW, srcH;
                glyphData.GetGlyphRect(out srcX, out srcY, out srcW, out srcH);

                float ngx = acc_x + (float)Math.Round(glyph.OffsetX * scale);
                float ngy = acc_y + (float)Math.Round(glyph.OffsetY * scale);
                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------
                g_x = (float)(x + (ngx - glyphData.TextureXOffset) * scaleFromTexture); //ideal x
                g_y = (float)(y + (ngy - glyphData.TextureYOffset - srcH + lineHeight) * scaleFromTexture);

                acc_x += (float)Math.Round(glyph.AdvanceX * scale);
                g_y = (float)Math.Floor(g_y);
 
                //clear with solid black color 
                //_maskBufferPainter.Clear(Color.Black);
                _maskBufferPainter.FillRect(g_x - 1, g_y - 1, srcW + 2, srcH + 2, Color.Black);
                //draw 'stencil' glyph on mask-buffer                
                _maskBufferPainter.DrawImage(_fontBmp, g_x, g_y, srcX, _fontBmp.Height - (srcY), srcW, srcH); 

                //select component to render this need to render 3 times for lcd technique
                //1. B
                maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.B;
                maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.B;
                painter.FillRect(g_x + 1, g_y, srcW, srcH);
                //2. G
                maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.G;
                maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.G;
                painter.FillRect(g_x + 1, g_y, srcW, srcH);
                //3. R
                maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.R;
                maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.R;
                painter.FillRect(g_x + 1, g_y, srcW, srcH); 
            }
        }

        void SetupMaskPixelBlender(int width, int height)
        {
            //----------
            //same size
            _alphaBmp = new ActualBitmap(width, height);
            _maskBufferPainter = AggPainter.Create(_alphaBmp, new PixelBlenderBGRA());
            _maskBufferPainter.Clear(Color.Black);
            //------------ 
            //draw glyph bmp to _alpha bmp
            //_maskBufferPainter.DrawImage(_glyphBmp, 0, 0);
            maskPixelBlender.SetMaskBitmap(_alphaBmp);
            maskPixelBlenderPerCompo.SetMaskBitmap(_alphaBmp);
        }
        [DemoConfig]
        public PixelProcessing.PixelBlenderColorComponent SelectedComponent
        {
            get
            {
                if (maskPixelBlender != null)
                {
                    return maskPixelBlender.SelectedMaskComponent;
                }
                else
                {
                    return PixelProcessing.PixelBlenderColorComponent.R;//default
                }
            }
            set
            {

                maskPixelBlender.SelectedMaskComponent = value;
                maskPixelBlenderPerCompo.SelectedMaskComponent = value;
                NeedRedraw = true;
            }
        }

        PixelBlenderWithMask maskPixelBlender = new PixelBlenderWithMask();
        PixelBlenderPerColorComponentWithMask maskPixelBlenderPerCompo = new PixelBlenderPerColorComponentWithMask();

        public override void Draw(Painter p)
        {

            p.RenderQuality = RenderQualtity.HighQuality;
            p.Orientation = DrawBoardOrientation.LeftBottom;


            //clear the image to white         
            // draw a circle
            p.Clear(Drawing.Color.White);
            p.FillColor = Color.Black;
            //-------- 



            p.FillColor = Color.Green;
            DrawString(p, "Hello World", 10, 20);

            p.FillColor = Color.Blue;
            DrawString(p, "Hello World", 10, 40);

            p.FillColor = Color.Red;
            DrawString(p, "Hello World", 10, 60);

            p.FillColor = Color.Yellow;
            DrawString(p, "Hello World", 10, 80);

            p.FillColor = Color.Gray;
            DrawString(p, "Hello World", 10, 100);


            p.FillColor = Color.Black;
            DrawString(p, "Hello World", 10, 120);
        }




    }


}
