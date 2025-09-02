using AutoMapper;
using PlanningInvestment.Application.DTOs.Investment;
using PlanningInvestment.Domain.Entities;

namespace PlanningInvestment.Application.Profiles;

/// <summary>
///     AutoMapper profile for Investment mappings. (EN)<br />
///     Profile AutoMapper cho các ánh xạ Investment. (VI)
/// </summary>
public class InvestmentProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the InvestmentProfile class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp InvestmentProfile. (VI)
    /// </summary>
    public InvestmentProfile()
    {
        // Investment Entity to ViewModel mapping
        // Ánh xạ từ thực thể Investment sang ViewModel
        CreateMap<Investment, InvestmentViewModel>();

        // CreateRequest to Investment Entity mapping
        // Ánh xạ từ CreateRequest sang thực thể Investment
        CreateMap<CreateInvestmentRequest, Investment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentMarketPrice, opt => opt.Ignore()); // Will be set separately if provided

        // UpdateRequest to Investment Entity mapping
        // Ánh xạ từ UpdateRequest sang thực thể Investment
        CreateMap<UpdateInvestmentRequest, Investment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // UserId should not be updated
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreateBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateBy, opt => opt.Ignore());
    }
}
