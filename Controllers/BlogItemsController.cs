using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduTechBlogsApi.Models;
using EduTechBlogsApi.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using EduTechBlogsApi.Models.Helpers;

namespace EduTechBlogsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles =UserRoles.Author+","+UserRoles.Reader)]
    public class BlogItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BlogItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/BlogItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogItem>>> Getblogs()
        {
            var blogItems = await _context.blogs.Include(b => b.User).ToListAsync();
            foreach (var blogItem in blogItems)
            {
                if (blogItem.User != null)
                {
                    blogItem.UserDto = new UserDto
                    {
                        Id = blogItem.User.Id,
                        FirstName = blogItem.User.FirstName,
                        LastName = blogItem.User.LastName,
                        UserName = blogItem.User.UserName,
                        Email = blogItem.User.Email
                    };
                }
            }
            return blogItems;
        }

        // GET: api/BlogItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogItem>> GetBlogItem(int id)
        {
            var blogItem = await _context.blogs.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);

            if (blogItem == null)
            {
                return NotFound();
            }

            if (blogItem.User != null)
            {
                blogItem.UserDto = new UserDto
                {
                    Id = blogItem.User.Id,
                    FirstName = blogItem.User.FirstName,
                    LastName = blogItem.User.LastName,
                    UserName = blogItem.User.UserName,
                    Email = blogItem.User.Email
                };
            }

            return blogItem;
        }


        // PUT: api/BlogItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Author)]
        public async Task<IActionResult> PutBlogItem(int id, BlogItem blogItem)
        {
            if (id != blogItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(blogItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogItemExists(id))
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

        // POST: api/BlogItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = UserRoles.Author)]
        public async Task<ActionResult<BlogItem>> PostBlogItem(BlogItem blogItem)
        {
            _context.blogs.Add(blogItem);
            await _context.SaveChangesAsync();

            // Load the User entity
            await _context.Entry(blogItem).Reference(b => b.User).LoadAsync();

            if (blogItem.User != null)
            {
                blogItem.UserDto = new UserDto
                {
                    Id = blogItem.User.Id,
                    FirstName = blogItem.User.FirstName,
                    LastName = blogItem.User.LastName,
                    UserName = blogItem.User.UserName,
                    Email = blogItem.User.Email
                };
            }

            return CreatedAtAction("GetBlogItem", new { id = blogItem.Id }, blogItem);
        }

        // DELETE: api/BlogItems/5
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Author)]
        public async Task<IActionResult> DeleteBlogItem(int id)
        {
            var blogItem = await _context.blogs.FindAsync(id);
            if (blogItem == null)
            {
                return NotFound();
            }

            _context.blogs.Remove(blogItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlogItemExists(int id)
        {
            return _context.blogs.Any(e => e.Id == id);
        }
    }
}
