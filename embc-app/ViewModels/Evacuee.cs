using AutoMapper;
using Gov.Jag.Embc.Public.Utils;
using System;
using static Gov.Jag.Embc.Public.Models.Db.Enumerations;

namespace Gov.Jag.Embc.Public.ViewModels
{
    public class EvacueeMappingProfie : Profile
    {
        public EvacueeMappingProfie()
        {
            CreateMap<Models.Db.Evacuee, EvacueeListItem>()
                .ForMember(d => d.Id, opts => opts.MapFrom(s => $"{s.RegistrationId.ToString()}-{s.EvacueeSequenceNumber}"))
                .ForMember(d => d.IsHeadOfHousehold, opts => opts.MapFrom(s => s.EvacueeType == Models.Db.Enumerations.EvacueeType.HeadOfHousehold))
                .ForMember(d => d.RestrictedAccess, opts => opts.MapFrom(s => s.EvacueeRegistration.RestrictedAccess))
                .ForMember(d => d.IncidentTaskNumber, opts => opts.MapFrom(s => s.EvacueeRegistration.IncidentTask.TaskNumber))
                .ForMember(d => d.RegistrationCompletionDate, opts => opts.MapFrom(s => s.EvacueeRegistration.RegistrationCompletionDate))
                .ForMember(d => d.EvacuatedFrom, opts => opts.MapFrom(s => s.EvacueeRegistration.HostCommunity.Name))
                .ForMember(d => d.EvacuatedTo, opts => opts.MapFrom(s => s.EvacueeRegistration.IncidentTask.Community != null
                                                                        ? s.EvacueeRegistration.IncidentTask.Community.Name
                                                                        : s.EvacueeRegistration.IncidentTask.Region.Name))
                .ForMember(d => d.Dob, opts => opts.MapFrom(s => s.Dob.HasValue ? s.Dob.Value.ToString("yyyy-MM-dd") : null))

                //In order to avoid n+1 when using s.EvacueeRegistration.Referrals.Count(), this uses a magic property to count the referrals
                .ForMember(d => d.NumberOfReferrals, opts => opts.MapFrom(s => s.EvacueeRegistration.NumberOfReferrals))
                .ForMember(d => d.PrimaryAddress, opts => opts.MapFrom(s => s.EvacueeRegistration.PrimaryAddress == null ? null : s.EvacueeRegistration.PrimaryAddress.AddressLine1))
                .ForMember(d => d.City, opts => opts.MapFrom(s => s.EvacueeRegistration.PrimaryAddress == null ? null : s.EvacueeRegistration.PrimaryAddress.City))
                .ForMember(d => d.Province, opts => opts.MapFrom(s => s.EvacueeRegistration.PrimaryAddress == null ? null : s.EvacueeRegistration.PrimaryAddress.Province))
                .ForMember(d => d.Country, opts => opts.MapFrom(s => s.EvacueeRegistration.PrimaryAddress == null ? null : s.EvacueeRegistration.PrimaryAddress.Country))
                .ForMember(d => d.PostalCode, opts => opts.MapFrom(s => s.EvacueeRegistration.PrimaryAddress == null ? null : s.EvacueeRegistration.PrimaryAddress.PostalCode))
                .ForMember(d => d.MailingAddressLine, opts => opts.MapFrom(s => s.EvacueeRegistration.MailingAddress == null ? null : s.EvacueeRegistration.MailingAddress.AddressLine1))
                .ForMember(d => d.MailingCity, opts => opts.MapFrom(s => s.EvacueeRegistration.MailingAddress == null ? null : s.EvacueeRegistration.MailingAddress.City))
                .ForMember(d => d.MailingProvince, opts => opts.MapFrom(s => s.EvacueeRegistration.MailingAddress == null ? null : s.EvacueeRegistration.MailingAddress.Province))
                .ForMember(d => d.MailingCountry, opts => opts.MapFrom(s => s.EvacueeRegistration.MailingAddress == null ? null : s.EvacueeRegistration.MailingAddress.Country))
                .ForMember(d => d.MailingPostalCode, opts => opts.MapFrom(s => s.EvacueeRegistration.MailingAddress == null ? null : s.EvacueeRegistration.MailingAddress.PostalCode))
                ;

            CreateMap<Models.Db.ViewEvacuee, EvacueeListItem>();

            CreateMap<EvacueeType, FamilyRelationshipType>()
                .ForMember(x => x.Active, opts => opts.MapFrom(s => true))
                .ForMember(x => x.Code, opts => opts.MapFrom(s => s.GetDisplayName()))
                .ForMember(x => x.Description, opts => opts.MapFrom(s => s.GetDescription()));
        }
    }

    public class EvacueeListItem
    {
        public string Id { get; set; }
        public bool RestrictedAccess { get; set; }
        public bool IsHeadOfHousehold { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; }
        public string Initials { get; set; }
        public string RegistrationId { get; set; }
        public string IncidentTaskNumber { get; set; }
        public string EvacuatedFrom { get; set; }
        public string EvacuatedTo { get; set; }
        public DateTime? RegistrationCompletionDate { get; set; }
        public bool IsFinalized { get => RegistrationCompletionDate.HasValue; }
        public bool? HasReferrals { get => NumberOfReferrals > 0; }
        public string Dob { get; set; }
        public DateTime? SelfRegisteredDate { get; set; }
        public string PrimaryAddress { get => PrimaryAddressLine; }
        public string City { get => PrimaryCity; }
        public string Province { get => PrimaryProvince; }
        public string Country { get => PrimaryCountry; }

        public string PostalCode { get => PrimaryPostalCode; }
        public string PrimaryAddressLine { get; set; }
        public string PrimaryCity { get; set; }
        public string PrimaryCountry { get; set; }
        public string PrimaryPostalCode { get; set; }
        public string PrimaryProvince { get; set; }
        public string MailingAddressLine { get; set; }
        public string MailingCity { get; set; }
        public string MailingCountry { get; set; }
        public string MailingPostalCode { get; set; }
        public string MailingProvince { get; set; }
        public DateTime? IncidentStartDate { get => TaskNumberStartDate?.DateTime; }
        public DateTime? IncidentEndDate { get => TaskNumberEndDate?.DateTime; }
        public DateTimeOffset? TaskNumberStartDate { get; set; }
        public DateTimeOffset? TaskNumberEndDate { get; set; }
        public string RegistrationLocation { get; set; }
        public int NumberOfReferrals { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberAlt { get; set; }
        public string InsuranceCode { get; set; }
        public bool? HasInsurance { get => InsuranceCode.StartsWith("yes", StringComparison.OrdinalIgnoreCase); }
        public bool? HasPets { get; set; }

        //TODO: add service recommendations
    }
}
