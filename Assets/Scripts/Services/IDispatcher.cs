namespace DroneDispatcher.Services
{
public interface IDispatcher
{
    DispatchResult Assign(string jobId, string droneId);
}

public struct DispatchResult
{
    public bool Success;
    public string Message;

    public static DispatchResult Ok(string msg) => new DispatchResult { Success = true, Message = msg };
    public static DispatchResult Fail(string msg) => new DispatchResult { Success = false, Message = msg };
}
}
