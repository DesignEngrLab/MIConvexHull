namespace MIConvexHull
{
    public enum ConvexHullCreationResultOutcome
    {
        Success,
        DimensionSmallerTwo,
        NotEnoughVerticesForDimension,
        NonUniformDimension,
        DegenerateData,
        UnknownError
    }
}