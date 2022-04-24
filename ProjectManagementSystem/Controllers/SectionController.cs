using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Dto.ReadBoardDto;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.UserElements;
//using Swashbuckle.AspNetCore.Annotations;

namespace ProjectManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SectionController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public SectionController(ManagementContext context, IMapper mapper, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }



        //[SwaggerOperation(Summary = "Remove a section from the board")]
        [HttpDelete("removesection/{id}")]
        public async Task<ActionResult> DeleteSection(int id)
        {
            var user = await GetIdentityUser();

            var sectionFromRepo = await _context.sections.Include(section => section.board).FirstAsync(section => section.Id == id);

            if (sectionFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                  .AnyAsync(rel => rel.project_id == sectionFromRepo.board.project_id &&
                      rel.user_id == user.Id)
                  ||
                  await _context.boardHasAdmins
                  .AnyAsync(rel => rel.board_id == sectionFromRepo.board.Id && rel.user_id == user.Id)
                  ;

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            _context.sections.Remove(sectionFromRepo);
            await _context.SaveChangesAsync();

            return Ok();
        }


        //[SwaggerOperation(Summary = "Add a section to the board")]
        [HttpPost("addsection")]
        public async Task<ActionResult<CreateSectionDto>> AddSection(CreateSectionDto sectionDto)
        {
            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.FindAsync(sectionDto.board_id);

            if (boardFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == boardFromRepo.project_id &&
                    rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == boardFromRepo.Id && rel.user_id == user.Id)
                ;

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var section = _mapper.Map<Section>(sectionDto);

            await _context.sections.AddAsync(section);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("addsection", sectionDto);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        private async Task<User> GetIdentityUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            return user;
        }
    }
}
