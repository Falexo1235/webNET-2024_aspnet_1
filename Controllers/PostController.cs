using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly BlogDbContext _context;

        public PostController(BlogDbContext context)
        {
            _context = context;
        }
        [HttpGet]
public async Task<IActionResult> GetPosts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? author = null,
    [FromQuery] int? min = null,
    [FromQuery] int? max = null,
    [FromQuery] bool onlyMyCommunities = false,
    [FromQuery] string? sorting = null,
    [FromQuery] List<Guid>? tags = null)
{
        var userId = User.Identity.IsAuthenticated
        ? Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value)
        : Guid.Empty;
    var postsQuery = _context.Posts
        .Include(p => p.PostTags)
        .Include(p => p.Comments)
        .AsQueryable();

    if (tags != null && tags.Any())
    {
        postsQuery = postsQuery.Where(p =>
            tags.All(tagId => p.PostTags.Any(pt => pt.TagId == tagId)));
    }
    if (!string.IsNullOrEmpty(author))
    {
        postsQuery = postsQuery.Where(p =>
            _context.Users.Any(u => u.Id == p.AuthorId && EF.Functions.ILike(u.FullName, $"%{author}%")));
    }
    if (min.HasValue)
    {
        postsQuery = postsQuery.Where(p => p.ReadingTime >= min.Value);
    }
    if (max.HasValue)
    {
        postsQuery = postsQuery.Where(p => p.ReadingTime <= max.Value);
    }
    if (onlyMyCommunities && userId != Guid.Empty)
    {
        var userCommunityIds = await _context.CommunityUsers
            .Where(cu => cu.UserId == userId)
            .Select(cu => cu.CommunityId)
            .ToListAsync();

        postsQuery = postsQuery.Where(p => p.CommunityId.HasValue && userCommunityIds.Contains(p.CommunityId.Value));
    }
    postsQuery = sorting switch
    {
        "CreateAsc" => postsQuery.OrderBy(p => p.CreateTime),
        "CreateDesc" => postsQuery.OrderByDescending(p => p.CreateTime),
        "LikesAsc" => postsQuery.OrderBy(p => _context.Likes.Count(l => l.PostId == p.Id)),
        "LikesDesc" => postsQuery.OrderByDescending(p => _context.Likes.Count(l => l.PostId == p.Id)),
        _ => postsQuery.OrderByDescending(p => p.CreateTime)
    };
    var totalPosts = await postsQuery.CountAsync();
    var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);
    var posts = await postsQuery
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new
        {
            p.Title,
            p.Description,
            p.ReadingTime,
            p.Image,
            AuthorId = p.AuthorId,
            Author = _context.Users.FirstOrDefault(u => u.Id == p.AuthorId).FullName,
            p.CommunityId,
            p.CommunityName,
            p.AddressId,
            Likes = _context.Likes.Count(l => l.PostId == p.Id),
            HasLike = userId != Guid.Empty
                ? _context.Likes.Any(l => l.PostId == p.Id && l.UserId == userId)
                : false,
            CommentsCount = p.Comments.Count,
            Tags = p.PostTags.Select(pt => new { pt.Tag.Id, pt.Tag.Name, pt.Tag.CreateTime }).ToList(),  // Получаем теги через PostTags
            p.Id,
            p.CreateTime,
        })
        .ToListAsync();
    var pagination = new
    {
        Size = pageSize,
        Count = totalPages,
        Current = page
    };
    return Ok(new { Posts = posts, Pagination = pagination });
}



        [HttpGet("{id}")]
public async Task<IActionResult> GetPostById(Guid id)
{
    var post = await _context.Posts
        .Include(p => p.Comments)
        .FirstOrDefaultAsync(p => p.Id == id);
    if (post == null)
        return NotFound("Post not found.");
    var postAuthor = await _context.Users.FindAsync(post.AuthorId);
    post.Author = postAuthor?.FullName ?? "Unknown";
    int totalCommentCount = 0;
    foreach (var comment in post.Comments.Where(c => c.ParentId == null))
    {
        var commentAuthor = await _context.Users.FindAsync(comment.AuthorId);
        comment.Author = commentAuthor?.FullName ?? "Unknown";
        comment.SubComments = await CountSubCommentsRecursively(comment.Id);
        totalCommentCount += comment.SubComments + 1;
    }
     bool hasLike = false;
    if (User.Identity.IsAuthenticated)
    {
        var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        hasLike = await _context.Likes.AnyAsync(l => l.PostId == post.Id && l.UserId == userId);
    }
    return Ok(new
    {
        Comments = post.Comments.Where(c => c.ParentId == null)
            .Select(c => new
            {
                c.Content,
                c.ModifiedDate,
                c.DeleteDate,
                c.AuthorId,
                c.Author,
                c.SubComments,
                c.Id,
                c.CreateTime
            }),
        post.Title,
        post.Description,
        post.ReadingTime,
        post.Image,
        post.AuthorId,
        post.Author,
        post.CommunityId,
        post.CommunityName,
        Likes = await _context.Likes.CountAsync(l => l.PostId == post.Id),
        HasLike = hasLike,
        CommentsCount = totalCommentCount,
        post.Tags,
        post.Id,
        post.CreateTime,
    });
}
private async Task<int> CountSubCommentsRecursively(Guid parentId)
{
    var directSubComments = await _context.Comments
        .Where(c => c.ParentId == parentId)
        .ToListAsync();

    var totalCount = directSubComments.Count;

    foreach (var subComment in directSubComments)
    {
        totalCount += await CountSubCommentsRecursively(subComment.Id);
    }

    return totalCount;
}

[HttpPost("{id}/comment")]
        [Authorize]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] CommentDto dto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var comment = new Comment
            {
                Content = dto.Content,
                CreateTime = DateTime.UtcNow,
                AuthorId = userId,
                PostId = id,
                ParentId = dto.ParentId,
                ModifiedDate = null,
                DeleteDate = null
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Comment added successfully." });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            if (dto.Tags == null || !dto.Tags.Any())
                return BadRequest("At least one tag must be specified.");
            if (!string.IsNullOrEmpty(dto.Image) && !Uri.IsWellFormedUriString(dto.Image, UriKind.Absolute))
                return BadRequest("Invalid image URL.");
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authorId))
                return Unauthorized("Invalid token.");
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                ReadingTime = dto.ReadingTime,
                Image = dto.Image,
                AddressId = dto.AddressId == Guid.Empty? null: dto.AddressId,
                AuthorId = authorId,
                CreateTime = DateTime.UtcNow,
                Tags = new List<Tag>()
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var tags = await _context.Tags.Where(t => dto.Tags.Contains(t.Id)).ToListAsync();
            if (tags.Count != dto.Tags.Count)
                return BadRequest("One or more tags are invalid.");
            var postTags = tags.Select(tag => new PostTag
            {
                PostId = post.Id,
                TagId = tag.Id
            }).ToList();

            _context.PostTags.AddRange(postTags);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Post created successfully." });
        }

        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> AddLike(Guid postId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (existingLike != null)
                return BadRequest("You have already liked this post.");

            var like = new Like
            {
                PostId = postId,
                UserId = userId
            };
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Post liked successfully." });
        }

        [HttpDelete("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> RemoveLike(Guid postId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null)
                return NotFound("Like not found.");

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Like removed successfully." });
        }
    }
}
