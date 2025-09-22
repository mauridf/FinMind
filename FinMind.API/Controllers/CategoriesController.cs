using Microsoft.AspNetCore.Mvc;
using FinMind.Application.DTOs;
using FinMind.Application.Services;
using FinMind.Domain.Enums;

namespace FinMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(string id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<CategoryDto>>> GetUserCategories(string userId)
    {
        try
        {
            var categories = await _categoryService.GetUserCategoriesAsync(userId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}/type/{type}")]
    public async Task<ActionResult<List<CategoryDto>>> GetUserCategoriesByType(string userId, TransactionType type)
    {
        try
        {
            var categories = await _categoryService.GetUserCategoriesByTypeAsync(userId, type);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("user/{userId}")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(string userId, CreateCategoryDto createCategoryDto)
    {
        try
        {
            var category = await _categoryService.CreateCategoryAsync(userId, createCategoryDto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(string id, UpdateCategoryDto updateCategoryDto)
    {
        try
        {
            var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(string id)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("default")]
    public async Task<ActionResult<List<CategoryDto>>> GetDefaultCategories()
    {
        try
        {
            var categories = await _categoryService.GetDefaultCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}