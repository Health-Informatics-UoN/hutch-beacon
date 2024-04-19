using AutoMapper;

namespace BeaconBridge.MappingProfiles;

public class FilteringTermProfile : Profile
{
  public FilteringTermProfile()
  {
    CreateMap<Data.Entities.FilteringTerm, Models.FilteringTerm>();
  }
}
