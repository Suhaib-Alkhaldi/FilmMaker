using FilmMaker.Entities;

namespace FilmMaker.Common.SeedData
{
    public static class LookupSeeder
    {
        public static void Seed(FilmMakerDbContext context)
        {
            if (context.LookupCategories.Any())
                return;

            var categories = new List<LookupCategory>
        {
            new LookupCategory
            {
                Name = "LocationStatus",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Active" },
                    new() { Name = "Archived" },
                    new() { Name = "Disabled By Admin" },
                    new() { Name = "Removed By Admin" }
                }
            },

            new LookupCategory
            {
                Name = "MediaType",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Image" },
                    new() { Name = "Video" }
                }
            },

            new LookupCategory
            {
                Name = "BookingStatus",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Pending" },
                    new() { Name = "Accepted" },
                    new() { Name = "Rejected" },
                    new() { Name = "Contract Created" },
                    new() { Name = "Awaiting Contract Approval" },
                    new() { Name = "Contract Signed" },
                    new() { Name = "Payment Pending" },
                    new() { Name = "Confirmed" },
                    new() { Name = "Completed" },
                    new() { Name = "Cancelled" }
                }
            },

            new LookupCategory
            {
                Name = "ContractStatus",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Pending" },
                    new() { Name = "Paid" },
                    new() { Name = "Failed" },
                    new() { Name = "Refunded" },
                    new() { Name = "Partially Refunded" },
                    new() { Name = "Cancelled" }
                }
            },

            new LookupCategory
            {
                Name = "PaymentType",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Advance Payment" },
                    new() { Name = "Full Payment" },
                    new() { Name = "Remaining Payment" },
                    new() { Name = "Refund" },
                    new() { Name = "Cancellation Fee" }
                }
            },

            new LookupCategory
            {
                Name = "EscrowStatus",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Held" },
                    new() { Name = "Released" },
                    new() { Name = "Refunded" },
                    new() { Name = "Partially Refunded" },
                    new() { Name = "Cancelled" }
                }
            },

            new LookupCategory
            {
                Name = "ProductionType",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Film" },
                    new() { Name = "Series" },
                    new() { Name = "Advertisement" },
                    new() { Name = "Documentary" },
                    new() { Name = "Music Video" },
                    new() { Name = "TV Show" },
                    new() { Name = "Short Film" },
                    new() { Name = "Commercial" },
                    new() { Name = "Other" }
                }
            },

            new LookupCategory
            {
                Name = "ServiceType",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Equipment" },
                    new() { Name = "Food And Catering" },
                    new() { Name = "Cars" },
                    new() { Name = "Lighting" },
                    new() { Name = "Security" },
                    new() { Name = "Caravans" },
                    new() { Name = "Clothes And Wardrobe" },
                    new() { Name = "Makeup" },
                    new() { Name = "Props" },
                    new() { Name = "Sound Equipment" },
                    new() { Name = "Transportation" },
                    new() { Name = "Accommodation" },
                    new() { Name = "Cleaning" },
                    new() { Name = "Medical Support" },
                    new() { Name = "Drone Services" },
                    new() { Name = "Other" }
                }
            },

            new LookupCategory
            {
                Name = "City",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Riyadh" },
                    new() { Name = "Jeddah" },
                    new() { Name = "Makkah" },
                    new() { Name = "Madinah" },
                    new() { Name = "Dammam" },
                    new() { Name = "Khobar" },
                    new() { Name = "Dhahran" },
                    new() { Name = "Abha" },
                    new() { Name = "Taif" },
                    new() { Name = "Tabuk" },
                    new() { Name = "Hail" },
                    new() { Name = "AlUla" },
                    new() { Name = "Buraidah" },
                    new() { Name = "Najran" },
                    new() { Name = "Jazan" },
                    new() { Name = "Yanbu" }
                }
            },

            new LookupCategory
            {
                Name = "Country",
                LookupItems = new List<LookupItem>
                {
                    new() { Name = "Saudi Arabia" },
                    new() { Name = "Jordan" },
                    new() { Name = "United Arab Emirates" },
                    new() { Name = "Qatar" },
                    new() { Name = "Kuwait" },
                    new() { Name = "Bahrain" },
                    new() { Name = "Oman" },
                    new() { Name = "Egypt" },
                    new() { Name = "Lebanon" },
                    new() { Name = "Other" }
                }
            },

            new LookupCategory
            {
                Name = "LocationType",
                LookupItems = new List <LookupItem>
                {
                    new() {Name = "House"},
                    new() {Name = "Apartment"},
                    new() {Name = "Villa"},
                    new() {Name = "Shop"},
                    new() {Name = "Land"},
                    new() {Name = "Farm"},
                    new() {Name = "Office"},
                    new() {Name = "Studio"},
                    new() {Name = "Warehouse"},
                    new() {Name = "Restaurant"},
                    new() {Name = "Cafe"},
                }
            },

            new LookupCategory
            {
                Name = "VisitStatus",
                LookupItems = new List<LookupItem>
                {
                    new() {Name = "Pending"},
                    new() {Name = "Accepted"},
                    new() {Name = "Rejected"},
                    new() {Name = "Cancelled"}
                }
            }
        };

            context.LookupCategories.AddRange(categories);
            context.SaveChanges();
        }
    }
}
