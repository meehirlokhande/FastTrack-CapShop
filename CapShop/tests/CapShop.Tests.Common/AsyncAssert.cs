namespace CapShop.Tests.Common;

/// <summary>Async exception capture for NUnit when classic helpers are unavailable.</summary>
public static class AsyncAssert
{
    public static async Task<Exception?> CatchAsync(Func<Task> act)
    {
        try
        {
            await act();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
