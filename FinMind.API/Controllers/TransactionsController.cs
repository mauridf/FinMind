using Microsoft.AspNetCore.Mvc;
using FinMind.Application.DTOs;
using FinMind.Application.Services;

namespace FinMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(string id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<TransactionDto>>> GetUserTransactions(
        string userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var transactions = await _transactionService.GetUserTransactionsAsync(userId, startDate, endDate);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("user/{userId}")]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(string userId, CreateTransactionDto createTransactionDto)
    {
        try
        {
            var transaction = await _transactionService.CreateTransactionAsync(userId, createTransactionDto);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionDto>> UpdateTransaction(string id, UpdateTransactionDto updateTransactionDto)
    {
        try
        {
            var transaction = await _transactionService.UpdateTransactionAsync(id, updateTransactionDto);
            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTransaction(string id)
    {
        try
        {
            await _transactionService.DeleteTransactionAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}/balance")]
    public async Task<ActionResult<decimal>> GetUserBalance(string userId)
    {
        try
        {
            var balance = await _transactionService.GetUserBalanceAsync(userId);
            return Ok(new { balance });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}/spending-by-category")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetSpendingByCategory(
        string userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var spending = await _transactionService.GetSpendingByCategoryAsync(userId, startDate, endDate);
            return Ok(spending);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}