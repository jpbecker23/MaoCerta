using Microsoft.AspNetCore.Mvc;
using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;

namespace MaoCerta.API.Controllers
{
    /// <summary>
    /// Controller for managing professionals
    /// Implements RESTful API endpoints for professional operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessionalsController : ControllerBase
    {
        private readonly IProfessionalService _professionalService;
        private readonly ILogger<ProfessionalsController> _logger;

        public ProfessionalsController(
            IProfessionalService professionalService,
            ILogger<ProfessionalsController> logger)
        {
            _professionalService = professionalService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all professionals
        /// </summary>
        /// <returns>List of professionals</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfessionalDto>>> GetProfessionals()
        {
            try
            {
                var professionals = await _professionalService.GetAllAsync();
                return Ok(professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving professionals");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a professional by ID
        /// </summary>
        /// <param name="id">Professional ID</param>
        /// <returns>Professional details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfessionalDto>> GetProfessional(int id)
        {
            try
            {
                var professional = await _professionalService.GetByIdAsync(id);
                if (professional == null)
                {
                    return NotFound();
                }
                return Ok(professional);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving professional with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new professional
        /// </summary>
        /// <param name="createProfessionalDto">Professional data</param>
        /// <returns>Created professional</returns>
        [HttpPost]
        public async Task<ActionResult<ProfessionalDto>> CreateProfessional(CreateProfessionalDto createProfessionalDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var professional = await _professionalService.CreateAsync(createProfessionalDto);
                return CreatedAtAction(nameof(GetProfessional), new { id = professional.Id }, professional);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating professional");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing professional
        /// </summary>
        /// <param name="id">Professional ID</param>
        /// <param name="updateProfessionalDto">Updated professional data</param>
        /// <returns>Updated professional</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfessional(int id, UpdateProfessionalDto updateProfessionalDto)
        {
            try
            {
                if (id != updateProfessionalDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var professional = await _professionalService.UpdateAsync(updateProfessionalDto);
                return Ok(professional);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating professional with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a professional
        /// </summary>
        /// <param name="id">Professional ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessional(int id)
        {
            try
            {
                var result = await _professionalService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting professional with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Searches professionals with filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Filtered professionals</returns>
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<ProfessionalDto>>> SearchProfessionals(ProfessionalSearchDto searchDto)
        {
            try
            {
                var professionals = await _professionalService.SearchAsync(searchDto);
                return Ok(professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching professionals");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets top rated professionals
        /// </summary>
        /// <param name="count">Number of professionals to return</param>
        /// <returns>Top rated professionals</returns>
        [HttpGet("top-rated")]
        public async Task<ActionResult<IEnumerable<ProfessionalDto>>> GetTopRated(int count = 10)
        {
            try
            {
                var professionals = await _professionalService.GetTopRatedAsync(count);
                return Ok(professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top rated professionals");
                return StatusCode(500, "Internal server error");
            }
        }

        // ✅ Novo método adicionado
        /// <summary>
        /// Gets detailed professional profile including reviews and statistics
        /// </summary>
        /// <param name="id">Professional ID</param>
        /// <returns>Detailed professional profile</returns>
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<ProfessionalDetailDto>> GetProfessionalDetail(int id)
        {
            try
            {
                var professional = await _professionalService.GetDetailAsync(id);
                if (professional == null)
                {
                    return NotFound(new { message = "Profissional não encontrado" });
                }
                return Ok(professional);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving detailed professional profile with ID {Id}", id);
                return StatusCode(500, new { message = "Erro ao buscar perfil do profissional" });
            }
        }
    }
}
