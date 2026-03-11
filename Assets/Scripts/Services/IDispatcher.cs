namespace DroneDispatcher.Services
{
// Interface for the dispatch service.
// We use interfaces so we can easily swap implementations or mock them for testing.
// The Dispatcher is the only class that implements this right now.
public interface IDispatcher
{
    DispatchResult Assign(string jobId, string droneId);
}

// Simple result struct returned by Assign() — tells the caller if it worked + a message.
// Using a struct instead of throwing exceptions keeps the flow clean and testable.
public struct DispatchResult
{
    public bool Success;
    public string Message;

    public static DispatchResult Ok(string msg) => new DispatchResult { Success = true, Message = msg };
    public static DispatchResult Fail(string msg) => new DispatchResult { Success = false, Message = msg };
}
}
