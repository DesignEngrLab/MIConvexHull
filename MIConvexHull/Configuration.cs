/******************************************************************************
 *
 * The MIT License (MIT)
 *
 * MIConvexHull, Copyright (c) 2015 David Sehnal, Matthew Campbell
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *  
 *****************************************************************************/

using System;

namespace MIConvexHull
{
    internal static class Constants
    {
        internal const double DefaultPlaneDistanceTolerance = 1e-5;
        internal const double DefaultShiftRadius = 1e-6;
        internal const double DefaultZeroCellVolumeTolerance = 1e-5;
        internal const double MaxDotProductInSimplex = 0.995;
        /// <summary>
        /// The connector table size
        /// </summary>
        internal const int ConnectorTableSize = 2017;
    }
    /// <summary>
    /// Determines the type of the point translation to use.
    /// This is useful for handling "degenerate" data (i.e. uniform grids of points).
    /// </summary>
    public enum PointTranslationType
    {
        /// <summary>
        /// Nothing happens.
        /// </summary>
        None,

        /// <summary>
        /// The points are only translated internally, the vertexes in the result
        /// retain their original coordinates.
        /// </summary>
        TranslateInternal
    }

    /// <summary>
    /// Configuration of the convex hull computation.
    /// </summary>
    public class ConvexHullComputationConfig
    {
        /// <summary>
        /// Create the config with default values set.
        /// </summary>
        public ConvexHullComputationConfig()
        {
            PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance;
            PointTranslationType = PointTranslationType.None;
            PointTranslationGenerator = null;
        }

        /// <summary>
        /// This value is used to determine which vertexes are eligible
        /// to be part of the convex hull.
        /// As an example, imagine a line with 3 points:
        /// A ---- C ---- B
        /// Points A and B were already determined to be on the hull.
        /// Now, the point C would need to be at least 'PlaneDistanceTolerance'
        /// away from the line determined by A and B to be also considered
        /// a hull point.
        /// </summary>
        /// <value>The plane distance tolerance.</value>
        public double PlaneDistanceTolerance { get; set; }

        /// <summary>
        /// Determines what method to use for point translation.
        /// This helps with handling "degenerate" data such as uniform grids.
        /// Default = None
        /// </summary>
        /// <value>The type of the point translation.</value>
        public PointTranslationType PointTranslationType { get; set; }

        /// <summary>
        /// A function used to generate translation direction.
        /// This function is called for each coordinate of each point as
        /// Position[i] -&gt; Position[i] + PointTranslationGenerator()
        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// From my testing the function should be set up so that the
        /// translation magnitude is lower than the PlaneDistanceTolerance.
        /// Otherwise, flat faces in triangulation could be created as a result.
        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// An example of the translation function that would shift each coordinate
        /// in 0.0000005 in either direction is:
        /// var rnd = new Random(0); // use the same seed for each computation
        /// f = () =&gt; 0.000001 * rnd.NextDouble() - 0.0000005;
        /// This is implemented by the
        /// ConvexHullComputationConfig.RandomShiftByRadius function.
        /// Default = null
        /// </summary>
        /// <value>The point translation generator.</value>
        public Func<double> PointTranslationGenerator { get; set; }

        /// <summary>
        /// Closures the specified radius.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="rnd">The random.</param>
        /// <returns>Func&lt;System.Double&gt;.</returns>
        private static Func<double> Closure(double radius, Random rnd)
        {
            return () => radius * (rnd.NextDouble() - 0.5);
        }

        /// <summary>
        /// Creates values in range (-radius / 2, radius / 2)
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <param name="randomSeed">If null, initialized to random default System.Random value</param>
        /// <returns>Func&lt;System.Double&gt;.</returns>
        public static Func<double> RandomShiftByRadius(double radius = Constants.DefaultShiftRadius,
            int? randomSeed = 0)
        {
            Random rnd;
            if (randomSeed.HasValue) rnd = new Random(randomSeed.Value);
            else rnd = new Random();
            return Closure(radius, rnd);
        }
    }

    /// <summary>
    /// Configuration of the triangulation computation.
    /// </summary>
    /// <seealso cref="MIConvexHull.ConvexHullComputationConfig" />
    public class TriangulationComputationConfig : ConvexHullComputationConfig
    {
        /// <summary>
        /// Create the config with default values set.
        /// </summary>
        public TriangulationComputationConfig()
        {
            ZeroCellVolumeTolerance = Constants.DefaultZeroCellVolumeTolerance;
        }

        /// <summary>
        /// If using PointTranslationType.TranslateInternal, this value is
        /// used to determine which boundary cells have zero volume after the
        /// points get "translated back".
        /// </summary>
        /// <value>The zero cell volume tolerance.</value>
        public double ZeroCellVolumeTolerance { get; set; }
    }
}