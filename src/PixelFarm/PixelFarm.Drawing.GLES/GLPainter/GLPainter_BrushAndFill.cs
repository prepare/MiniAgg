﻿//MIT, 2016-present, WinterDev
//Apache2, https://xmlgraphics.apache.org/

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.DrawingGL
{
    partial class GLPainter
    {
        Color _fillColor;
        Brush _currentBrush;
        Brush _defaultBrush;
        float _fillOpacity;
        bool _hasFillOpacity;

        public override Color FillColor
        {
            get => _fillColor;
            set => _fillColor = value;
        }
        public override FillingRule FillingRule
        {
            //TODO: implement filling rule for GL
            //this need to change to tess level
            get => _pcx.FillingRule;
            set => _pcx.FillingRule = value;
        }
        public override float FillOpacity
        {
            get => _fillOpacity;
            set
            {
                //apply to all brush

                _fillOpacity = value;
                if (value < 0)
                {
                    _fillOpacity = 0;
                    _hasFillOpacity = true;
                }
                else if (value >= 1)
                {
                    _fillOpacity = 1;
                    _hasFillOpacity = false;
                }
                else
                {
                    _fillOpacity = value;
                    _hasFillOpacity = true;
                }
            }
        }
        public override Brush CurrentBrush
        {
            get => _currentBrush;
            set
            {
                //brush with its detail             
                //------------------------
                if (value == null)
                {
                    _currentBrush = _defaultBrush;
                    return;
                }
                //
                //
                switch (value.BrushKind)
                {
                    default:
                        break;
                    case BrushKind.Solid:
                        {
                            SolidBrush solidBrush = (SolidBrush)value;
                            _fillColor = solidBrush.Color;
                        }
                        break;
                    case BrushKind.LinearGradient:
                        {
                        }
                        break;
                    case BrushKind.CircularGradient:
                        break;

                    case BrushKind.Texture:

                        break;
                }
                _currentBrush = value;
            }

        }

        //
        public bool UseTwoColorsMask
        {
            get => _pcx.UseTwoColorsMask;
            set => _pcx.UseTwoColorsMask = value;
        }

        public override void Fill(VertexStore vxs)
        {


            using (PathRenderVx pathRenderVx = PathRenderVx.Create(_pathRenderVxBuilder.Build(vxs)))
            {
                switch (_currentBrush.BrushKind)
                {
                    default:
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("unknown brush!");
#endif
                        break;
                    case BrushKind.Texture:
                    case BrushKind.CircularGradient:
                    case BrushKind.LinearGradient:
                    case BrushKind.PolygonGradient:

                        //resolve internal linear gradient brush impl
                        _pcx.FillGfxPath(_currentBrush, pathRenderVx);
                        break;

                    case BrushKind.Solid:
                        {
                            TextureRenderVx textureRenderVx = null;//TODO: review, not finish
                            if (textureRenderVx != null)
                            {
                                //review this again!                                
                                int ox = _pcx.OriginX;
                                int oy = _pcx.OriginY;
                                _pcx.SetCanvasOrigin(-(int)textureRenderVx.SpriteSource.TextureXOffset, -(int)textureRenderVx.SpriteSource.TextureYOffset);
                                _pcx.DrawImageWithMsdf(textureRenderVx.GetBmp(), 0, 0, 1, _fillColor);
                                _pcx.SetCanvasOrigin(ox, oy);
                            }
                            else if (pathRenderVx != null)
                            {
                                _pcx.FillGfxPath(
                                   _fillColor,
                                   pathRenderVx
                                );
                            }

                        }
                        break;
                }
            }

        }

        public override void FillRenderVx(Brush brush, RenderVx renderVx)
        {
            _pcx.FillRenderVx(brush, renderVx);
        }
        public override void FillRenderVx(RenderVx renderVx)
        {
            _pcx.FillRenderVx(_fillColor, renderVx);
        }
        public void ClearRect(Color color, double left, double top, double width, double height)
        {
            _pcx.ClearRect(color, left, top, width, height);
        }
        public override void FillRect(double left, double top, double width, double height)
        {
            switch (_currentBrush.BrushKind)
            {
                default:
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("unknown brush!");
#endif
                    break;
                case BrushKind.CircularGradient:
                case BrushKind.LinearGradient:
                case BrushKind.PolygonGradient:
                    {
                        //resolve internal linear gradient brush impl 

                        if (_currentClipTech == ClipingTechnique.ClipMask)
                        {
                            _pcx.FillRect(_currentBrush, left, top, width, height);
                        }
                        else
                        {
                            using (Tools.BorrowVxs(out var v1))
                            using (Tools.BorrowRect(out var rect))
                            {
                                rect.SetRectFromLTWH(left, top, width, height);
                                rect.MakeVxs(v1);

                                //convert to render vx
                                //TODO: optimize here ***
                                //we don't want to create path render vx everytime 
                                using (PathRenderVx pathRenderVx = PathRenderVx.Create(_pathRenderVxBuilder.Build(v1)))
                                {
                                    _pcx.FillGfxPath(_currentBrush, pathRenderVx);
                                }
                            }
                        }
                    }
                    break;
                case BrushKind.Solid:
                    _pcx.FillRect(_fillColor, left, top, width, height);
                    break;
                case BrushKind.Texture:
                    break;
            }

        }
        public override void FillEllipse(double left, double top, double width, double height)
        {
            //version 2:
            //agg's ellipse tools with smooth border

            double x = (left + width / 2);
            double y = (top + height / 2);
            double rx = Math.Abs(width / 2);
            double ry = Math.Abs(height / 2);
            //
            using (Tools.BorrowEllipse(out var ellipse))
            using (Tools.BorrowVxs(out var vxs))
            {
                ellipse.MakeVxs(vxs);
                //***
                //we fill  
                using (PathRenderVx pathRenderVx = PathRenderVx.Create(_pathRenderVxBuilder.Build(vxs)))
                {
                    _pcx.FillGfxPath(_strokeColor, pathRenderVx);
                }
            }

        }
        public void FillCircle(float x, float y, double radius)
        {
            FillEllipse(x - radius, y - radius, x + radius, y + radius);
        }
    }
}