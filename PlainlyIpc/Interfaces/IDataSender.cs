using System.Threading.Tasks;

namespace PlainlyIpc.Interfaces;

public interface IDataSender
{
    public Task SendAsync(byte[] data);

}
