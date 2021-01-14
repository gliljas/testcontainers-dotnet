using System.Threading.Tasks;

namespace TestContainers.Images.Builder
{
    public interface ITaskSource<T>
    {
        Task<T> Task { get; }
    }
}
