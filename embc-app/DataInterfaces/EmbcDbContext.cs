using Gov.Jag.Embc.Public.Models.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Gov.Jag.Embc.Public.DataInterfaces
{
    /// <summary>
    /// Database Context Factory Interface
    /// </summary>
    public interface IEmbcDbContextFactory
    {
        /// <summary>
        /// Create new database context
        /// </summary>
        /// <returns></returns>
        EmbcDbContext Create();
    }

    /// <summary>
    /// Database Context Factory
    /// </summary>
    public class EmbcDbContextFactory : IEmbcDbContextFactory
    {
        private readonly DbContextOptions<EmbcDbContext> options;
        private readonly IHttpContextAccessor ctx;

        /// <summary>
        /// Database Context Factory Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="options"></param>
        public EmbcDbContextFactory(IHttpContextAccessor httpContextAccessor, DbContextOptions<EmbcDbContext> options)
        {
            this.options = options;
            ctx = httpContextAccessor;
        }

        /// <summary>
        /// Create new database context
        /// </summary>
        /// <returns></returns>
        public EmbcDbContext Create()
        {
            return new EmbcDbContext(ctx, options);
        }
    }

    public class EmbcDbContext : DbContext
    {
        private readonly IHttpContextAccessor ctx;

        public EmbcDbContext(DbContextOptions<EmbcDbContext> options) : base(options)
        {
        }

        public EmbcDbContext(IHttpContextAccessor httpContextAccessor, DbContextOptions<EmbcDbContext> options) : this(options)
        {
            ctx = httpContextAccessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .AddEnvironmentVariables()
                   .Build();
                string connectionString = DatabaseTools.GetConnectionString(configuration);
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        public DbSet<Community> Communities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Evacuee> Evacuees { get; set; }
        public DbSet<FamilyRelationshipType> FamilyRelationshipTypes { get; set; }
        public DbSet<EvacueeRegistration> EvacueeRegistrations { get; set; }
        public DbSet<IncidentTask> IncidentTasks { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<EvacueeRegistrationAddress> EvacueeRegistrationAddresses { get; set; }
        public DbSet<EvacueeRegistrationAudit> EvacueeRegistrationAudits { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<VolunteerTask> VolunteerTasks { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ViewEvacuee> ViewEvacuees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // this line is required so ef migrations will work.
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Evacuee>()
                .HasKey(e => new { e.RegistrationId, e.EvacueeSequenceNumber });

            modelBuilder.Entity<VolunteerTask>()
            .HasAlternateKey(e => e.IncidentTaskId);

            modelBuilder.Entity<VolunteerTask>()
            .HasAlternateKey(e => e.VolunteerId);

            modelBuilder.Entity<VolunteerTask>()
            .HasOne(e => e.Volunteer)
            .WithMany(v => v.VolunteerTasks)
            .HasForeignKey(e => e.VolunteerId);

            modelBuilder.Entity<VolunteerTask>()
            .HasOne(e => e.IncidentTask)
            .WithMany(v => v.VolunteerTasks)
            .HasForeignKey(e => e.IncidentTaskId);

            modelBuilder.Entity<VolunteerTask>()
                .Property(p => p.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<VolunteerTask>()
                .HasIndex(p => new { p.VolunteerId, p.IncidentTaskId }).IsUnique();

            modelBuilder.Entity<Evacuee>()
                .HasOne(e => e.EvacueeRegistration)
                .WithMany(r => r.Evacuees)
                .HasForeignKey(e => e.RegistrationId)
                .HasPrincipalKey(r => r.EssFileNumber);

            modelBuilder.Entity<EvacueeRegistrationAddress>()
                .HasKey(a => new { a.RegistrationId, a.AddressSequenceNumber });

            modelBuilder.Entity<EvacueeRegistrationAddress>()
                .HasOne<EvacueeRegistration>()
                .WithMany(r => r.EvacueeRegistrationAddresses)
                .HasForeignKey(e => e.RegistrationId)
                .HasPrincipalKey(r => r.EssFileNumber);

            modelBuilder.HasSequence<long>("ESSFileNumbers")
                .StartsAt(100000)
                .IncrementsBy(1);

            modelBuilder.Entity<EvacueeRegistration>()
                .Property(r => r.EssFileNumber)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEXT VALUE FOR ESSFileNumbers");

            modelBuilder.HasSequence<long>("seq_ReferralIds")
               .StartsAt(1000001)
               .IncrementsBy(1);

            modelBuilder.Entity<Referral>()
              .Property(r => r.Id)
              .ValueGeneratedOnAdd()
              .HasDefaultValueSql("NEXT VALUE FOR seq_ReferralIds");

            modelBuilder.Entity<ReferralEvacuee>()
                .HasKey(e => new { e.RegistrationId, e.EvacueeId, e.ReferralId });

            modelBuilder.Entity<ReferralEvacuee>()
                .HasOne(e => e.Referral)
                .WithMany(e => e.Evacuees)
                .HasForeignKey(e => e.ReferralId);

            modelBuilder.Entity<ReferralEvacuee>()
                .HasOne(e => e.Evacuee)
                .WithMany(r => r.Referrals)
                .HasForeignKey(e => new { e.RegistrationId, e.EvacueeId })
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Referral>()
                .Property<string>("Type")
                .HasMaxLength(50);

            modelBuilder.Entity<Referral>()
                .HasDiscriminator<string>("Type")
                .HasValue<ClothingReferral>(ReferralType.Clothing.ToString())
                .HasValue<IncidentalsReferral>(ReferralType.Incidentals.ToString())
                .HasValue<GroceriesFoodReferral>(ReferralType.Food_Groceries.ToString())
                .HasValue<RestaurantFoodReferral>(ReferralType.Food_Restaurant.ToString())
                .HasValue<TaxiTransportationReferral>(ReferralType.Transportation_Taxi.ToString())
                .HasValue<OtherTransportationReferral>(ReferralType.Transportation_Other.ToString())
                .HasValue<HotelLodgingReferral>(ReferralType.Lodging_Hotel.ToString())
                .HasValue<GroupLodgingReferral>(ReferralType.Lodging_Group.ToString())
                .HasValue<BilletingLodgingReferral>(ReferralType.Lodging_Billeting.ToString());

            modelBuilder.Entity<EvacueeRegistrationAudit>()
                .HasOne(typeof(EvacueeRegistration))
                .WithMany()
                .HasForeignKey("EssFileNumber");

            modelBuilder.AddShadowProperties();

            modelBuilder.Query<ViewModels.EvacueeListItem>();
        }

        public override int SaveChanges()
        {
            SetShadowProperties(ctx?.HttpContext?.User);

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            SetShadowProperties(ctx?.HttpContext?.User);

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetShadowProperties(ClaimsPrincipal principal)
        {
            var timestamp = DateTime.UtcNow;
            var userId = principal?.FindFirstValue(ClaimTypes.Sid) ?? "System";
            foreach (var entry in ChangeTracker.Entries().Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified)))
            {
                if (entry.Entity is IAuditableEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property("CreatedDateTime").CurrentValue = timestamp;
                        entry.Property("CreatedByUserId").CurrentValue = userId;
                    }

                    entry.Property("UpdateDateTime").CurrentValue = timestamp;
                    entry.Property("UpdatedByUserId").CurrentValue = userId;
                }
            }
        }

        private const string EvacueeListItemQuery = @"
SELECT

    e.RegistrationId,
    e.EvacueeSequenceNumber,
    t.TaskNumber,
    t.TaskNumberStartDate,
    t.TaskNumberEndDate,
    er.Active,
    er.RegistrationCompletionDate,
    er.SelfRegisteredDate,
    er.Facility,
    from_c.Name AS EvacuatedFrom,
    ISNULL(to_c.Name, t.RegionName) AS EvacuatedTo,
    e.EvacueeTypeCode,
    e.FirstName,
    e.LastName,
    CAST(e.Dob AS VARCHAR(10)) AS Dob,
    e.Gender,
    er.PhoneNumber,
    er.PhoneNumberAlt,
    er.Email,
    adp.AddressLine1 AS Primary_AddressLine,
    adp.City AS Primary_City,
    (SELECT Name FROM Communities WHERE Id = adp.CommunityId) AS Primary_Community,
    (SELECT Name FROM Countries WHERE CountryCode = adp.CountryCode) AS Primary_Country,
    adp.Province AS Primary_Province,
    adp.PostalCode AS Primary_PostalCode,
    adm.AddressLine1 AS Mailing_AddressLine,
    adm.City AS Mailing_City,
    (SELECT Name FROM Communities WHERE Id = adm.CommunityId) AS Mailing_Community,
    (SELECT Name FROM Countries WHERE CountryCode = adm.CountryCode) AS Mailing_Country,
    adm.Province AS Mailing_Province,
    adm.PostalCode AS Mailing_PostalCode,
    (SELECT COUNT(*) FROM Referrals r WHERE r.RegistrationId = e.RegistrationId) AS NumberOfReferrals,
    er.HasPets,
    er.InsuranceCode,
    er.HasInquiryReferral,
    er.HasHealthServicesReferral,
    er.HasFirstAidReferral,
    er.HasPersonalServicesReferral,
    er.HasChildCareReferral,
    er.HasPetCareReferral
FROM

    Evacuees e
INNER JOIN

    EvacueeRegistrations er ON e.RegistrationId = er.EssFileNumber
LEFT OUTER JOIN
    IncidentTasks t ON er.IncidentTaskId = t.Id
LEFT OUTER JOIN
    Communities from_c ON t.CommunityId = from_c.Id
LEFT OUTER JOIN
    Communities to_c ON er.HostCommunityId = to_c.Id
LEFT OUTER JOIN
    EvacueeRegistrationAddresses adp ON e.RegistrationId = adp.RegistrationId AND adp.AddressTypeCode = 'Primary'
LEFT OUTER JOIN
    EvacueeRegistrationAddresses adm ON e.RegistrationId = adm.RegistrationId AND adm.AddressTypeCode = 'Mailing'
";
    }
}
