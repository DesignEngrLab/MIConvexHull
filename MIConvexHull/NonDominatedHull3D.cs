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
namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;
    using System.Collections;

    /// <summary>
    /// MIConvexHull for 3D is not yet complete. Most of the code from Find2D is shown below.
    /// Look for my 5-stars comments for things to do ("*****")
    /// </summary>
    public static partial class NonDominatedHull
    {
        /// <summary>
        /// Finds the convex hull vertices.
        /// </summary>
        /// <param name="vertices">All of the vertices as a list.</param>
        /// <returns></returns>
        public static List<IVertexConvHull> Find3D(List<IVertexConvHull> origVertices)
        {
            var origVNum = origVertices.Count;
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;
            double maxZ = double.NegativeInfinity;
            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double minZ = double.PositiveInfinity;

            /* the array of extreme is comprised of: 0.minX, 1. minSum, 2. minY, 3. maxDiff, 4. MaxX, 5. MaxSum, 6. MaxY, 7. MinDiff. */
            IVertexConvHull[] extremeVertices = new IVertexConvHull[6];
            //  int[] extremeVertexIndices = new int[8]; I thought that this might speed things up. That is, to use this to RemoveAt
            // as oppoaws to the Remove in line 91, which I thought might be slow. Turns out I was wrong - plus code is more succinct
            // way.
            for (int i = 0; i < origVNum; i++)
            {
                var n = origVertices[i];
                if (n.X < minX) { extremeVertices[0] = n; minX = n.X; }
                if (n.Y < minY) { extremeVertices[1] = n; minY = n.Y; }
                if (n.Z < minZ) { extremeVertices[2] = n; minZ = n.Z; }

                if (n.X > maxX) { extremeVertices[3] = n; maxX = n.X; }
                if (n.Y > maxY) { extremeVertices[4] = n; maxY = n.Y; }
                if (n.Z > maxZ) { extremeVertices[5] = n; maxZ = n.Z; }
            }
            maxX = Math.Min(Math.Min(extremeVertices[1].X, extremeVertices[2].X),
                Math.Min(extremeVertices[4].X, extremeVertices[5].X));
            maxY = Math.Min(Math.Min(extremeVertices[0].X, extremeVertices[2].X),
                Math.Min(extremeVertices[3].X, extremeVertices[5].X));
            maxZ = Math.Min(Math.Min(extremeVertices[0].X, extremeVertices[1].X),
                Math.Min(extremeVertices[3].X, extremeVertices[4].X));
            minX = Math.Max(Math.Max(extremeVertices[1].X, extremeVertices[2].X),
                Math.Max(extremeVertices[4].X, extremeVertices[5].X));
            minY = Math.Max(Math.Max(extremeVertices[0].X, extremeVertices[2].X),
                Math.Max(extremeVertices[3].X, extremeVertices[5].X));
            minZ = Math.Max(Math.Max(extremeVertices[0].X, extremeVertices[1].X),
                Math.Max(extremeVertices[3].X, extremeVertices[4].X));
            var nonDomHull = new List<IVertexConvHull>();
            for (int i = 0; i < 6; i++)
                if (!nonDomHull.Contains(extremeVertices[i]))
                {
                    nonDomHull.Add(extremeVertices[i]);
                    origVertices.Remove(extremeVertices[i]);
                }
            origVNum = origVertices.Count;
            var ParetoSets = new List<IVertexConvHull>[8];
            for (int i = 0; i < 8; i++) ParetoSets[i] = new List<IVertexConvHull>();
            for (int i = 0; i < origVNum; i++)
            {
                var n = origVertices[i];
                var addedToASet = false;
                if ((n.X >= minX) && (n.Y >= minY) && (n.Z >= minZ))
                    addedToASet = addNewCandtoPareto(n, ParetoSets[0], true, true, true);
                if ((!addedToASet) && (n.X >= minX) && (n.Y >= minY) && (n.Z <= maxZ))
                    addedToASet = addNewCandtoPareto(n, ParetoSets[1], true, true, false);
                if ((!addedToASet) && (n.X >= minX) && (n.Y <= maxY) && (n.Z >= minZ))
                    addedToASet = addNewCandtoPareto(n, ParetoSets[2], true, false, true);
                if ((!addedToASet) && (n.X >= minX) && (n.Y <= maxY) && (n.Z <= maxZ))
                    addedToASet = addNewCandtoPareto(n, ParetoSets[3], true, false, false);
                if ((!addedToASet) && (n.X <= maxX) && (n.Y >= minY) && (n.Z >= minZ))
                    addedToASet = addNewCandtoPareto(n, ParetoSets[4], false, true, true);
                if ((!addedToASet) && (n.X <= maxX) && (n.Y >= minY) && (n.Z <= maxZ))
                    addedToASet = addNewCandtoPareto(n, ParetoSets[5], false, true, false);
                if ((!addedToASet) && (n.X <= maxX) && (n.Y <= maxY) && (n.Z >= minZ))
                    addedToASet = addNewCandtoPareto(n, ParetoSets[6], false, false, true);
                if ((!addedToASet) && (n.X <= maxX) && (n.Y <= maxY) && (n.Z <= maxZ))
                    addedToASet = addNewCandtoPareto(n, ParetoSets[7], false, false, false);
                if (!addedToASet)
                {
                    if ((n.X >= maxX) && (n.Y >= maxY) && (n.Z >= maxZ))
                        addNewCandtoPareto(n, ParetoSets[0], true, true, true);
                    if ((n.X >= maxX) && (n.Y >= maxY) && (n.Z <= minZ))
                        addNewCandtoPareto(n, ParetoSets[1], true, true, false);
                    if ((n.X >= maxX) && (n.Y <= minY) && (n.Z >= maxZ))
                        addNewCandtoPareto(n, ParetoSets[2], true, false, true);
                    if ((n.X >= maxX) && (n.Y <= minY) && (n.Z <= minZ))
                        addNewCandtoPareto(n, ParetoSets[3], true, false, false);
                    if ((n.X <= minX) && (n.Y >= maxY) && (n.Z >= maxZ))
                        addNewCandtoPareto(n, ParetoSets[4], false, true, true);
                    if ((n.X <= minX) && (n.Y >= maxY) && (n.Z <= minZ))
                        addNewCandtoPareto(n, ParetoSets[5], false, true, false);
                    if ((n.X <= minX) && (n.Y <= minY) && (n.Z >= maxZ))
                        addNewCandtoPareto(n, ParetoSets[6], false, false, true);
                    if ((n.X <= minX) && (n.Y <= minY) && (n.Z <= minZ))
                        addNewCandtoPareto(n, ParetoSets[7], false, false, false);

                }
            }
            for (int i = 0; i < 8; i++)
                nonDomHull.AddRange(ParetoSets[i]);
            return nonDomHull;

        }

        /// <summary>
        /// Adds the new candidate to the pareto set.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="ParetoCands">The pareto cands.</param>
        static Boolean addNewCandtoPareto(IVertexConvHull c, List<IVertexConvHull> ParetoCands, Boolean maximizeX, Boolean maximizeY,
            Boolean maximizeZ)
        {
            for (int i = ParetoCands.Count - 1; i >= 0; i--)
            {
                IVertexConvHull pc = ParetoCands[i];
                if (dominates(c, pc, maximizeX, maximizeY, maximizeZ))
                    ParetoCands.Remove(pc);
                else if (dominates(pc, c, maximizeX, maximizeY, maximizeZ)) return false;
            }
            ParetoCands.Add(c);
            return true;
        }

        private static bool dominates(IVertexConvHull c, IVertexConvHull pc, bool maximizeX, bool maximizeY, bool maximizeZ)
        {
            var xDominates = (((c.X > pc.X) && (maximizeX)) || ((!maximizeX) && (pc.X > c.X)));
            var yDominates = (((c.Y > pc.Y) && (maximizeY)) || ((!maximizeY) && (pc.Y > c.Y)));
            var zDominates = (((c.Z > pc.Z) && (maximizeZ)) || ((!maximizeZ) && (pc.Z > c.Z)));
            if (xDominates && yDominates && zDominates) return true;
            if (xDominates && yDominates && (c.Z == pc.Z)) return true;
            if (zDominates && yDominates && (c.X == pc.X)) return true;
            if (xDominates && zDominates && (c.Y == pc.Y)) return true;
            if (xDominates && (c.Y == pc.Y) && (c.Z == pc.Z)) return true;
            if (yDominates && (c.X == pc.X) && (c.Z == pc.Z)) return true;
            if (zDominates && (c.Y == pc.Y) && (c.X == pc.X)) return true;
            return false;
        }

        //public static List<IVertexConvHull> Find3D(List<IVertexConvHull> vertices, Type face_Type = null)
        //{
        //    /* first, the original vertices are copied as they will be modified
        //     * by this function. */
        //    faceType = face_Type;
        //    origVertices = new List<IVertexConvHull>(vertices);
        //    return Find3D();
        //}
        //public static List<IVertexConvHull> Find3D(List<IVertexConvHull> vertices, Type face_Type, out List<IFaceConvHull> faces)
        //{
        //    /* first, the original vertices are copied as they will be modified
        //     * by this function. */
        //    faceType = face_Type;
        //    origVertices = new List<IVertexConvHull>(vertices);
        //    var convexVertices = Find3D();
        //    faces = convexFaces;
        //    return convexVertices;
        //}


        ///* These three overloads take longer than the ones above. They are provided in cases
        // * where the users classes and collections are more like these. Ideally, the
        // * user should declare there list of vertices as a List<IVertexConvHull>, but 
        // * this is an unrealistic requirement. At any rate, these methods take about 50  
        // * nano-second to add each one. */
        //public static List<IVertexConvHull> Find3D(IList vertices, Type face_Type = null)
        //{
        //    object[] arrayOfVertices = new object[vertices.Count];
        //    vertices.CopyTo(arrayOfVertices, 0);
        //    return Find3D(arrayOfVertices, face_Type);
        //}
        //public static List<IVertexConvHull> Find3D(object[] vertices, Type face_Type = null)
        //{
        //    faceType = face_Type;
        //    origVertices = new List<IVertexConvHull>();
        //    for (int i = 0; i < vertices.GetLength(0); i++)
        //        origVertices.Add((IVertexConvHull)vertices[i]);
        //    return Find3D();
        //}
        //public static List<IVertexConvHull> Find3D(IList vertices, Type face_Type, IList faces)
        //{
        //    faceType = face_Type;
        //    var convexVertices = Find3D(vertices, faces[0].GetType());
        //    faces = convexFaces;
        //    return convexVertices;
        //}

    }
}