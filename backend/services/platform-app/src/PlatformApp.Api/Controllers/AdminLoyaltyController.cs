using Microsoft.AspNetCore.Mvc;
using PlatformApp.Application.Loyalty;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace PlatformApp.Api.Controllers;

[ApiController]
[Route("api/admin/loyalty")]
[Authorize(Policy = "AdminOnly")]
public class AdminLoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyaltyService;

    public AdminLoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules()
    {
        var rules = await _loyaltyService.GetRewardRulesAsync();
        return Ok(rules);
    }

    [HttpPost("rules")]
    public async Task<IActionResult> CreateRule([FromBody] RewardRuleDto dto)
    {
        var created = await _loyaltyService.CreateRewardRuleAsync(dto);
        return CreatedAtAction(nameof(GetRules), new { id = created.Id }, created);
    }

    [HttpPut("rules/{id}")]
    public async Task<IActionResult> UpdateRule([FromRoute] System.Guid id, [FromBody] RewardRuleDto dto)
    {
        dto.Id = id;
        await _loyaltyService.UpdateRewardRuleAsync(dto);
        return NoContent();
    }

    [HttpDelete("rules/{id}")]
    public async Task<IActionResult> DeleteRule([FromRoute] System.Guid id)
    {
        await _loyaltyService.DeleteRewardRuleAsync(id);
        return NoContent();
    }
}
