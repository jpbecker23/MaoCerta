using Microsoft.AspNetCore.Mvc;
using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;
using MaoCerta.Domain.Interfaces;
using MaoCerta.Domain.Entities;

namespace MaoCerta.API.Controllers
{
    /// <summary>
    /// Controller for managing categories
    /// Implements RESTful API endpoints for category operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            IUnitOfWork unitOfWork,
            ILogger<CategoriesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <returns>List of categories</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                var categoryDtos = categories.Select(c => MapToDto(c, professionals));
                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                return Ok(MapToDto(category, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new category
        /// </summary>
        /// <param name="createCategoryDto">Category data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = new Category
                {
                    Name = createCategoryDto.Name,
                    Description = createCategoryDto.Description,
                    Icon = createCategoryDto.Icon,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category created successfully with ID {CategoryId}", category.Id);
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, MapToDto(category, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="updateCategoryDto">Updated category data</param>
        /// <returns>Updated category</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (id != updateCategoryDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                category.Name = updateCategoryDto.Name;
                category.Description = updateCategoryDto.Description;
                category.Icon = updateCategoryDto.Icon;
                category.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category updated successfully with ID {CategoryId}", id);
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                return Ok(MapToDto(category, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                await _unitOfWork.Categories.DeleteAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category deleted successfully with ID {CategoryId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private static CategoryDto MapToDto(Category category, IEnumerable<Domain.Entities.Professional> professionals)
        {
            var professionalCount = professionals.Count(p => p.CategoryId == category.Id);
            
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description ?? string.Empty,
                Icon = category.Icon ?? string.Empty,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ProfessionalCount = professionalCount
            };
        }
    }
}
