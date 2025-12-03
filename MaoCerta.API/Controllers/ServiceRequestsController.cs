using Microsoft.AspNetCore.Mvc;
using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;

namespace MaoCerta.API.Controllers
{
    /// <summary>
    /// Controller for managing service requests
    /// Implements RESTful API endpoints for service request operations
    /// </summary>
    [ApiController]
    [Route("api/service-requests")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            IServiceRequestService serviceRequestService,
            ILogger<ServiceRequestsController> logger)
        {
            _serviceRequestService = serviceRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all service requests
        /// </summary>
        /// <returns>List of service requests</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetServiceRequests()
        {
            try
            {
                var serviceRequests = await _serviceRequestService.GetAllAsync();
                return Ok(serviceRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service requests");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a service request by ID
        /// </summary>
        /// <param name="id">Service request ID</param>
        /// <returns>Service request details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestDto>> GetServiceRequest(int id)
        {
            try
            {
                var serviceRequest = await _serviceRequestService.GetByIdAsync(id);
                if (serviceRequest == null)
                {
                    return NotFound();
                }
                return Ok(serviceRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service request with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Creates a new service request
        /// </summary>
        /// <param name="createServiceRequestDto">Service request data</param>
        /// <returns>Created service request</returns>
        [HttpPost]
        public async Task<ActionResult<ServiceRequestDto>> CreateServiceRequest(CreateServiceRequestDto createServiceRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var serviceRequest = await _serviceRequestService.CreateAsync(createServiceRequestDto);
                return CreatedAtAction(nameof(GetServiceRequest), new { id = serviceRequest.Id }, serviceRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument creating service request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates an existing service request
        /// </summary>
        /// <param name="id">Service request ID</param>
        /// <param name="updateServiceRequestDto">Updated service request data</param>
        /// <returns>Updated service request</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceRequest(int id, UpdateServiceRequestDto updateServiceRequestDto)
        {
            try
            {
                if (id != updateServiceRequestDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var serviceRequest = await _serviceRequestService.UpdateAsync(updateServiceRequestDto);
                return Ok(serviceRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument updating service request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service request with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a service request
        /// </summary>
        /// <param name="id">Service request ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            try
            {
                var result = await _serviceRequestService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service request with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets service requests by client ID
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <returns>List of service requests for the client</returns>
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetServiceRequestsByClient(int clientId)
        {
            try
            {
                var serviceRequests = await _serviceRequestService.GetByClientIdAsync(clientId);
                return Ok(serviceRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service requests for client with ID {ClientId}", clientId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets service requests by professional ID
        /// </summary>
        /// <param name="professionalId">Professional ID</param>
        /// <returns>List of service requests for the professional</returns>
        [HttpGet("professional/{professionalId}")]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetServiceRequestsByProfessional(int professionalId)
        {
            try
            {
                var serviceRequests = await _serviceRequestService.GetByProfessionalIdAsync(professionalId);
                return Ok(serviceRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service requests for professional with ID {ProfessionalId}", professionalId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates the status of a service request
        /// </summary>
        /// <param name="id">Service request ID</param>
        /// <param name="updateStatusDto">Status update data</param>
        /// <returns>Updated service request</returns>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateServiceRequestStatus(int id, UpdateServiceRequestStatusDto updateStatusDto)
        {
            try
            {
                if (id != updateStatusDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var serviceRequest = await _serviceRequestService.UpdateStatusAsync(updateStatusDto);
                return Ok(serviceRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument updating service request status");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service request status with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Generates a verification code for a service request
        /// </summary>
        /// <param name="id">Service request ID</param>
        /// <returns>Generated verification code</returns>
        [HttpPost("{id}/generate-verification-code")]
        public async Task<ActionResult<string>> GenerateVerificationCode(int id)
        {
            try
            {
                var code = await _serviceRequestService.GenerateVerificationCodeAsync(id);
                return Ok(new { verificationCode = code });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument generating verification code");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating verification code for service request with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Verifies a code for a service request
        /// </summary>
        /// <param name="id">Service request ID</param>
        /// <param name="request">Verification code request</param>
        /// <returns>Verification result</returns>
        [HttpPost("{id}/verify-code")]
        public async Task<ActionResult<bool>> VerifyCode(int id, [FromBody] VerifyCodeRequestDto request)
        {
            try
            {
                var isValid = await _serviceRequestService.VerifyCodeAsync(id, request.Code);
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying code for service request with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        public class VerifyCodeRequestDto
        {
            public string Code { get; set; } = string.Empty;
        }
    }
}

