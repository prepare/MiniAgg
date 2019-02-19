﻿//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017-present, WinterDev (C# port)
using System;
using System.Collections.Generic;
using PixelFarm.Drawing;

namespace ExtMsdfGen
{
    class MyCustomPixelBlender : PixelFarm.CpuBlit.PixelProcessing.CustomPixelBlender
    {
        //const int WHITE = (255 << 24) | (255 << 16) | (255 << 8) | 255;
        const int BLACK = (255 << 24);
        const int GREEN = (255 << 24) | (255 << 8);
        const int RED = (255 << 24) | (255 << 16);


        struct OverlapPart
        {
            readonly int _edgeA;
            readonly int _edgeB;
            readonly AreaKind _areaKindA;
            readonly AreaKind _areaKindB;
            public OverlapPart(int edgeA, AreaKind areaKindA, int edgeB, AreaKind areaKindB)
            {
                if (edgeB < edgeA)
                {
                    //swap
                    _edgeA = edgeB;
                    _edgeB = edgeA;
                    _areaKindA = areaKindB;
                    _areaKindB = areaKindA;
                }
                else
                {
                    _edgeA = edgeA;
                    _edgeB = edgeB;
                    _areaKindA = areaKindA;
                    _areaKindB = areaKindB;
                }

            }
#if DEBUG
            public override string ToString()
            {
                return _edgeA + "," + _edgeB;
            }

#endif
        }

        Dictionary<OverlapPart, ushort> _overlapParts = new Dictionary<OverlapPart, ushort>();
        internal List<List<ushort>> _overlapList = new List<List<ushort>>();

        int _areaInside100;

