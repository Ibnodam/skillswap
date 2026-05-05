using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviewService.Api.Data;
using ReviewService.Api.DTOs;
using ReviewService.Api.Middleware;
using ReviewService.Api.Models;

namespace ReviewService.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ReviewDbContext _context;

    public ReviewsController(ReviewDbContext context)
    {
        _context = context;
    }

    // GET /api/reviews/user/{userId}
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult> GetUserReviews(Guid userId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ToUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                FromUserId = r.FromUserId,
                FromUserName = r.FromUserName,
                Rating = r.Rating,
                Text = r.Text,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }

    // POST /api/reviews
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateReviewRequest request)
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        var userName = JwtMiddleware.GetUserName(HttpContext);

        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        if (userId == request.ToUserId)
            return BadRequest(new { message = "Нельзя оставить отзыв самому себе" });

        var review = new Review
        {
            Id = Guid.NewGuid(),
            FromUserId = userId,
            FromUserName = userName,
            ToUserId = request.ToUserId,
            ToUserName = request.ToUserName,
            MeetingId = request.MeetingId,
            Rating = request.Rating,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserReviews), new { userId = review.ToUserId }, review);
    }
}