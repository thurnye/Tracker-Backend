using DonationsTracker.Core.RequestModel;

namespace DonationsTracker.Core.Helpers
{
    public class PaginatedCache<T>
    {
        public T Data { get; set; } = default!;
        public PaginationMeta Meta { get; set; } = new();
    }
}
