using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;

namespace TestContainers.Utility
{
    public interface ITransferable
    {
        int FileMode { get; }
        long Size { get; }

        byte[] GetBytes();

        string Description { get; }
    }

}
