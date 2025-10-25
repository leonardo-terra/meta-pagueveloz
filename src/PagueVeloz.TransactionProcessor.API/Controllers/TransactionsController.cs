using Microsoft.AspNetCore.Mvc;
using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Application.Services;

namespace PagueVeloz.TransactionProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountValidationService _accountValidationService;
    private readonly IAuditService _auditService;

    public TransactionsController(
        ITransactionService transactionService, 
        IAccountValidationService accountValidationService,
        IAuditService auditService)
    {
        _transactionService = transactionService;
        _accountValidationService = accountValidationService;
        _auditService = auditService;
    }

    [HttpPost]
    public async Task<ActionResult<ProcessTransactionResponse>> ProcessTransaction([FromBody] ProcessTransactionRequest request)
    {
        var userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        try
        {
            await _auditService.LogTransactionAttemptAsync(
                request.AccountId, 
                request.ReferenceId, 
                request.Operation, 
                request.Amount, 
                request.Currency, 
                userIp, 
                userAgent);

            var validationResult = await _accountValidationService.ValidateAccountForTransactionAsync(
                request.AccountId, 
                request.Amount, 
                request.Operation);

            if (!validationResult.IsValid)
            {
                await _auditService.LogTransactionFailureAsync(
                    request.AccountId, 
                    request.ReferenceId, 
                    request.Operation, 
                    request.Amount, 
                    request.Currency, 
                    validationResult.ErrorMessage, 
                    validationResult.ErrorCode);

                return BadRequest(new { 
                    error = validationResult.ErrorMessage, 
                    errorCode = validationResult.ErrorCode 
                });
            }

            var response = await _transactionService.ProcessTransactionAsync(request);

            if (response.Status == "success")
            {
                await _auditService.LogTransactionSuccessAsync(
                    response.TransactionId, 
                    request.AccountId, 
                    request.ReferenceId, 
                    request.Operation, 
                    request.Amount, 
                    request.Currency);
            }
            else
            {
                await _auditService.LogTransactionFailureAsync(
                    request.AccountId, 
                    request.ReferenceId, 
                    request.Operation, 
                    request.Amount, 
                    request.Currency, 
                    response.ErrorMessage ?? "Unknown error", 
                    "PROCESSING_ERROR");
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            await _auditService.LogTransactionFailureAsync(
                request.AccountId, 
                request.ReferenceId, 
                request.Operation, 
                request.Amount, 
                request.Currency, 
                ex.Message, 
                "SYSTEM_ERROR");

            return BadRequest(new { error = ex.Message });
        }
    }
}

