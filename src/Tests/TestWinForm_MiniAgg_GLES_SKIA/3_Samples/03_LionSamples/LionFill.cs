//BSD, 2014-2018, WinterDev

/*
Copyright (c) 2013, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

using System;
using Mini;
namespace PixelFarm.Agg.Samples
{
    [Info(OrderCode = "03")]
    [Info("Affine transformer, and basic renderers. You can rotate and scale the �Lion� with the"
      + " left mouse button. Right mouse button adds �skewing� transformations, proportional to the �X� "
      + "coordinate. The image is drawn over the old one with a cetrain opacity value. Change �Alpha� "
      + "to draw funny looking �lions�. Change window size to clear the window.")]
    public class LionFillExample : DemoBase
    {
        LionFillSprite lionFill;
        public override void Init()
        {
            lionFill = new LionFillSprite();
            //lionFill.AutoFlipY = true;           
        }

        public override void Draw(PixelFarm.Drawing.Painter p)
        {
            p.Clear(Drawing.Color.White);

            if (UseBitmapExt)
            {
                p.RenderQuality = Drawing.RenderQualtity.Fast;
            }
            else
            {
                p.RenderQuality = Drawing.RenderQualtity.HighQuality;
            }

            lionFill.Draw(p);
        }
        public override void MouseDrag(int x, int y)
        {
            lionFill.Move(x, y);
        }
        [DemoConfig]
        public bool UseBitmapExt
        {
            get;
            set;
        }
        [DemoConfig(MaxValue = 20)]
        public int SharpRadius
        {
            //test
            get { return lionFill.SharpenRadius; }
            set { lionFill.SharpenRadius = value; }

        }
        [DemoConfig(MaxValue = 255)]
        public int AlphaValue
        {
            get { return lionFill.AlphaValue; }
            set
            {
                lionFill.AlphaValue = (byte)value;
            }
        }
    }



    public enum LionMoveOption
    {
        ZoomAndRotate,
        Move,
    }
    //----------
    [Info(OrderCode = "03.1")]
    [Info("HitTest and Selection")]
    public class LionFillExample_HitTest : DemoBase
    {
        LionFillSprite lionFill;
        bool hitOnLion;
        public override void Init()
        {
            lionFill = new LionFillSprite();
            //lionFill.AutoFlipY = true;           
        }

        public override void Draw(PixelFarm.Drawing.Painter p)
        {
            p.Clear(Drawing.Color.White);

            if (UseBitmapExt)
            {
                p.RenderQuality = Drawing.RenderQualtity.Fast;
            }
            else
            {
                p.RenderQuality = Drawing.RenderQualtity.HighQuality;
            }

            lionFill.Draw(p);
        }
        public override void MouseDown(int x, int y, bool isRightButton)
        {

            //check if we hit a lion or not 
            hitOnLion = lionFill.HitTest(x, y);
            base.MouseDown(x, y, isRightButton);
        }
        public override void MouseUp(int x, int y)
        {
            hitOnLion = false;
            base.MouseUp(x, y);
        }
        public override void MouseDrag(int x, int y)
        {
            if (hitOnLion)
            {
                lionFill.Move(x, y);
            }
        }
        [DemoConfig]
        public bool UseBitmapExt
        {
            get;
            set;
        }

        LionMoveOption _moveOption;
        [DemoConfig]
        public LionMoveOption LionMoveOption
        {
            get
            {
                return _moveOption;
            }
            set
            {
                switch (_moveOption = value)
                {
                    default: break;
                    case LionMoveOption.Move:
                        lionFill.JustMove = true;
                        break;
                    case LionMoveOption.ZoomAndRotate:
                        lionFill.JustMove = false;
                        break;
                }
            }
        }
        //[DemoConfig(MaxValue = 20)]
        //public int SharpRadius
        //{
        //    //test
        //    get { return lionFill.SharpenRadius; }
        //    set { lionFill.SharpenRadius = value; }

        //}
        //[DemoConfig(MaxValue = 255)]
        //public int AlphaValue
        //{
        //    get { return lionFill.AlphaValue; }
        //    set
        //    {
        //        lionFill.AlphaValue = (byte)value;
        //    }
        //}
    }

}

