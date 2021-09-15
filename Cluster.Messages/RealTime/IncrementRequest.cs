namespace Cluster.Messages.RealTime
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