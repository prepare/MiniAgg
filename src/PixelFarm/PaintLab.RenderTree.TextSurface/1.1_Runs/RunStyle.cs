﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing;

namespace LayoutFarm.TextEditing
{

    public class RunStyle
    {
        public RunStyle()
        {

        }
        //
        public byte ContentHAlign;
        //
        public RequestFont ReqFont { get; set; }
        public Color FontColor { get; set; }
        //
        internal Size MeasureString(ref TextBufferSpan textBufferSpan)
        {
            return GlobalRootGraphic.TextService.MeasureString(ref textBufferSpan, ReqFont);
        }
        internal float MeasureBlankLineHeight()
        {
            return GlobalRootGraphic.TextService.MeasureBlankLineHeight(ReqFont);
        }
        internal bool SupportsWordBreak => GlobalRootGraphic.TextService.SupportsWordBreak;
        internal ILineSegmentList BreakToLineSegments(ref TextBufferSpan textBufferSpan)
        {
            return GlobalRootGraphic.TextService.BreakToLineSegments(ref textBufferSpan);
        }

        internal void CalculateUserCharGlyphAdvancePos(ref TextBufferSpan textBufferSpan,
            int[] outputXAdvances,
            out int outputW,
            out int outputLineH)
        {
            GlobalRootGraphic.TextService.CalculateUserCharGlyphAdvancePos(
              ref textBufferSpan,
                ReqFont,
                outputXAdvances,
                out outputW,
                out outputLineH);
        }

        internal void CalculateUserCharGlyphAdvancePos(ref TextBufferSpan textBufferSpan,
            ILineSegmentList lineSegs,
            int[] outputXAdvances,
            out int outputW,
            out int outputLineH)
        {
            GlobalRootGraphic.TextService.CalculateUserCharGlyphAdvancePos(
              ref textBufferSpan,
                lineSegs,
                ReqFont,
                outputXAdvances,
                out outputW,
                out outputLineH);
        }

    }

}