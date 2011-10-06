namespace MIConvexHull
{
    using System.Collections.Generic;
    using System.Linq;
    
    public static class ConvexHull
    {
        public static ConvexHull<TVertex, TFace> Create<TVertex, TFace>(IEnumerable<TVertex> data)
            where TVertex : IVertex
            where TFace : ConvexFace<TVertex, TFace>, new()
        {
            return ConvexHull<TVertex, TFace>.Create(data);
        }

        public static ConvexHull<TVertex, DefaultConvexFace<TVertex>> Create<TVertex>(IEnumerable<TVertex> data)
            where TVertex : IVertex
        {
            return ConvexHull<TVertex, DefaultConvexFace<TVertex>>.Create(data);
        }
    }

    public class ConvexHull<TVertex, TFace>
        where TVertex : IVertex
        where TFace : ConvexFace<TVertex, TFace>, new()
    {
        public IEnumerable<TVertex> Hull { get; private set; }
        public IEnumerable<TFace> Faces { get; private set; }

        public static ConvexHull<TVertex, TFace> Create(IEnumerable<TVertex> data)
        { 
            var ch = ConvexHullInternal.GetConvexHullAndFaces<TVertex, TFace>(data.Cast<IVertex>());

            return new ConvexHull<TVertex, TFace> { Hull = ch.Item1, Faces = ch.Item2 };
        }

        private ConvexHull()
        {

        }
    }
}
