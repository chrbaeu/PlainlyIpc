using System;
using System.Threading.Tasks;

namespace PlainlyIpc.Interfaces;

public interface IIpcSender
{
    public Task SendAsync(byte[] data);
    public Task SendAsync(ReadOnlyMemory<byte> data);
    public Task SendStringAsync(string data);
    public Task SendObjectAsync<T>(T data);

}
