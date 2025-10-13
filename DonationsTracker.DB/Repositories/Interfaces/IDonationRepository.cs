using System.Collections.Generic;
using DonationsTracker.DB;

namespace DonationsTracker.DB.Repositories
{
    public interface IDonationRepository
    {
       ( List<Donation> Items, int TotalCount) GetAll(int page, int limit);
        Donation? GetById(int id);
        Donation Save(Donation donation);
        void Delete(int id);
    }
}
