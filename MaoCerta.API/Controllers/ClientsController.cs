using Microsoft.AspNetCore.Mvc;
using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;

namespace MaoCerta.API.Controllers
{
    /// <summary>
    /// Controller for managing clients
    /// Implements RESTful API endpoints for client operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(
            IClientService clientService,
            ILogger<ClientsController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all clients
        /// </summary>
        /// <returns>List of clients</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            try
            {
                var clients = await _clientService.GetAllAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a client by ID
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <returns>Client details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetClient(int id)
        {
            try
            {
                var client = await _clientService.GetByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new client
        /// </summary>
        /// <param name="createClientDto">Client data</param>
        /// <returns>Created client</returns>
        [HttpPost]
        public async Task<ActionResult<ClientDto>> CreateClient(CreateClientDto createClientDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var client = await _clientService.CreateAsync(createClientDto);
                return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating client");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, new { message = "Erro interno do servidor ao criar cliente." });
            }
        }

        /// <summary>
        /// Updates an existing client
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="updateClientDto">Updated client data</param>
        /// <returns>Updated client</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, UpdateClientDto updateClientDto)
        {
            try
            {
                if (id != updateClientDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var client = await _clientService.UpdateAsync(updateClientDto);
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a client
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                var result = await _clientService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a client by email
        /// </summary>
        /// <param name="email">Client email</param>
        /// <returns>Client details</returns>
        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<ClientDto>> GetClientByEmail(string email)
        {
            try
            {
                var client = await _clientService.GetByEmailAsync(email);
                if (client == null)
                {
                    return NotFound();
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client with email {Email}", email);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
