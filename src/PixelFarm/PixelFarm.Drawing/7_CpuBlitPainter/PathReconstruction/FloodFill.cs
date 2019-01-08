//MIT, 2014-present, WinterDev
//MatterHackers 
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using System;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit;

namespace PixelFarm.PathReconstruction
{
    /// <summary>
    /// flood fill tool
    /// </summary>
    public class FloodFill
    {
        int _imageWidth;
        int _imageHeight;

        byte _tolerance0To255;
        Color _fillColor;
        bool[] _pixelsChecked;
        FillingRule _fillRule;
        SimpleQueue<HSpan> _ranges = new SimpleQueue<HSpan>(9);

        IBitmapSrc _destImgRW;

        /// <summary>
        /// if user want to collect output range 
        /// </summary>
        ConnectedHSpans _connectedHSpans;

        public FloodFill(Color fillColor)
            : this(fillColor, 0)
        {
        }
        public FloodFill(Color fillColor, byte tolerance)
        {
            Update(fillColor, tolerance);
        }

        public void SetOutput(ConnectedHSpans output)
        {
            _connectedHSpans = output;
        }

        public Color FillColor => _fillColor;
        public byte Tolerance => _tolerance0To255;
        public void Update(Color fillColor, byte tolerance)
        {
            _tolerance0To255 = tolerance;
            _fillColor = fillColor;

            if (tolerance > 0)
            {
                _fillRule = new ToleranceMatch(fillColor, tolerance);
            }
            else
            {
                _fillRule = new ExactMatch(fillColor);
            }
        }



