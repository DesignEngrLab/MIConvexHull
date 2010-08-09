/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2006, 2010 Matthew Ira Campbell, PhD.
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
namespace ExampleWithGraphics
{
    using MIConvexHullPluginNameSpace;
    using Petzold.Media3D;
    using System.Windows.Media.Media3D;
    using System.Windows.Media;
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class vertex : Sphere, IVertexConvHull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="vertex"/> class.
        /// </summary>
        public vertex()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="vertex"/> class.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        public vertex(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Center = new System.Windows.Media.Media3D.Point3D(X, Y, Z);
            this.Radius = 0.25;

            this.BackMaterial = new DiffuseMaterial(Brushes.Black);
        }

        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>The X position.</value>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>The Y position.</value>
        public double Y { get; set; }


        /// <summary>
        /// Gets or sets the Z. Not used by MIConvexHull2D.
        /// </summary>
        /// <value>The Z position.</value>
        public double Z { get; set; }
    }
}
