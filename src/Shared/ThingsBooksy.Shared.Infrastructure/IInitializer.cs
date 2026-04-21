using System.Threading.Tasks;

namespace ThingsBooksy.Shared.Infrastructure;

public interface IInitializer
{
    Task InitAsync();
}