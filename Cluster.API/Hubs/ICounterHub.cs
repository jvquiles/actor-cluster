using System.Threading.Tasks;

namespace Cluster.API.Hubs
{
    public interface ICounterHub
    {
        Task UpdateCounter(string key, long count);
    }
}