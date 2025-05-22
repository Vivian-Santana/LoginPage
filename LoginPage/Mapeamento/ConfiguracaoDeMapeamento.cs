using AutoMapper;
using LoginPage.DTOs;
using LoginPage.Modelo;

namespace LoginPage.Mapeamento
{
    public class ConfiguracaoDeMapeamento : Profile
    {
        public ConfiguracaoDeMapeamento()
        {
            // Mapeamento de entidade para resposta (saída)
            CreateMap<UsuarioModelo, RespostaUsuario>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name));

            // Mapeamento de request (entrada) para entidade
            CreateMap<UsuarioRequest, UsuarioModelo>()
                .ForMember(dest => dest.SenhaHash, opt => opt.Ignore());
        }

    }
}
