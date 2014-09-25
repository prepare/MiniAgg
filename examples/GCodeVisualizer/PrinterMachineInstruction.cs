﻿/*
Copyright (c) 2014, Lars Brubaker
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MatterHackers.VectorMath;

namespace MatterHackers.GCodeVisualizer
{
    public class PrinterMachineInstruction
    {
        public string Line;

        Vector3 xyzPosition = new Vector3();
        double ePosition = 0;
        double feedRate = 0;

        public enum MovementTypes { Absolute, Relative };
        // Absolute is the RepRap default
        public MovementTypes movementType = MovementTypes.Absolute;

        public double secondsThisLine;
        public double secondsToEndFromHere;

        public PrinterMachineInstruction(string Line)
        {
            this.Line = Line;
        }

        public PrinterMachineInstruction(string Line, PrinterMachineInstruction copy)
            : this(Line)
        {
            xyzPosition = copy.xyzPosition;
            feedRate = copy.feedRate;
            ePosition = copy.ePosition;
            movementType = copy.movementType;
            secondsToEndFromHere = copy.secondsToEndFromHere;
            ExtruderIndex = copy.ExtruderIndex;
        }

        public int ExtruderIndex { get; set; }

        public Vector3 Position
        {
            get { return xyzPosition; }
        }

        public double X
        {
            get { return xyzPosition.x; }
            set
            {
                if (movementType == MovementTypes.Absolute)
                {
                    xyzPosition.x = value;
                }
                else
                {
                    xyzPosition.x += value;
                }
            }
        }

        public double Y
        {
            get { return xyzPosition.y; }
            set
            {
                if (movementType == MovementTypes.Absolute)
                {
                    xyzPosition.y = value;
                }
                else
                {
                    xyzPosition.y += value;
                }
            }
        }

        public double Z
        {
            get { return xyzPosition.z; }
            set
            {
                if (movementType == MovementTypes.Absolute)
                {
                    xyzPosition.z = value;
                }
                else
                {
                    xyzPosition.z += value;
                }
            }
        }

        public double EPosition
        {
            get { return ePosition; }
            set
            {
                ePosition = value;
            }
        }

        public double FeedRate
        {
            get { return feedRate; }
            set { feedRate = value; }
        }
    }
}
