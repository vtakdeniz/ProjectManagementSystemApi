using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Controllers.UserControllers
{
    [Route("api/admin/user")]
    [ApiController]
    //[Authorize(Roles=UserRoles.Admin)]
    [Produces("application/json")]
    public class AdminUserController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public AdminUserController(ManagementContext context, IMapper mapper, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadUserDto>>> Getusers()
        {

            var users = await _context.users.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ReadUserDto>>(users));
        }

    }
}
