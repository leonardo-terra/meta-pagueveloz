using Microsoft.AspNetCore.Mvc;
using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Application.Services;

namespace PagueVeloz.TransactionProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IAuditService _auditService;

    public AccountsController(IAccountService accountService, IAuditService auditService)
    {
        _accountService = accountService;
        _auditService = auditService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateAccountResponse>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            var response = await _accountService.CreateAccountAsync(request);
            
            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
            await _auditService.LogAccountCreationAsync(
                response.AccountId, 
                response.ClientId, 
                response.Balance, 
                response.CreditLimit, 
                userIp, 
                userAgent);

            return CreatedAtAction(nameof(GetAccount), new { accountId = response.AccountId }, response);
        }
        catch (Exception ex)
        {
            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
            await _auditService.LogValidationFailureAsync("ACCOUNT_CREATION", ex.Message, userIp, userAgent);
            
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

