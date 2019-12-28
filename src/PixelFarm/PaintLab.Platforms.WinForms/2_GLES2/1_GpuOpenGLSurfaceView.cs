﻿//MIT, 2014-present, WinterDev
using System;
using System.Windows.Forms;
using OpenTK;

namespace LayoutFarm.UI.OpenGL
{
    //app specific
    public partial class GpuOpenGLSurfaceView : OpenTK.GLControl, IGpuOpenGLSurfaceView
    {
        //static OpenTK.Graphics.GraphicsMode s_gfxmode = new OpenTK.Graphics.GraphicsMode(
        //    DisplayDevice.Default.BitsPerPixel,//default 32 bits color
        //    16,//depth buffer => 16
        //    8, //stencil buffer => 8 (set this if you want to use stencil buffer toos)
        //    0, //number of sample of FSAA (not always work)
        //    0, //accum buffer
        //    2, // n buffer, 2=> double buffer
        //    false);//sterio 

        MyTopWindowBridgeOpenGL _winBridge;
        public GpuOpenGLSurfaceView()
        { 

        }
        PixelFarm.Drawing.Size IGpuOpenGLSurfaceView.GetSize() => new PixelFarm.Drawing.Size(Width, Height);
        //----------------------------------------------------------------------------
        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public void Bind(MyTopWindowBridgeOpenGL winBridge)
        {
            //1. 
            _winBridge = winBridge;
            _winBridge.BindWindowControl(this);
        }
        //----------------------------------------------------------------------------
        protected override void OnSizeChanged(EventArgs e)
        {
            if (_winBridge != null)
            {
                _winBridge.UpdateCanvasViewportSize(this.Width, this.Height);
            }
            base.OnSizeChanged(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_winBridge != null)
            {
                _winBridge.PaintToOutputWindow(e.ClipRectangle.ToRect());
            }
            base.OnPaint(e);
        }
        //-----------------------------------------------------------------------------
        protected override void OnGotFocus(EventArgs e)
        {
            _winBridge.HandleGotFocus(e);
            base.OnGotFocus(e);

        }
        protected override void OnLostFocus(EventArgs e)
        {
            _winBridge.HandleLostFocus(e);
            base.OnLostFocus(e);
        }
        //-----------------------------------------------------------------------------
        protected override void OnMouseEnter(EventArgs e)
        {
            _winBridge.HandleMouseEnterToViewport();
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            _winBridge.HandleMouseLeaveFromViewport();
            base.OnMouseLeave(e);
        }
        //
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _winBridge.HandleMouseDown(e);
            base.OnMouseDown(e);

        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _winBridge.HandleMouseMove(e);
            base.OnMouseMove(e);

        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _winBridge.HandleMouseUp(e);
            base.OnMouseUp(e);

        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            _winBridge.HandleMouseWheel(e);
            base.OnMouseWheel(e);
        }
        //-----------------------------------------------------------------------------
        protected override void OnKeyDown(KeyEventArgs e)
        {
            _winBridge.HandleKeyDown(e);
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            _winBridge.HandleKeyUp(e);
            base.OnKeyUp(e);
        }
        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            _winBridge.HandleKeyPress(e);
            return;
        }
        protected override bool ProcessDialogKey(System.Windows.Forms.Keys keyData)
        {
            if (_winBridge.HandleProcessDialogKey(keyData))
            {
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}
