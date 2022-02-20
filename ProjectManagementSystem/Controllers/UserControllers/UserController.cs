using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models.UserElements;
using AutoMapper;
using ProjectManagementSystem.Dto.UserDto;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace ProjectManagementSystem.Controllers.UserControllers
{
    //TODO:Add repository
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> userManager;

        public UserController(ManagementContext context, IMapper mapper, UserManager<User> userManager)
        {
            this.userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        // GET: api/User
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Getusers()
        {

            var users=await _context.users.ToListAsync();
            return  Ok(_mapper.Map<IEnumerable<ReadUserDto>>(users));
        }

        
        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var name = User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault()?.Value;
            var email = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;

            var user = await _context.users.FirstOrDefaultAsync(u => u.UserName == name);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ReadUserDto>(user));
        }

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /*[HttpPost]
        public async Task<ActionResult<User>> PostUser(RegisterUserDto userDto)
        {

            var userFromRepo = _context.users.Any(u => u.userName == userDto.userName);
            if (userFromRepo) {
                return BadRequest("DuplicateUserName");
            }

            var user = _mapper.Map<User>(userDto);

            await _context.users.AddAsync(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, _mapper.Map<ReadUserDto>(user));
        }*/

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(string id)
        {
            return _context.users.Any(e => e.Id == id);
        }


        [HttpPost("/AddTaskUser/{id}")]
        public async Task<ActionResult<User>> AddTaskUser(int id,RegisterUserDto userDto)
        {
            throw new NotImplementedException();
        }



    }
}
