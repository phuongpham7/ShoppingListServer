using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShoppingList.Dtos;
using ShoppingList.Entities;
using ShoppingList.Helpers;
using ShoppingList.Services;

namespace ShoppingListServer.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly IOptions<AppSettings> _appSettings;

        public UsersController(IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings;
        }

        //[AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate ([FromBody]UserDto userDto)
        {
            var user = await _userService.Authenticate(userDto.Email, userDto.Password);

            if (user == null)    return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Value.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.Name, user.Id)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new {
                            Id = user.Id,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Token = tokenString
                        });
        }

        //[AllowAnonymous]
        [HttpPost]
        public IActionResult Register([FromBody]UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);

            var tempUser = _userService.GetUserByEmail(user.Email);

            if (tempUser.Result != null)    
                return StatusCode(409);
            
            try
            {
                _userService.CreateUser(user, userDto.Password);
                return Ok();
            }
            catch(AppException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            var userDtos = _mapper.Map<IList<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserById(id);
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(string id, [FromBody]UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            userDto.Id = id;

            try 
            {
                _userService.UpdateUser(user, userDto.Password);
                return Ok();
            }
            catch(AppException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(string id)
        {
            _userService.RemoveUser(id);
            return Ok();
        }
    }
}