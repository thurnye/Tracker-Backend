using System.Text.Json;
using DonationsTracker.Core.Entity;
using DonationsTracker.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace DonationsTracker.DB.Seed
{
    public static class TransactionSeeder
    {
        public static async Task SeedTransactionsAsync(DonationDbContext context)
        {
            if (await context.Transactions.AnyAsync())
            {
                Console.WriteLine("Transactions already exist — skipping seed.");
                return;
            }

            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Seed", "transactions.json");
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($" Seed file not found at {jsonPath}");
                return;
            }

            Console.WriteLine("📥 Reading transactions.json...");
            var json = await File.ReadAllTextAsync(jsonPath);
            var rawTransactions = JsonSerializer.Deserialize<List<RawTransaction>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            // Fixed category ID pool
            var categoryPool = new List<string>
            {
                "00fd1c9d-78f7-4c45-8434-b0aab050e400",
                "085b0fa7-2b24-4f61-b7fc-c0bb5eef6318",
                "08C3EA75-C933-4B35-A994-B08450795A1E",
                "3EC5399E-7848-4DBA-B827-CE870DF5855D",
                "44B3C6B2-8281-4801-8B14-CFC1874BE990",
                "4A5F58A7-8C7B-4FAF-AE49-35BA160AD4DC",
                "53534AB6-BB00-4525-A0BC-954B73C8994B",
                "58CB31B5-D611-466D-9782-2DA54A13C04F",
                "681B4487-5780-4E15-BCB7-11585C006B3D",
                "71005D51-CF81-441E-8D00-84950228ED55",
                "7FDB498F-8FFF-4C97-8934-D7778665D7C9",
                "8D62E07D-5CA8-4A4F-A447-197612074E9F",
                "91551488-EE03-4384-B3B2-5636C7880C5C",
                "AEBDFAEA-7ED0-4AA3-BF20-06553A9F456E",
                "B0F541A7-48FC-4A2D-9181-BB8AE95934A0",
                "B3CB6A64-93DF-4D3A-A63B-EFDB6CC280DA",
                "B99B3948-CE11-4A65-876B-B5F8BA931690",
                "BA93CE71-F232-42D4-830E-AD9F83E055AC",
                "C431D378-7D8A-4259-A97D-F53844772843",
                "D089C15A-020F-4EEA-9A49-27CCD7F8D355",
                "D3F51479-B22F-45ED-9ACE-A6DF88589C03",
                "DBDC6001-F2CB-485A-9F84-05A034D1C76E",
                "E023A363-1D70-4172-A56E-13EF11BCBD82",
                "E2D20F3C-937B-42DB-89F1-BFF397125C67",
                "FEF51C2D-5EF9-4E1F-AC32-D9FC723CCF88"
            };

            var random = new Random();
            var transactions = new List<Transaction>();
            int skipped = 0;

            foreach (var raw in rawTransactions)
            {
                try
                {
                    if (raw == null)
                    {
                        skipped++;
                        continue;
                    }

                    var isCredit = string.Equals(raw.TransactionType, "credit", StringComparison.OrdinalIgnoreCase);
                    var walletId = isCredit
                        ? "3a33dbf4-02f8-43ef-b546-28bada90220f"
                        : "35443dfc-0099-471b-a6b0-cf920581cc31";

                    // Pick a random categoryId from the fixed list
                    var categoryId = categoryPool[random.Next(categoryPool.Count)];

                    DateTime transactionDate = DateTime.TryParse(raw.TransactionDate, out var parsedDate)
                        ? parsedDate
                        : DateTime.UtcNow;

                    var transaction = new Transaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = "a8d428d0-2d14-4701-93b8-6cca63401f5d",
                        WalletId = walletId,
                        CategoryId = categoryId,
                        TransactionDate = transactionDate,
                        TransactionAmount = raw.TransactionAmount,
                        TransactionType = string.IsNullOrWhiteSpace(raw.TransactionType) ? "transfer" : raw.TransactionType,
                        MerchantName = raw.MerchantName ?? "Unknown Merchant",
                        Description = raw.Description ?? "No description provided",
                        CurrencyCode = raw.CurrencyCode ?? "CAD",
                        Status = raw.Status ?? "pending",
                        Location = raw.Location ?? "unspecified",
                        Method = raw.Method ?? "in-store",
                        Reference = string.IsNullOrWhiteSpace(raw.Reference)
                            ? Guid.NewGuid().ToString()
                            : raw.Reference,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    transactions.Add(transaction);
                }
                catch (Exception ex)
                {
                    skipped++;
                    Console.WriteLine($"Skipped record due to error: {ex.Message}");
                }
            }

            Console.WriteLine($"🚀 Preparing to seed {transactions.Count} valid transactions ({skipped} skipped).");

            foreach (var batch in transactions.Chunk(500))
            {
                await context.Transactions.AddRangeAsync(batch);
                await context.SaveChangesAsync();
                Console.WriteLine($"Saved batch of {batch.Length} records...");
            }

            Console.WriteLine("Transaction seeding completed successfully!");
        }

        private class RawTransaction
        {
            public string? Id { get; set; }
            public string? WalletId { get; set; }
            public string? TransactionDate { get; set; }
            public decimal TransactionAmount { get; set; }
            public string? TransactionType { get; set; }
            public string? MerchantName { get; set; }
            public string? Description { get; set; }
            public string? CurrencyCode { get; set; }
            public string? CategoryId { get; set; }
            public string? Status { get; set; }
            public string? Location { get; set; }
            public string? Method { get; set; }
            public string? Reference { get; set; }
        }
    }
}