        int _bmpW;
        int _bmpH;
        public MyCustomPixelBlender()
        {
        }
        public void SetBitmapSize(int w, int h)
        {
            _bmpW = w;
            _bmpH = h;
        }
        public void ClearOverlapList()
        {
            _overlapParts.Clear();
            _overlapList.Clear();
        }
        public void SetCurrentInsideAreaCoverage100(Color areaInside100)
        {
            _areaInside100 = (int)areaInside100.ToABGR();
        }
        public ushort RegisterOverlapOuter(ushort corner1, ushort corner2, AreaKind areaKind)
        {

            OverlapPart overlapPart = new OverlapPart(corner1, areaKind, corner2, areaKind);
            if (!_overlapParts.TryGetValue(overlapPart, out ushort found))
            {
                if (_overlapList.Count > ushort.MaxValue)
                {
                    throw new NotSupportedException();
                }

                ushort newPartNo = (ushort)_overlapList.Count;
                _overlapParts.Add(overlapPart, newPartNo);
                //
                List<ushort> cornerList = new List<ushort>();
                _overlapList.Add(cornerList);
                cornerList.Add(corner1);
                cornerList.Add(corner2);
            }
            return found;
        }
        protected override unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor)
        {
            CustomBlendPixel32(dstPtr, srcColor);
        }
        protected override unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor, int coverageValue)
        {
            CustomBlendPixel32(dstPtr, srcColor);
        }


        unsafe void CustomBlendPixel32(int* dstPtr, Color srcColor)
        {

            int srcColorABGR = (int)srcColor.ToABGR();
            int existingColor = *dstPtr;

            int existing_R = (existingColor >> CO.R_SHIFT) & 0xFF;
            int existing_G = (existingColor >> CO.G_SHIFT) & 0xFF;
            int existing_B = (existingColor >> CO.B_SHIFT) & 0xFF;

            if (srcColorABGR == _areaInside100)
            {
                if (existing_G != EdgeBmpLut.AREA_INSIDE_COVERATE100)
                {
                    *dstPtr = _areaInside100;
                }
                return;
            }


            if (existingColor != _areaInside100 && existingColor != BLACK)
            {
                //overlap pixel found! 


                if (existing_R == 255 && existing_G == 0 && existing_B == 0)
                {
                    //red color => return 
                    return;
                }
                if (srcColorABGR == existingColor)
                {
                    //same color
                    return;
                }

                if (existing_G == EdgeBmpLut.AREA_INSIDE_COVERATE100)
                {
                    //inside 100 of another area
                    return;
                }


                //decode edge information
                //we use 2 bytes for encode edge number 
                ushort existingEdgeNo = EdgeBmpLut.DecodeEdgeFromColor(existingColor, out AreaKind existingAreaKind);
                ushort newEdgeNo = EdgeBmpLut.DecodeEdgeFromColor(srcColor, out AreaKind newEdgeAreaKind);

#if DEBUG

#endif

                if (newEdgeAreaKind == AreaKind.OverlapInside || newEdgeAreaKind == AreaKind.OverlapOutside)
                {
                    //new color is overlap color 
                    if (existingAreaKind == AreaKind.OverlapInside || existingAreaKind == AreaKind.OverlapOutside)
                    {
                        List<ushort> registerList = _overlapList[newEdgeNo];
                        _overlapList[existingEdgeNo].AddRange(registerList);
                    }
                    else
                    {
                        List<ushort> registerList = _overlapList[newEdgeNo];
                        registerList.Add(existingEdgeNo);
                        *dstPtr = EdgeBmpLut.EncodeToColor(newEdgeNo, (existing_G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside).ToARGB();
                    }
                }
                else
                {
                    if (existingAreaKind == AreaKind.OverlapInside ||
                        existingAreaKind == AreaKind.OverlapOutside)
                    {
                        _overlapList[existingEdgeNo].Add(newEdgeNo);
                    }
                    else
                    {
                        ////create new overlap part
                        if (newEdgeNo == existingEdgeNo) return;

                        OverlapPart overlapPart;
                        AreaKind areaKind;
                        if (existingAreaKind == AreaKind.Inside)
                        {
                            if (newEdgeAreaKind == AreaKind.Inside)
                            {
                                areaKind = AreaKind.OverlapInside;
                                overlapPart = new OverlapPart(
                                    existingEdgeNo, (existing_G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside,
                                    newEdgeNo, (srcColor.G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside);
                            }
                            else
                            {
                                areaKind = AreaKind.OverlapInside;
                                overlapPart = new OverlapPart(
                                   existingEdgeNo, (existing_G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside,
                                   newEdgeNo, (srcColor.G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside);
                            }
                        }
                        else
                        {
                            //existing is outside
                            if (newEdgeAreaKind == AreaKind.Inside)
                            {
                                areaKind = AreaKind.OverlapInside;
                                overlapPart = new OverlapPart(
                                    existingEdgeNo, (existing_G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside,
                                    newEdgeNo, (srcColor.G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside);
                            }
                            else
                            {
                                areaKind = AreaKind.OverlapOutside;
                                overlapPart = new OverlapPart(
                                   existingEdgeNo, (existing_G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside,
                                   newEdgeNo, (srcColor.G == 0) ? AreaKind.OverlapInside : AreaKind.OverlapOutside);
                            }
                        }


                        if (!_overlapParts.TryGetValue(overlapPart, out ushort found))
                        {
                            if (_overlapList.Count >= ushort.MaxValue)
                            {
                                throw new NotSupportedException();
                            }
                            //
                            ushort newPartNo = (ushort)_overlapList.Count;
                            _overlapParts.Add(overlapPart, newPartNo);
                            //
                            List<ushort> cornerList = new List<ushort>();
                            _overlapList.Add(cornerList);
                            cornerList.Add(existingEdgeNo);
                            cornerList.Add(newEdgeNo);
                            //set new color
                            *dstPtr = EdgeBmpLut.EncodeToColor(newPartNo, areaKind).ToARGB();
                        }
                        else
                        {
                            //set new color
                            *dstPtr = EdgeBmpLut.EncodeToColor(found, areaKind).ToARGB();
                        }
                    }
                }
            }
            else if (existingColor != _areaInside100)
            {
                *dstPtr = srcColor.ToARGB();
            }

            else
            {
                *dstPtr = srcColor.ToARGB();
            }
        }
    }

    /// <summary>
    /// edge bitmap lookup table
    /// </summary>
    public class EdgeBmpLut
    {
        int _w;
        int _h;
        int[] _buffer;
        List<ContourCorner> _corners;
        List<EdgeSegment> _flattenEdges;
        List<EdgeSegment[]> _overlappedEdgeList;
        internal EdgeBmpLut(List<ContourCorner> corners, List<EdgeSegment> flattenEdges, List<int> segOfNextContours, List<int> cornerOfNextContours)
        {
            //move first to last 
            int startAt = 0;
            for (int i = 0; i < segOfNextContours.Count; ++i)
            {
                int nextStartAt = segOfNextContours[i];
                //
                EdgeSegment firstSegment = flattenEdges[startAt];

                flattenEdges.RemoveAt(startAt);
                if (i == segOfNextContours.Count - 1)
                {
                    flattenEdges.Add(firstSegment);
                }
                else
                {
                    flattenEdges.Insert(nextStartAt - 1, firstSegment);
                }
                startAt = nextStartAt;
            }

            _corners = corners;
            _flattenEdges = flattenEdges;
            EdgeOfNextContours = segOfNextContours;
            CornerOfNextContours = cornerOfNextContours;

            ConnectExtendedPoints(corners, cornerOfNextContours); //after arrange 
        }
        internal void SetOverlappedList(List<ushort[]> overlappedList)
        {
            int m = overlappedList.Count;
            _overlappedEdgeList = new List<EdgeSegment[]>(m);
            for (int i = 0; i < m; ++i)
            {
                ushort[] arr1 = overlappedList[i];
                EdgeSegment[] corners = new EdgeSegment[arr1.Length];//overlapping corner region
                for (int a = 0; a < arr1.Length; ++a)
                {
                    corners[a] = _corners[arr1[a]].CenterSegment;
                }
                _overlappedEdgeList.Add(corners);
            }

        }
        static void ConnectExtendedPoints(List<ContourCorner> corners, List<int> cornerOfNextContours)
        {
            //test 2 if each edge has unique color 
            int startAt = 0;
            for (int i = 0; i < cornerOfNextContours.Count; ++i)
            {
                int nextStartAt = cornerOfNextContours[i];
                for (int n = startAt + 1; n < nextStartAt; ++n)
                {
                    ContourCorner.ConnectToEachOther(corners[n - 1], corners[n]);
                }
                //--------------
                {
                    //the last one 
                    ContourCorner.ConnectToEachOther(corners[nextStartAt - 1], corners[startAt]);
                }
                //---------
                startAt = nextStartAt;//***
            }
        }
        //
        public List<int> EdgeOfNextContours { get; private set; }
        public List<int> CornerOfNextContours { get; private set; }

        //
        public void SetBmpBuffer(int w, int h, int[] buffer)
        {
            _w = w;
            _h = h;
            _buffer = buffer;
        }
        public List<ContourCorner> Corners => _corners;

        public int GetPixel(int x, int y) => _buffer[y * _w + x];

        //const int WHITE = (255 << 24) | (255 << 16) | (255 << 8) | 255;

        public EdgeStructure GetEdgeStructure(int x, int y)
        {
            //decode 
            int pixel = _buffer[y * _w + x];
            int g = (pixel >> 8) & 0xFF;
            if (pixel == 0)
            {
                return EdgeStructure.Empty;
            }
            else if (g == AREA_INSIDE_COVERATE100)
            {
                return EdgeStructure.Empty;
            }
            else
            {
                int index = DecodeEdgeFromColor(pixel, out AreaKind areaKind);
                switch (areaKind)
                {
                    default: throw new NotSupportedException();
                    case AreaKind.Outside:
                    case AreaKind.OuterGap:
                    case AreaKind.Inside:
                        return new EdgeStructure(_corners[index].CenterSegment, areaKind);
                    case AreaKind.OverlapInside:
                    case AreaKind.OverlapOutside:
                        return new EdgeStructure(_overlappedEdgeList[index], areaKind);
                }
            }
        }
        internal const int AREA_INSIDE_COVERATE100 = 10;
        const int AREA_INSIDE = 15;
        const int AREA_OUTSIDE_GAP = 25;
        const int AREA_OUTSIDE = 50;
        const int AREA_OVERLAP_INSIDE = 70;
        const int AREA_OVERLAP_OUTSIDE = 75;

        public static ushort DecodeEdgeFromColor(Color c, out AreaKind areaKind)
        {
            switch ((int)c.G)
            {
                case AREA_INSIDE_COVERATE100: areaKind = AreaKind.AreaInsideCoverage100; break;
                case AREA_INSIDE: areaKind = AreaKind.Inside; break;
                case AREA_OUTSIDE_GAP: areaKind = AreaKind.OuterGap; break;
                case AREA_OUTSIDE: areaKind = AreaKind.Outside; break;
                case AREA_OVERLAP_INSIDE: areaKind = AreaKind.OverlapInside; break;
                case AREA_OVERLAP_OUTSIDE: areaKind = AreaKind.OverlapOutside; break;

                default: throw new NotSupportedException();
            }
            return (ushort)((c.R << 8) | c.B);
        }
        public static ushort DecodeEdgeFromColor(int inputColor, out AreaKind areaKind)
        {
            //int inputR = (inputColor >> CO.R_SHIFT) & 0xFF;
            //int inputB = (inputColor >> CO.B_SHIFT) & 0xFF;
            //int inputG = (inputColor >> CO.G_SHIFT) & 0xFF;

            //ABGR

            int inputB = (inputColor >> 0) & 0xFF;
            int inputG = (inputColor >> 8) & 0xFF;
            int inputR = (inputColor >> 16) & 0xFF;

            switch (inputG)
            {
                case AREA_INSIDE_COVERATE100: areaKind = AreaKind.AreaInsideCoverage100; break;
                case AREA_INSIDE: areaKind = AreaKind.Inside; break;
                case AREA_OUTSIDE_GAP: areaKind = AreaKind.OuterGap; break;
                case AREA_OUTSIDE: areaKind = AreaKind.Outside; break;
                case AREA_OVERLAP_INSIDE: areaKind = AreaKind.OverlapInside; break;
                case AREA_OVERLAP_OUTSIDE: areaKind = AreaKind.OverlapOutside; break;

                default: throw new NotSupportedException();
            }
            return (ushort)((inputR << 8) | inputB);
        }
        public static PixelFarm.Drawing.Color EncodeToColor(ushort cornerNo, AreaKind areaKind)
        {

            switch (areaKind)
            {
                default: throw new NotSupportedException();
                case AreaKind.Inside:
                    {
                        int r = cornerNo >> 8;
                        int b = cornerNo & 0xFF;
                        return new PixelFarm.Drawing.Color((byte)r, AREA_INSIDE, (byte)b);
                    }
                case AreaKind.OuterGap:
                    {
                        int r = cornerNo >> 8;
                        int b = cornerNo & 0xFF;
                        return new PixelFarm.Drawing.Color((byte)r, AREA_OUTSIDE_GAP, (byte)b);
                    }
                case AreaKind.Outside:
                    {
                        int r = cornerNo >> 8;
                        int b = cornerNo & 0xFF;
                        return new PixelFarm.Drawing.Color((byte)r, AREA_OUTSIDE, (byte)b);
                    }
                case AreaKind.OverlapInside:
                    {
                        int r = cornerNo >> 8;
                        int b = cornerNo & 0xFF;

                        return new PixelFarm.Drawing.Color((byte)r, AREA_OVERLAP_INSIDE, (byte)b);
                    }
                case AreaKind.OverlapOutside:
                    {
                        int r = cornerNo >> 8;
                        int b = cornerNo & 0xFF;
                        return new PixelFarm.Drawing.Color((byte)r, AREA_OVERLAP_OUTSIDE, (byte)b);
                    }
                case AreaKind.AreaInsideCoverage100:
                    {
                        //corner=> shape number (id)
                        int r = cornerNo >> 8;
                        int b = cornerNo & 0xFF;
                        return new PixelFarm.Drawing.Color((byte)r, AREA_INSIDE_COVERATE100, (byte)b);
                    }
            }
        }
    }

}