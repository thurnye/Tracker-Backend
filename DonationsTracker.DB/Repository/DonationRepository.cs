using System.Collections.Generic;
using System.Linq;

namespace DonationsTracker.DB.Repositories
{
    public class DonationRepository : IDonationRepository
    {
        private readonly DonationDbContext _context;

        public DonationRepository(DonationDbContext context)
        {
            _context = context;
        }

        public (List<Donation> Items, int TotalCount) GetAll(int page, int limit)
        {
            var query = _context.Donations.AsQueryable();

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(d => d.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return (items, totalCount);
        }


        public Donation? GetById(int id) => _context.Donations.FirstOrDefault(d => d.Id == id);

        public Donation Save(Donation donation)
        {
            if (donation.Id == 0)
                _context.Donations.Add(donation);
            else
                _context.Donations.Update(donation);
            _context.SaveChanges();
            return donation;
        }

        public void Delete(int id)
        {
            var donation = _context.Donations.FirstOrDefault(d => d.Id == id);
            if (donation == null)
                throw new KeyNotFoundException($"Donation with ID {id} not found.");

            _context.Donations.Remove(donation);
            _context.SaveChanges();
        }
    }
}
