﻿/*
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
using System.Collections.Generic;

using MatterHackers.VectorMath;


namespace MatterHackers.RenderOpenGl
{
    public class CachedTesselator : VertexTesselatorAbstract
    {
        internal bool lastEdgeFlagSet = false;
        internal List<AddedVertex> VerticesCache = new List<AddedVertex>();
        internal List<RenderIndices> IndicesCache = new List<RenderIndices>();

        internal class AddedVertex
        {
            Vector2 position;

            public Vector2 Position { get { return position; } }

            internal AddedVertex(double x, double y)
            {
                position.x = x;
                position.y = y;
            }
        }

        public class RenderIndices
        {
            int index;
            bool isEdge;

            public int Index
            {
                get { return index; }
            }

            public bool IsEdge
            {
                get { return isEdge; }
            }

            internal RenderIndices(int index, bool isEdge)
            {
                this.index = index;
                this.isEdge = isEdge;
            }
        }

        public CachedTesselator()
        {
            callVertex += VertexCallBack;
            callEdgeFlag += EdgeFlagCallBack;
            callCombine += CombineCallBack;
        }

        public override void BeginPolygon()
        {
            VerticesCache.Clear();
            IndicesCache.Clear();

            base.BeginPolygon();
        }

        public void VertexCallBack(int index)
        {
            IndicesCache.Add(new RenderIndices(index, lastEdgeFlagSet));
        }

        public void EdgeFlagCallBack(bool isEdge)
        {
            lastEdgeFlagSet = isEdge;
        }

        public void CombineCallBack(double[] coords3, int[] data4,
            double[] weight4, out int outData)
        {
            outData = AddVertex(coords3[0], coords3[1], false);
        }

        public override void AddVertex(double x, double y)
        {
            AddVertex(x, y, true);
        }

        public int AddVertex(double x, double y, bool passOnToTesselator)
        {
            int index = VerticesCache.Count;
            VerticesCache.Add(new AddedVertex(x, y));
            double[] coords = new double[3];
            coords[0] = x;
            coords[1] = y;
            if (passOnToTesselator)
            {
                AddVertex(coords, index);
            }
            return index;
        }
    }
}