        public void Fill(MemBitmap bmpTarget, int x, int y)
        {
            Fill((IBitmapSrc)bmpTarget, x, y);
        }
        public void Fill(IBitmapSrc bmpTarget, int x, int y)
        {
            y -= _imageHeight;
            unchecked // this way we can overflow the uint on negative and get a big number
            {
                if ((uint)x >= bmpTarget.Width || (uint)y >= bmpTarget.Height)
                {
                    return;
                }
            }
            _destImgRW = bmpTarget;

            unsafe
            {
                //review TempMemPtr
                using (PixelFarm.CpuBlit.Imaging.TempMemPtr destBufferPtr = bmpTarget.GetBufferPtr())
                {

                    _imageWidth = bmpTarget.Width;
                    _imageHeight = bmpTarget.Height;
                    //reset new buffer, clear mem?
                    _pixelsChecked = new bool[_imageWidth * _imageHeight];

                    int* destBuffer = (int*)destBufferPtr.Ptr;
                    int startColorBufferOffset = bmpTarget.GetBufferOffsetXY32(x, y);

                    int start_color = *(destBuffer + startColorBufferOffset);

                    _fillRule.SetStartColor(Drawing.Color.FromArgb(
                        (start_color >> 16) & 0xff,
                        (start_color >> 8) & 0xff,
                        (start_color) & 0xff));


                    LinearFill(destBuffer, x, y);

                    bool addToOutputRanges = _connectedHSpans != null;
                    if (addToOutputRanges)
                    {
                        _connectedHSpans.Clear();
                        _connectedHSpans.SetYCut(y);
                    }


                    while (_ranges.Count > 0)
                    {
                        HSpan range = _ranges.Dequeue();

                        if (addToOutputRanges)
                        {
                            _connectedHSpans.AddHSpan(range);
                        }

                        int downY = range.y - 1;
                        int upY = range.y + 1;
                        int downPixelOffset = (_imageWidth * (range.y - 1)) + range.startX;
                        int upPixelOffset = (_imageWidth * (range.y + 1)) + range.startX;
                        for (int rangeX = range.startX; rangeX <= range.endX; rangeX++)
                        {
                            if (range.y > 0)
                            {
                                if (!_pixelsChecked[downPixelOffset])
                                {
                                    int bufferOffset = bmpTarget.GetBufferOffsetXY32(rangeX, downY);

                                    if (_fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                                    {
                                        LinearFill(destBuffer, rangeX, downY);
                                    }
                                }
                            }

                            if (range.y < (_imageHeight - 1))
                            {
                                if (!_pixelsChecked[upPixelOffset])
                                {
                                    int bufferOffset = bmpTarget.GetBufferOffsetXY32(rangeX, upY);
                                    if (_fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                                    {
                                        LinearFill(destBuffer, rangeX, upY);
                                    }
                                }
                            }
                            upPixelOffset++;
                            downPixelOffset++;
                        }
                    }
                }
            }

            //reset
            _imageHeight = 0;
            _ranges.Clear();
            _destImgRW = null;
        }
        public bool SkipActualFill { get; set; }
        /// <summary>
        /// fill to left side and right side of the line
        /// </summary>
        /// <param name="destBuffer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        unsafe void LinearFill(int* destBuffer, int x, int y)
        {

            int leftFillX = x;
            int bufferOffset = _destImgRW.GetBufferOffsetXY32(x, y);
            int pixelOffset = (_imageWidth * y) + x;

            bool doActualFill = !SkipActualFill;
            for (; ; )
            {

                if (doActualFill)
                {
                    _fillRule.SetPixel(destBuffer + bufferOffset);
                }


                _pixelsChecked[pixelOffset] = true;
                leftFillX--;
                pixelOffset--;
                bufferOffset--;
                if (leftFillX <= 0 || (_pixelsChecked[pixelOffset]) || !_fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                {
                    break;
                }
            }
            leftFillX++;
            //
            int rightFillX = x;
            bufferOffset = _destImgRW.GetBufferOffsetXY32(x, y);
            pixelOffset = (_imageWidth * y) + x;
            for (; ; )
            {
                if (doActualFill)
                {
                    _fillRule.SetPixel(destBuffer + bufferOffset);
                }
                _pixelsChecked[pixelOffset] = true;
                rightFillX++;
                pixelOffset++;
                bufferOffset++;
                if (rightFillX >= _imageWidth || _pixelsChecked[pixelOffset] || !_fillRule.CheckPixel(*(destBuffer + bufferOffset)))
                {
                    break;
                }
            }
            rightFillX--;
            _ranges.Enqueue(new HSpan(leftFillX, rightFillX, y));
        }


        class SimpleQueue<T>
        {
            T[] _itemArray;
            int _size;
            int _head;
            int _shiftFactor;
            int _mask;
            //

            public SimpleQueue(int shiftFactor)
            {
                _shiftFactor = shiftFactor;
                _mask = (1 << shiftFactor) - 1;
                _itemArray = new T[1 << shiftFactor];
                _head = 0;
                _size = 0;
            }
            public int Count => _size;
            public T First => _itemArray[_head & _mask];

            public void Clear() => _head = 0;

            public void Enqueue(T itemToQueue)
            {
                if (_size == _itemArray.Length)
                {
                    int headIndex = _head & _mask;
                    _shiftFactor += 1;
                    _mask = (1 << _shiftFactor) - 1;
                    T[] newArray = new T[1 << _shiftFactor];
                    // copy the from head to the end
                    Array.Copy(_itemArray, headIndex, newArray, 0, _size - headIndex);
                    // copy form 0 to the size
                    Array.Copy(_itemArray, 0, newArray, _size - headIndex, headIndex);
                    _itemArray = newArray;
                    _head = 0;
                }
                _itemArray[(_head + (_size++)) & _mask] = itemToQueue;
            }

            public T Dequeue()
            {
                int headIndex = _head & _mask;
                T firstItem = _itemArray[headIndex];
                if (_size > 0)
                {
                    _head++;
                    _size--;
                }
                return firstItem;
            }
        }
    }


    /// <summary>
    /// horizontal (scanline) span
    /// </summary>
    public struct HSpan
    {
        public readonly int startX;
        public readonly int endX;
        public readonly int y;
        public HSpan(int startX, int endX, int y)
        {
            this.startX = startX;
            this.endX = endX;
            this.y = y;
        }
        internal bool HasHorizontalTouch(int otherStartX, int otherEndX)
        {
            return HasHorizontalTouch(this.startX, this.endX, otherStartX, otherEndX);
        }
        internal static bool HasHorizontalTouch(int x0, int x1, int x2, int x3)
        {
            if (x0 == x2)
            {
                return true;
            }
            else if (x0 > x2)
            {
                //
                return x0 <= x3;
            }
            else
            {
                return x1 >= x2;
            }
        }

#if DEBUG
        public override string ToString()
        {
            return "line:" + y + ", x_start=" + startX + ",end_x=" + endX + ",len=" + (endX - startX);
        }
#endif
    }


}