﻿using System;
using System.Collections.Generic;
using System.ComponentModel; 
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using LayoutFarm.DrawingGL;
namespace Mini2
{
    public partial class FormDev : Form
    {
        GLBitmapTexture hwBmp;
        public FormDev()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormTestWinGLControl2 form = new FormTestWinGLControl2();
            CanvasGL2d canvas = new CanvasGL2d();

            form.SetGLPaintHandler((o, s) =>
            {
                canvas.Clear(LayoutFarm.Drawing.Color.White);
                if (hwBmp == null)
                {
                    string app = Application.ExecutablePath;

                    using (Bitmap bitmap = new Bitmap("../../../Data/Textures/logo-dark.jpg"))
                    {
                        hwBmp = GLBitmapTexture.CreateBitmapTexture(bitmap);
                    }
                }
                //canvas.DrawImage(hwBmp, 10, 10);
                canvas.DrawImage(hwBmp, 300, 300, hwBmp.Width / 4, hwBmp.Height / 4);
                canvas.FillColor = LayoutFarm.Drawing.Color.DeepPink;
                canvas.DrawLine(0, 300, 500, 300);

                //-----------------------------------------------------
                canvas.FillColor = LayoutFarm.Drawing.Color.Magenta;
                //draw line test 
                canvas.DrawLine(20, 20, 600, 200);
                //-----------------------------------------------------
                //smooth with agg 

                canvas.SmoothMode = CanvasSmoothMode.AggSmooth;
                canvas.FillColor = new LayoutFarm.Drawing.Color(50, 255, 0, 0);  //  LayoutFarm.Drawing.Color.Red;
                //rect polygon
                var polygonCoords = new float[]{
                        5,300,
                        40,300,
                        50,340,
                        10f,340};
                //canvas.DrawPolygon(polygonCoords);
                //fill polygon test                
                canvas.FillPolygon(polygonCoords);

                var polygonCoords2 = new float[]{
                        5+10,300,
                        40+10,300,
                        50+10,340,
                        10f +10,340};
                canvas.FillColor = new LayoutFarm.Drawing.Color(100, 0, 255, 0);  //  L
                canvas.DrawPolygon(polygonCoords2, polygonCoords2.Length);

                int strkW = 10;
                canvas.FillColor = LayoutFarm.Drawing.Color.LightGray;

                for (int i = 1; i < 90; i += 10)
                {
                    canvas.StrokeWidth = strkW;
                    double angle = OpenTK.MathHelper.DegreesToRadians(i);
                    canvas.DrawLine(20, 400, (float)(600 * Math.Cos(angle)), (float)(600 * Math.Sin(angle)));

                    strkW--;
                    if (strkW < 1)
                    {
                        strkW = 1;
                    }
                }


                canvas.FillColor = LayoutFarm.Drawing.Color.FromArgb(150, LayoutFarm.Drawing.Color.Green);

                ////---------------------------------------------
                ////draw ellipse and circle

                canvas.StrokeWidth = 0.75f;
                canvas.DrawCircle(400, 500, 50);
                canvas.FillCircle(450, 550, 25);

                canvas.StrokeWidth = 3;
                canvas.DrawRoundRect(500, 450, 100, 100, 10, 10);


                canvas.StrokeWidth = 3;
                canvas.FillColor = LayoutFarm.Drawing.Color.FromArgb(150, LayoutFarm.Drawing.Color.Blue);

                //canvas.DrawBezierCurve(0, 0, 500, 500, 0, 250, 500, 250);
                canvas.DrawBezierCurve(120, 500 - 160, 220, 500 - 40, 35, 500 - 200, 220, 500 - 260);
                canvas.SmoothMode = CanvasSmoothMode.No;

                //canvas.DrawArc(150, 200, 300, 50, 0, 150, 150, SvgArcSize.Large, SvgArcSweep.Negative);
                canvas.DrawArc(100, 200, 300, 200, 30, 30, 50, SvgArcSize.Large, SvgArcSweep.Negative);

                canvas.FillColor = LayoutFarm.Drawing.Color.FromArgb(150, LayoutFarm.Drawing.Color.Green);
                // canvas.DrawArc(100, 200, 300, 200, 0, 100, 100, SvgArcSize.Large, SvgArcSweep.Negative);

                canvas.FillColor = LayoutFarm.Drawing.Color.FromArgb(150, LayoutFarm.Drawing.Color.Black);
                canvas.DrawLine(100, 200, 300, 200);


                //load font data
                var font = PixelFarm.Agg.Fonts.NativeFontStore.LoadFont("c:\\Windows\\Fonts\\Tahoma.ttf", 64);
                var fontGlyph = font.GetGlyph('{');
                //PixelFarm.Font2.MyFonts.SetShapingEngine();

                canvas.FillVxs(fontGlyph.flattenVxs);

                canvas.FillColor = LayoutFarm.Drawing.Color.White;
                canvas.CurrentFont = font;

                canvas.FillColor = LayoutFarm.Drawing.Color.Black;
                canvas.DrawLine(0, 200, 500, 200);

                //test Thai words
                canvas.DrawString("ดุดีดำด่าด่ำญญู", 80, 200);
                canvas.DrawString("1234567890", 80, 200);
                GLBitmapTexture bmp = GLBitmapTexture.CreateBitmapTexture(fontGlyph.glyphImage32);

                canvas.DrawImage(bmp, 50, 50);
                bmp.Dispose();

            });
            form.Show();
        }
    }
}
