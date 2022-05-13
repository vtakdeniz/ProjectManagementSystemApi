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
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Models.RelationTables;

namespace ProjectManagementSystem.Controllers.UserController
{
    //TODO:Implement error model
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public UserController(ManagementContext context, IMapper mapper, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }
        
        [HttpGet]
        public async Task<ActionResult<ReadUserDto>> GetUserSelfInfo(string id)
        {
            /*var name = User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault()?.Value;
            var email = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            var test = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
            var user = await _context.users.FirstOrDefaultAsync(u => u.UserName == name);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test2 = await _userManager.FindByIdAsync(userId);*/

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<ReadUserDto>(user));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(UpdateUserDto dto) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.notifications).FirstOrDefaultAsync(u => u.Id == userId);
            if (userId == null || user == null)
            {
                return NotFound();
            }
            user.Email = dto.email;
            user.firstName = dto.firstName;
            user.lastName = dto.lastName;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("username")]
        public async Task<ActionResult> UpdateUsername([FromQuery]string userName) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.notifications).FirstOrDefaultAsync(u => u.Id == userId);
            if (userId == null || user == null)
            {
                return NotFound();
            }
            var exists = await _userManager.FindByNameAsync(userName);
            if (exists!=null) {
                return BadRequest();
            }

            await _userManager.SetUserNameAsync(user,userName);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("password")]
        public async Task<ActionResult> UpdatePassword([FromQuery] string current_password,[FromQuery]string new_password)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.notifications).FirstOrDefaultAsync(u => u.Id == userId);
            if (userId == null || user == null)
            {
                return NotFound();
            }

            var res=await _userManager.ChangePasswordAsync(user, current_password, new_password);
            if (res.Succeeded==false) {
                return BadRequest();
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (userId == null || user == null)
            {
                return NotFound();
            }
           
            await _userManager.DeleteAsync(user);
            return NoContent();
        }

        [Route("notification")]
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users
                .Include(u => u.notifications)
                .ThenInclude(n=>n.sender_user)

                .Include(u=>u.notifications)
                .ThenInclude(n=>n.board)

                .Include(u=>u.notifications)
                .ThenInclude(n=>n.job)

                .Include(u => u.notifications)
                .ThenInclude(n => n.project)

                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userId == null || user==null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<List<ReadNotificationDto>>(user.notifications));
        }

        [Route("notification/{id}/accept")]
        [HttpPost]
        public async Task<IActionResult> AcceptNotification(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.notifications).FirstOrDefaultAsync(u => u.Id == userId);
            if (userId == null || user == null)
            {
                return NotFound();
            }
            var notification = user.notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null) {
                return NotFound();
            }

            if (notification.action_type == NotificationConstants.ACTION_TYPE_ASSIGN
                    &&notification.target_type==NotificationConstants.TARGET_PROJECT)
            {
                var project = await _context.projects.FindAsync(notification.project_id);
                if (project == null)
                {
                    return NotFound();
                }
                var assignedRel = new UserAssignedProjects
                {
                    receiver_id=userId,
                    project_id=project.Id,
                    assigner_id=notification.sender_user_id
                };
                await _context.userAssignedProjects.AddAsync(assignedRel);
                user.notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }

            else if (notification.action_type == NotificationConstants.ACTION_TYPE_ASSIGN_ADMIN
                    && notification.target_type == NotificationConstants.TARGET_BOARD)
            {
                var boardHasAdmins = new BoardHasAdmins
                {
                    board_id=notification.board_id,
                    user_id=userId
                };
                var boardHasUsers = new BoardHasUsers
                {
                    user_id=userId,
                    board_id=notification.board_id
                };
                await _context.boardHasAdmins.AddAsync(boardHasAdmins);
                user.notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [Route("notification/{id}/deny")]
        [HttpPost]
        public async Task<IActionResult> DenyNotification(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.notifications).FirstOrDefaultAsync(u => u.Id == userId);
            if (userId == null || user == null)
            {
                return NotFound();
            }
            var notification = user.notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null)
            {
                return NotFound();
            }
            user.notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
