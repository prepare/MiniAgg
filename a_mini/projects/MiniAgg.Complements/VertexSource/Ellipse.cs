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
// class ellipse
//
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using FlagsAndCommand = MatterHackers.Agg.ShapePath.FlagsAndCommand;

using MatterHackers.VectorMath;

namespace MatterHackers.Agg.VertexSource
{
    public class Ellipse
    {
        public double originX;
        public double originY;
        public double radiusX;
        public double radiusY;

        double m_scale = 1;
        int numSteps;
        bool m_cw;

        public Ellipse()
        {

            Init(0, 0, 1, 1, 4, false);
        }
        public Ellipse(double originX, double originY, double radiusX, double radiusY, int num_steps = 0, bool cw = false)
        { 
            Init(originX, originY, radiusX, radiusY, num_steps, cw); 
        }
        public void Reset(double originX, double originY, double radiusX, double radiusY, int num_steps)
        {
            Init(originX, originY, radiusX, radiusY, num_steps, false);
        }

        void Init(double ox, double oy,
                 double rx, double ry,
                 int num_steps, bool cw)
        {
            originX = ox;
            originY = oy;
            radiusX = rx;
            radiusY = ry;
            numSteps = num_steps;
            m_cw = cw;
            if (numSteps == 0)
            {
                CalculateNumSteps();
            }
        }
        public double ApproximateScale
        {
            get { return this.m_scale; }
            set
            {
                this.m_scale = value;
                CalculateNumSteps();
            }
        } 

        IEnumerable<VertexData> GetVertexIter()
        {
            VertexData vertexData = new VertexData();
            vertexData.command = FlagsAndCommand.CommandMoveTo;
            vertexData.x = originX + radiusX;
            vertexData.y = originY;
            yield return vertexData;

            double anglePerStep = MathHelper.Tau / (double)numSteps;
            double angle = 0;
            vertexData.command = FlagsAndCommand.CommandLineTo;
            for (int i = 1; i < numSteps; i++)
            {
                angle += anglePerStep;

                if (m_cw)
                {
                    vertexData.x = originX + Math.Cos(MathHelper.Tau - angle) * radiusX;
                    vertexData.y = originY + Math.Sin(MathHelper.Tau - angle) * radiusY;
                    yield return vertexData;
                }
                else
                {
                    vertexData.x = originX + Math.Cos(angle) * radiusX;
                    vertexData.y = originY + Math.Sin(angle) * radiusY;
                    yield return vertexData;
                }
            }

            vertexData.x = vertexData.y = 0;
            vertexData.command = FlagsAndCommand.CommandEndPoly | FlagsAndCommand.FlagClose | FlagsAndCommand.FlagCCW;
            yield return vertexData;
            vertexData.command = FlagsAndCommand.CommandStop;
            yield return vertexData;
        }


        //public ShapePath.FlagsAndCommand GetNextVertex(out double x, out double y)
        //{
        //    x = 0;
        //    y = 0;
        //    if (m_step == numSteps)
        //    {
        //        ++m_step;
        //        return FlagsAndCommand.CommandEndPoly | FlagsAndCommand.FlagClose | FlagsAndCommand.FlagCCW;
        //    }

        //    if (m_step > numSteps)
        //    {
        //        return FlagsAndCommand.CommandStop;
        //    }

        //    double angle = (double)(m_step) / (double)(numSteps) * 2.0 * Math.PI;
        //    if (m_cw)
        //    {
        //        angle = 2.0 * Math.PI - angle;
        //    }

        //    x = originX + Math.Cos(angle) * radiusX;
        //    y = originY + Math.Sin(angle) * radiusY;
        //    m_step++;
        //    return ((m_step == 1) ? FlagsAndCommand.CommandMoveTo : FlagsAndCommand.CommandLineTo);
        //}

        void CalculateNumSteps()
        {
            double ra = (Math.Abs(radiusX) + Math.Abs(radiusY)) / 2;
            double da = Math.Acos(ra / (ra + 0.125 / m_scale)) * 2;
            numSteps = (int)Math.Round(2 * Math.PI / da);
        }

        //-------------------------------------------------------
        public VertexSnap MakeVertexSnap()
        {
            return new VertexSnap(MakeVxs());
        }
        public VertexStorage MakeVxs()
        {
            List<VertexData> list = new List<VertexData>();
            foreach (VertexData vx in this.GetVertexIter())
            {
                list.Add(vx);
            }
            return new VertexStorage(list);
        }
        //-------------------------------------------------------
    }





}