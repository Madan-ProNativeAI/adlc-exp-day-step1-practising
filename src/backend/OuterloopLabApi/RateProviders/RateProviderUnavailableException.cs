namespace OuterloopLabApi.RateProviders;

public sealed class RateProviderUnavailableException : Exception
{
    public RateProviderUnavailableException() : base("Rate provider unavailable")
    {
    }
}
