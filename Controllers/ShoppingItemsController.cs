using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShoppingList.Dtos;
using ShoppingList.Entities;
using ShoppingList.Helpers;
using ShoppingList.Services;

namespace ShoppingListServer.Controllers
{
    [Route("api/[controller]")]
    public class ShoppingItemsController : Controller
    {
        private IShoppingItemService _shoppingItemService;
        private IMapper _mapper;
        private readonly IOptions<AppSettings> _appSettings;

        public ShoppingItemsController(IShoppingItemService shoppingItemService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _shoppingItemService = shoppingItemService;
            _mapper = mapper;
            _appSettings = appSettings;
        }

        [HttpPost]
        public IActionResult AddShoppingItem([FromBody]ShoppingItemDto shoppingItemDto)
        {
            var shoppingItem = _mapper.Map<ShoppingItem>(shoppingItemDto);

            try
            {
                var created = _shoppingItemService.CreateShoppingItem(shoppingItem);
                shoppingItemDto.Id = created.Id;
                
                return Ok(shoppingItemDto);
            }
            catch(AppException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShoppingItems()
        {
            var shoppingItems = await _shoppingItemService.GetAllShoppingItems();
            var shoppingItemDtos = _mapper.Map<IList<ShoppingItemDto>>(shoppingItems);
            return Ok(shoppingItemDtos);
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetShoppingItemsByUserId(string userId)
        {
            var shoppingItems = await _shoppingItemService.GetShoppingItemsByUserId(userId);
            var shoppingItemDtos = _mapper.Map<IList<ShoppingItemDto>>(shoppingItems);
            return Ok(shoppingItemDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShoppingItemById(string id)
        {
            var shoppingItem = await _shoppingItemService.GetShoppingItemById(id);
            var shoppingItemDto = _mapper.Map<ShoppingItemDto>(shoppingItem);
            return Ok(shoppingItemDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateShoppingItem(string id, [FromBody]ShoppingItemDto shoppingItemDto)
        {
            var shoppingItem = _mapper.Map<ShoppingItem>(shoppingItemDto);
            shoppingItemDto.Id = id;

            try 
            {
                _shoppingItemService.UpdateShoppingItem(shoppingItem);
                return Ok(shoppingItemDto);
            }
            catch(AppException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteShoppingItem(string id)
        {
            _shoppingItemService.RemoveShoppingItem(id);
            return Ok();
        }
    }
}