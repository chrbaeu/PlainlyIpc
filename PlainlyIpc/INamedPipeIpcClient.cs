using System.Threading.Tasks;

namespace PlainlyIpc;

public interface INamedPipeIpcClient
{
    public void Connect();
    public Task ConnectAsync();
    public void Send(string data);
    public void Send<T>(T data);
    public Task SendAsync(string data);
    public Task SendAsync<T>(T data);

}
