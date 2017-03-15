//BSD, 2014-2017, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
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
//
// vertex_sequence container and vertex_dist struct
//
//----------------------------------------------------------------------------


namespace PixelFarm.Agg
{
    //----------------------------------------------------------vertex_sequence
    // Modified agg::pod_vector. The data is interpreted as a sequence 
    // of vertices. It means that the type T must expose:
    //
    // bool T::operator() (const T& val)
    // 
    // that is called every time a new vertex is being added. The main purpose
    // of this operator is the possibility to calculate some values during 
    // adding and to return true if the vertex fits some criteria or false if
    // it doesn't. In the last case the new vertex is not added. 
    // 
    // The simple example is filtering coinciding vertices with calculation 
    // of the distance between the current and previous ones:
    //
    //    struct vertex_dist
    //    {
    //        double   x;
    //        double   y;
    //        double   dist;
    //
    //        vertex_dist() {}
    //        vertex_dist(double x_, double y_) :
    //            x(x_),
    //            y(y_),
    //            dist(0.0)
    //        {
    //        }
    //
    //        bool operator () (const vertex_dist& val)
    //        {
    //            return (dist = calc_distance(x, y, val.x, val.y)) > EPSILON;
    //        }
    //    };
    //
    // Function close() calls this operator and removes the last vertex if 
    // necessary.
    //------------------------------------------------------------------------
    class VertexDistanceList : ArrayList<VertexDistance>
    {
        public override void AddVertex(VertexDistance val)
        {

            switch (base.Count)
            {
                case 0: break;
                case 1:
                    {
                        //check if not duplicated vertex
                        if (Array[base.Count - 1].IsEqual(val))
                        {
                            base.RemoveLast();
                        }
                    }
                    break;
                default:
                    {
                        //check if not duplicated vertex
                        if (Array[base.Count - 2].IsEqual(Array[base.Count - 1]))
                        {
                            base.RemoveLast();
                        }
                    }
                    break;
            }



            base.AddVertex(val);
        }

        public void ReplaceLast(VertexDistance val)
        {
            base.RemoveLast();
            AddVertex(val);
        }

        public void Close(bool closed)
        {
            int snapSize = base.Count;
            var vtxArray = this.Array;
            while (snapSize > 1)
            {
                if (!vtxArray[snapSize - 2].IsEqual(vtxArray[snapSize - 1]))
                {
                    break;
                }
                VertexDistance t = this[snapSize - 1];
                base.RemoveLast();
                snapSize--;
                ReplaceLast(t);
            }

            if (closed)
            {
                snapSize = base.Count;
                while (snapSize > 1)
                {
                    if (!Array[snapSize - 1].IsEqual(Array[0]))
                    {
                        break;
                    }
                    base.RemoveLast();
                    snapSize--;
                }
            }
        }

        public void GetTripleVertices(int idx, out VertexDistance prev, out VertexDistance cur, out VertexDistance next)
        {
            int count = this.Count;
            prev = this[(idx + count - 1) % count];
            cur = this[idx];
            next = this[(idx + 1) % count];
        }
    }

    //-------------------------------------------------------------vertex_dist
    // Vertex (x, y) with the distance to the next one. The last vertex has 
    // distance between the last and the first points if the polygon is closed
    // and 0.0 if it's a polyline.
    public struct VertexDistance
    {

        public readonly double x;
        public readonly double y;
        public double dist;

        public VertexDistance(double x, double y)
        {
            this.x = x;
            this.y = y;
            dist = 0; //lazy calculate ?

        }
        public bool IsEqual(VertexDistance val)
        {
            if ((dist = AggMath.calc_distance(x, y, val.x, val.y)) > AggMath.VERTEX_DISTANCE_EPSILON)
            {
                //diff enough=> this is NOT equal with val
                return false;
            }
            else
            {
                //not diff enough => this is equal (with val)
                dist = 1.0 / AggMath.VERTEX_DISTANCE_EPSILON;
                return true;
            }
        }
    }

    /*
    //--------------------------------------------------------vertex_dist_cmd
    // Save as the above but with additional "command" value
    struct vertex_dist_cmd : vertex_dist
    {
        unsigned cmd;

        vertex_dist_cmd() {}
        vertex_dist_cmd(double x_, double y_, unsigned cmd_) :
            base (x_, y_)
            
        {
            cmd = cmd;
        }
    };
     */
}

//#endif
