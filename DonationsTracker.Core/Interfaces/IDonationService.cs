using DonationsTracker.DB;

namespace DonationsTracker.Core;

public interface IDonationService
{
    (List<Donation> Items, int TotalCount) GetDonations(int page, int limit);
    Donation GetDonationById(int id);
    Donation CreateAndUpdate(Donation donation);
    void DeleteDonation(int id);
    

}
