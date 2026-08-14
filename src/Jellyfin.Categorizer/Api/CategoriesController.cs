using Jellyfin.Categorizer.Data;
using Jellyfin.Categorizer.Models;
using Jellyfin.Categorizer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Categorizer.Api;

[ApiController]
[Authorize]
[Route("Categorizer/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ILogger<CategoriesController> _logger;
    private readonly CategoryService _categoryService;
    private readonly Plugin _plugin;

    public CategoriesController(
        ILogger<CategoriesController> logger,
        CategoryService categoryService,
        Plugin plugin)
    {
        _logger = logger;
        _categoryService = categoryService;
        _plugin = plugin;
    }

    [HttpGet]
    public ActionResult<List<Category>> GetCategories()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var categories = _categoryService.GetCategoriesForUser(userGuid, _plugin.Configuration);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public ActionResult<List<Category>> GetCategoriesById(string id)
    {
        if (!Guid.TryParse(id, out var userId))
        {
            return BadRequest("Invalid user ID format.");
        }

        var categories = _categoryService.GetCategoriesForUser(userId, _plugin.Configuration);
        return Ok(categories);
    }
}
