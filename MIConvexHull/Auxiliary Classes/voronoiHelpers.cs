using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIConvexHull
{
    class voronoiVertex
    {
        public IVertexConvHull vertex { get; set; }
        public FaceData face { get; set; }
    }

    class voronoiEdge : IEquatable<voronoiEdge>
    {
        public voronoiVertex a { get; set; }
        public voronoiVertex b { get; set; }

        public bool Equals(voronoiEdge other)
        {
            return (object.ReferenceEquals(a.face, other.a.face) && object.ReferenceEquals(b.face, other.b.face)) ||
                   (object.ReferenceEquals(a.face, other.b.face) && object.ReferenceEquals(b.face, other.a.face));
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ b.GetHashCode();
        }
    }
}
