using Microsoft.AspNetCore.Mvc;
using MaoCerta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaoCerta.API.Controllers
{
    /// <summary>
    /// Controller for health checks and system status
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Checks if the API is running
        /// </summary>
        /// <returns>API status</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { 
                status = "OK", 
                message = "Mão Certa API is running",
                timestamp = DateTime.UtcNow 
            });
        }

        /// <summary>
        /// Checks database connectivity
        /// </summary>
        /// <returns>Database status</returns>
        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Get database info
                    var connectionString = _context.Database.GetConnectionString();
                    var databaseName = _context.Database.GetDbConnection().Database;
                    
                    return Ok(new
                    {
                        status = "OK",
                        message = "Database connection successful",
                        database = databaseName,
                        connectionString = connectionString?.Replace(connectionString?.Split(';').FirstOrDefault(s => s.Contains("Password")) ?? "", "Password=***"),
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(503, new
                    {
                        status = "ERROR",
                        message = "Database connection failed",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return StatusCode(503, new
                {
                    status = "ERROR",
                    message = "Database connection error",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Checks if all tables exist
        /// </summary>
        /// <returns>Tables status</returns>
        [HttpGet("tables")]
        public async Task<IActionResult> CheckTables()
        {
            try
            {
                var tables = new List<object>();
                
                // Check if tables exist
                var categoriesCount = await _context.Categories.CountAsync();
                var professionalsCount = await _context.Professionals.CountAsync();
                var clientsCount = await _context.Clients.CountAsync();
                var serviceRequestsCount = await _context.ServiceRequests.CountAsync();
                var reviewsCount = await _context.Reviews.CountAsync();

                tables.Add(new { table = "Categories", count = categoriesCount, status = "OK" });
                tables.Add(new { table = "Professionals", count = professionalsCount, status = "OK" });
                tables.Add(new { table = "Clients", count = clientsCount, status = "OK" });
                tables.Add(new { table = "ServiceRequests", count = serviceRequestsCount, status = "OK" });
                tables.Add(new { table = "Reviews", count = reviewsCount, status = "OK" });

                return Ok(new
                {
                    status = "OK",
                    message = "All tables are accessible",
                    tables = tables,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tables health check failed");
                return StatusCode(503, new
                {
                    status = "ERROR",
                    message = "Tables check failed",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
