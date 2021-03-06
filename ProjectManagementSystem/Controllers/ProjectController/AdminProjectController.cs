using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Controllers.ProjectControllers
{
    [Route("api/admin/proj")]
    [ApiController]
    //[Authorize(Roles = UserRoles.Admin)]
    [Produces("application/json")]
    public class AdminProjectController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public AdminProjectController(ManagementContext context, IMapper mapper, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }
        // GET: api/Project
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadProjectDto>>> Getprojects()
        {
            var projects = await _context.projects.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ReadProjectDto>>(projects));

        }

        // GET: api/Project/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadProjectDto>> GetProject(int id)
        {
            var project = await _context.projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ReadProjectDto>(project));
        }

        // PUT: api/Project/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest();
            }

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        // POST: api/Project
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ReadProjectDto>> PostProject(CreateProjectDto projectDto)
        {
            var project = _mapper.Map<Project>(projectDto);
            await _context.projects.AddAsync(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.Id }, _mapper.Map<ReadProjectDto>(project));
        }

        // DELETE: api/Project/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool ProjectExists(int id)
        {
            return _context.projects.Any(e => e.Id == id);
        }
    }
}
