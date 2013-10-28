/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2013 David Sehnal, Matthew Campbell
 *
 *  This library is free software; you can redistribute it and/or modify it 
 *  under the terms of  the GNU Lesser General Public License as published by 
 *  the Free Software Foundation; either version 2.1 of the License, or 
 *  (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful, 
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of 
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser 
 *  General Public License for more details.
 *  
 *****************************************************************************/

namespace MIConvexHull
{
    using System.Collections.Generic;

    /// <summary>
    /// A helper class for object allocation/storage. 
    /// This helps the GC a lot as it prevents the creation of about 75% of 
    /// new face objects (in the case of ConvexFaceInternal). In the case of
    /// FaceConnectors and DefferedFaces, the difference is even higher (in most
    /// cases O(1) vs O(number of created faces)). 
    /// </summary>
    class ObjectManager
    {
        readonly int Dimension;

        Stack<ConvexFaceInternal> RecycledFaceStack;
        Stack<FaceConnector> ConnectorStack;
        Stack<VertexBuffer> EmptyBufferStack;
        Stack<DeferredFace> DeferredFaceStack;

        public void DepositFace(ConvexFaceInternal face)
        {
            for (int i = 0; i < Dimension; i++)
            {
                face.AdjacentFaces[i] = null;
            }
            RecycledFaceStack.Push(face);
        }

        public ConvexFaceInternal GetFace()
        {
            return RecycledFaceStack.Count != 0
                    ? RecycledFaceStack.Pop()
                    : new ConvexFaceInternal(Dimension, GetVertexBuffer());
        }

        public void DepositConnector(FaceConnector connector)
        {
            ConnectorStack.Push(connector);
        }

        public FaceConnector GetConnector()
        {
            return ConnectorStack.Count != 0
                    ? ConnectorStack.Pop()
                    : new FaceConnector(Dimension);
        }

        public void DepositVertexBuffer(VertexBuffer buffer)
        {
            buffer.Clear();
            EmptyBufferStack.Push(buffer);
        }

        public VertexBuffer GetVertexBuffer()
        {
            return EmptyBufferStack.Count != 0 ? EmptyBufferStack.Pop() : new VertexBuffer();
        }

        public void DepositDeferredFace(DeferredFace face)
        {
            DeferredFaceStack.Push(face);
        }

        public DeferredFace GetDeferredFace()
        {
            return DeferredFaceStack.Count != 0 ? DeferredFaceStack.Pop() : new DeferredFace();
        }
                
        public ObjectManager(int dimension)
        {
            this.Dimension = dimension;

            RecycledFaceStack = new Stack<ConvexFaceInternal>();
            ConnectorStack = new Stack<FaceConnector>();
            EmptyBufferStack = new Stack<VertexBuffer>();
            DeferredFaceStack = new Stack<DeferredFace>();
        }
    }
}
