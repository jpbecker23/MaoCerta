using Microsoft.AspNetCore.Mvc;
using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;

namespace MaoCerta.API.Controllers
{
    /// <summary>
    /// Controller for managing reviews
    /// Implements RESTful API endpoints for review operations
    /// </summary>
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(
            IReviewService reviewService,
            ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all reviews
        /// </summary>
        /// <returns>List of reviews</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a review by ID
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>Review details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            try
            {
                var review = await _reviewService.GetByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }
                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new review
        /// </summary>
        /// <param name="createReviewDto">Review data</param>
        /// <returns>Created review</returns>
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewDto createReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var review = await _reviewService.CreateAsync(createReviewDto);
                return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating review");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument creating review");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="updateReviewDto">Updated review data</param>
        /// <returns>Updated review</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto updateReviewDto)
        {
            try
            {
                if (id != updateReviewDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var review = await _reviewService.UpdateAsync(updateReviewDto);
                return Ok(review);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument updating review");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var result = await _reviewService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets reviews by professional ID
        /// </summary>
        /// <param name="professionalId">Professional ID</param>
        /// <returns>List of reviews for the professional</returns>
        [HttpGet("professional/{professionalId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByProfessional(int professionalId)
        {
            try
            {
                var reviews = await _reviewService.GetByProfessionalIdAsync(professionalId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for professional with ID {ProfessionalId}", professionalId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets reviews by client ID
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>List of reviews from the client</returns>
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByClient(int clientId)
        {
            try
            {
                var reviews = await _reviewService.GetByClientIdAsync(clientId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for client with ID {ClientId}", clientId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets average rating for a professional
        /// </summary>
        /// <param name="professionalId">Professional ID</param>
        /// <returns>Average rating</returns>
        [HttpGet("professional/{professionalId}/average-rating")]
        public async Task<ActionResult<double>> GetAverageRating(int professionalId)
        {
            try
            {
                var averageRating = await _reviewService.GetAverageRatingAsync(professionalId);
                return Ok(new { averageRating });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving average rating for professional with ID {ProfessionalId}", professionalId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets review by service request ID
        /// </summary>
        /// <param name="serviceRequestId">Service request ID</param>
        /// <returns>Review data</returns>
        [HttpGet("service-request/{serviceRequestId}")]
        public async Task<ActionResult<ReviewDto>> GetByServiceRequest(int serviceRequestId)
        {
            try
            {
                var review = await _reviewService.GetByServiceRequestIdAsync(serviceRequestId);
                if (review == null)
                {
                    return NotFound();
                }
                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review for service request with ID {ServiceRequestId}", serviceRequestId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

