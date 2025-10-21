using Microsoft.AspNetCore.Mvc;
using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Application.Services;

namespace PagueVeloz.TransactionProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<ActionResult<ProcessTransactionResponse>> ProcessTransaction([FromBody] ProcessTransactionRequest request)
    {
        try
        {
            var response = await _transactionService.ProcessTransactionAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

