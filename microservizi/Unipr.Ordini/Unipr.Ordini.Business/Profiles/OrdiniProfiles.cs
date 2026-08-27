using AutoMapper;
using Ordini.Repository.Model;
using Ordini.Shared;
using System.Diagnostics.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ordini.Business.Profiles;

public sealed class AssemblyMarker
{
    AssemblyMarker() { }
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class InputFileProfile : Profile
{
    public InputFileProfile()
    {
        CreateMap<OrdineInsertDto, Ordine>();
        CreateMap<Ordine, OrdineInsertDto>();
        CreateMap<OrdineReadDto, Ordine>();
        CreateMap<Ordine, OrdineReadDto>();
    }
}