using System;

namespace MIConvexHull
{
    public class ConvexHullGenerationException : Exception
    {
        public ConvexHullGenerationException(ConvexHullCreationResultOutcome error, string errorMessage)
        {
            ErrorMessage = errorMessage;
            Error        = error;
        }

        public string ErrorMessage { get; }

        public ConvexHullCreationResultOutcome Error { get; }
    }
}