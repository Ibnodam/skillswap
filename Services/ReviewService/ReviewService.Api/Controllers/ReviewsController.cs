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
    private readonly IHttpClientFactory _httpClientFactory;

    public ReviewsController(ReviewDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
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
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }


    [HttpGet("ratings")]
    public async Task<ActionResult> GetRatings([FromQuery] List<Guid> userIds)
    {
        if (userIds == null || !userIds.Any())
            return Ok(new List<object>());

        var ratings = await _context.Reviews
            .Where(r => userIds.Contains(r.ToUserId))
            .GroupBy(r => r.ToUserId)
            .Select(g => new
            {
                UserId = g.Key,
                AverageRating = Math.Round(g.Average(r => r.Rating), 1),
                ReviewsCount = g.Count()
            })
            .ToListAsync();

        return Ok(ratings);
    }



    // GET /api/reviews/my/{userId} — мой отзыв о пользователе
    [HttpGet("my/{userId:guid}")]
    public async Task<ActionResult> GetMyReview(Guid userId)
    {
        var currentUserId = JwtMiddleware.GetUserId(HttpContext);
        if (currentUserId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.FromUserId == currentUserId && r.ToUserId == userId);

        if (review == null)
            return Ok(new { exists = false });

        return Ok(new
        {
            exists = true,
            review = new ReviewResponse
            {
                Id = review.Id,
                FromUserId = review.FromUserId,
                FromUserName = review.FromUserName,
                Rating = review.Rating,
                Text = review.Text,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            }
        });
    }

    // GET /api/reviews/can-review/{userId}
    [HttpGet("can-review/{userId:guid}")]
    public async Task<ActionResult> CanReviewUser(Guid userId)
    {
        var currentUserId = JwtMiddleware.GetUserId(HttpContext);
        if (currentUserId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        // Проверяем, есть ли уже отзыв
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.FromUserId == currentUserId && r.ToUserId == userId);

        if (existingReview != null)
            return Ok(new { canReview = false, reason = "already_reviewed", reviewId = existingReview.Id });

        // Проверяем наличие контакта через HTTP-запросы к другим сервисам
        var hasContact = await CheckContactAsync(currentUserId, userId);

        return Ok(new { canReview = hasContact, reason = hasContact ? null : "no_contact" });
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

        // Проверяем, что отзыва ещё нет
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.FromUserId == userId && r.ToUserId == request.ToUserId);

        if (existingReview != null)
            return BadRequest(new { message = "Вы уже оставили отзыв этому пользователю. Вы можете отредактировать или удалить существующий." });

        // Проверяем наличие контакта
        var hasContact = await CheckContactAsync(userId, request.ToUserId);
        if (!hasContact)
            return BadRequest(new { message = "Нельзя оставить отзыв без контакта с пользователем (чат или встреча)" });

        var review = new Review
        {
            Id = Guid.NewGuid(),
            FromUserId = userId,
            FromUserName = userName,
            ToUserId = request.ToUserId,
            ToUserName = request.ToUserName,
            Rating = request.Rating,
            Text = request.Text?.Trim() ?? "",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserReviews), new { userId = review.ToUserId }, new ReviewResponse
        {
            Id = review.Id,
            FromUserId = review.FromUserId,
            FromUserName = review.FromUserName,
            Rating = review.Rating,
            Text = review.Text,
            CreatedAt = review.CreatedAt
        });
    }

    // PUT /api/reviews/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateReviewRequest request)
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return NotFound(new { message = "Отзыв не найден" });

        if (review.FromUserId != userId)
            return StatusCode(403, new { message = "Вы можете редактировать только свои отзывы" });

        review.Rating = request.Rating;
        review.Text = request.Text?.Trim() ?? "";
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Отзыв обновлён" });
    }

    // DELETE /api/reviews/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = JwtMiddleware.GetUserId(HttpContext);
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Токен не предоставлен" });

        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return NotFound(new { message = "Отзыв не найден" });

        if (review.FromUserId != userId)
            return StatusCode(403, new { message = "Вы можете удалять только свои отзывы" });

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Отзыв удалён" });
    }

    // Проверка контакта между пользователями
    private async Task<bool> CheckContactAsync(Guid userId1, Guid userId2)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // Проверяем встречи
            var meetingsResponse = await client.GetAsync($"http://localhost:5003/api/meetings/check-contact?userId1={userId1}&userId2={userId2}");
            if (meetingsResponse.IsSuccessStatusCode)
            {
                var hasMeetings = await meetingsResponse.Content.ReadFromJsonAsync<bool>();
                if (hasMeetings) return true;
            }

            // Проверяем чат
            var chatResponse = await client.GetAsync($"http://localhost:5005/api/messages/check-contact?userId1={userId1}&userId2={userId2}");
            if (chatResponse.IsSuccessStatusCode)
            {
                var hasChat = await chatResponse.Content.ReadFromJsonAsync<bool>();
                if (hasChat) return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка проверки контакта: {ex.Message}");
        }

        return false;
    }
}











//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ReviewService.Api.Data;
//using ReviewService.Api.DTOs;
//using ReviewService.Api.Middleware;
//using ReviewService.Api.Models;

//namespace ReviewService.Api.Controllers;

//[ApiController]
//[Route("api/reviews")]
//public class ReviewsController : ControllerBase
//{
//    private readonly ReviewDbContext _context;

//    public ReviewsController(ReviewDbContext context)
//    {
//        _context = context;
//    }

//    // GET /api/reviews/user/{userId}
//    [HttpGet("user/{userId:guid}")]
//    public async Task<ActionResult> GetUserReviews(Guid userId)
//    {
//        var reviews = await _context.Reviews
//            .Where(r => r.ToUserId == userId)
//            .OrderByDescending(r => r.CreatedAt)
//            .Select(r => new ReviewResponse
//            {
//                Id = r.Id,
//                FromUserId = r.FromUserId,
//                FromUserName = r.FromUserName,
//                Rating = r.Rating,
//                Text = r.Text,
//                CreatedAt = r.CreatedAt
//            })
//            .ToListAsync();

//        return Ok(reviews);
//    }

//    // POST /api/reviews
//    [HttpPost]
//    public async Task<ActionResult> Create([FromBody] CreateReviewRequest request)
//    {
//        var userId = JwtMiddleware.GetUserId(HttpContext);
//        var userName = JwtMiddleware.GetUserName(HttpContext);

//        if (userId == Guid.Empty)
//            return Unauthorized(new { message = "Токен не предоставлен" });

//        if (userId == request.ToUserId)
//            return BadRequest(new { message = "Нельзя оставить отзыв самому себе" });

//        var review = new Review
//        {
//            Id = Guid.NewGuid(),
//            FromUserId = userId,
//            FromUserName = userName,
//            ToUserId = request.ToUserId,
//            ToUserName = request.ToUserName,
//            MeetingId = request.MeetingId,
//            Rating = request.Rating,
//            Text = request.Text,
//            CreatedAt = DateTime.UtcNow
//        };

//        _context.Reviews.Add(review);
//        await _context.SaveChangesAsync();

//        return CreatedAtAction(nameof(GetUserReviews), new { userId = review.ToUserId }, review);
//    }
//}