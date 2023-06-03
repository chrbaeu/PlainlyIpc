using System.Linq.Expressions;

namespace PlainlyIpc.Interfaces;

/// <summary>
/// Interface for IPC handler implementations.
/// </summary>
public interface IIpcHandler : IIpcSender, IIpcReceiver
{

    /// <summary>
    /// Registers a service.
    /// </summary>
    /// <param name="type">The type of the service.</param>
    /// <param name="service">The service instance.</param>
    void RegisterService(Type type, object service);

    /// <summary>
    /// Registers a service.
    /// </summary>
    /// <typeparam name="TIService">The type of the service.</typeparam>
    /// <param name="service">The service instance.</param>
    void RegisterService<TIService>(TIService service) where TIService : notnull;

    /// <summary>
    /// Executes a async remote procedure call.
    /// </summary>
    /// <typeparam name="TIRemoteService">The type of the remote interface.</typeparam>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <param name="func">The function to call.</param>
    /// <returns>The result.</returns>
    Task<TResult> ExecuteRemote<TIRemoteService, TResult>(Expression<Func<TIRemoteService, Task<TResult>>> func);

    /// <summary>
    /// Executes a remote procedure call.
    /// </summary>
    /// <typeparam name="TIRemoteService">The type of the remote interface.</typeparam>
    /// <typeparam name="TResult">The type of the expected result.</typeparam>
    /// <param name="func">The function to call.</param>
    /// <returns>The result.</returns>
    Task<TResult> ExecuteRemote<TIRemoteService, TResult>(Expression<Func<TIRemoteService, TResult>> func);


    /// <summary>
    /// Executes a remote procedure call.
    /// </summary>
    /// <typeparam name="TIRemoteService">The type of the remote interface.</typeparam>
    /// <param name="func">The function to call.</param>
    Task ExecuteRemote<TIRemoteService>(Expression<Action<TIRemoteService>> func);

}
