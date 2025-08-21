using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.Dtos;
using SubscriptionService.Interface;

namespace SubscriptionService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionManagementService _service;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(ISubscriptionManagementService service, ILogger<SubscriptionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<SubscriptionReadDto>>> GetAllSubscriptions(
        [FromQuery] string? searchTerm,
        [FromQuery] int? type,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var subscriptions = await _service.GetAllAsync(searchTerm, type, pageNumber, pageSize);
        return Ok(subscriptions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriptionReadDto>> GetSubscriptionById(Guid id)
    {
        var subscription = await _service.GetByIdAsync(id);
        if (subscription == null)
        {
            return NotFound();
        }
        return Ok(subscription);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<ActionResult<SubscriptionReadDto>> CreateSubscription(SubscriptionCreateDto createDto)
    {
        var newSubscription = await _service.CreateAsync(createDto);
        return CreatedAtAction(nameof(GetSubscriptionById), new { id = newSubscription.Id }, newSubscription);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> UpdateSubscription(Guid id, SubscriptionUpdateDto updateDto)
    {
        var result = await _service.UpdateAsync(id, updateDto);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubscription(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin,SuperUser")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn một file Excel." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _service.ImportFromExcelAsync(stream);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi server nội bộ: {ex.Message}" });
        }
    }

    [HttpGet("download-template")]
    [Authorize]
    public async Task<IActionResult> DownloadTemplate()
    {
        try
        {
            var fileBytes = await _service.GenerateExcelTemplateAsync();
            var fileName = $"Subscription_Import_Template_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo file Excel mẫu.");
            return StatusCode(500, new { message = "Không thể tạo file mẫu." });
        }
    }

    [HttpGet("export")]
    [Authorize]
    public async Task<IActionResult> ExportToExcel(
       [FromQuery] string? searchTerm,
       [FromQuery] int? type)
    {
        try
        {
            var fileBytes = await _service.GenerateExcelExportAsync(searchTerm, type);
            var fileName = $"Subscriptions_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xuất dữ liệu ra Excel.");
            return StatusCode(500, new { message = "Không thể xuất file Excel." });
        }
    }


    [HttpGet("stats-by-type")]
    public async Task<ActionResult<IEnumerable<SubscriptionStatsDto>>> GetSubscriptionStats()
    {
        var stats = await _service.GetStatsByTypeAsync();
        return Ok(stats);
    }
}