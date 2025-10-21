using Microsoft.AspNetCore.Mvc;
using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Application.Services;

namespace PagueVeloz.TransactionProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateAccountResponse>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            var response = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetAccount), new { accountId = response.AccountId }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{accountId}")]
    public async Task<ActionResult<CreateAccountResponse>> GetAccount(Guid accountId)
    {
        try
        {
            var response = await _accountService.GetAccountAsync(accountId);
            if (response == null)
                return NotFound(new { error = "Account not found" });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

