namespace Cluster.API.Actors.RealTime
{
    public class IncrementRequest
    {
        public string Key { get; }

        public IncrementRequest(string key)
        {
            this.Key = key;
        }
    }
}