/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2006, 2008 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with MIConvexHull.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://miconvexhull.codeplex.com
 *************************************************************************/
using System;
namespace MIConvexNameSpace
{
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class face
    {
        const double epsilon = 0.0000001;
        internal vertex v1 { get; private set; }
        internal vertex v2 { get; private set; }
        internal vertex v3 { get; private set; }
        internal vertex normal { get; private set; }
        internal vertex center { get; private set; }

        public static face MakeFace(vertex v1, vertex v2, vertex v3)
        {
            if (v1.Equals(v2) || v2.Equals(v3) || v3.Equals(v1)) return null;
            vertex n = MIConvexHull.crossProduct(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z,
                v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);
            var nMag = Math.Sqrt((n.X * n.X) + (n.Y * n.Y) + (n.Z * n.Z));
            if (nMag < epsilon) return null;
            n.X /= nMag;
            n.Y /= nMag;
            n.Z /= nMag;

            return new face()
            {
                v1 = v1,
                v2 = v2,
                v3 = v3,
                normal = n,
                center = new vertex(((v1.X + v2.X + v3.X) / 3),
                    ((v1.Y + v2.Y + v3.Y) / 3),
                    ((v1.Z + v2.Z + v3.Z) / 3))
            };
        }
    }
}
