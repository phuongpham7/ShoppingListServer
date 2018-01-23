using AutoMapper;
using ShoppingList.Dtos;
using ShoppingList.Entities;

namespace ShoppingList.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();

            CreateMap<ShoppingItem, ShoppingItemDto>();
            CreateMap<ShoppingItemDto, ShoppingItem>();
        }
    }
}