using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.Payment.AlfaPay;
using Tgs.Payment.Clients;

namespace AlfaPayTest;

internal class Demo : BackgroundService
{
    private readonly IPaymentClient _paymentClient;
    private readonly ILogger<Demo> _logger;

    public Demo(
        IPaymentClient paymentClient,
        ILogger<Demo> logger)
    {
        _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //await DepoistAsync();
        await WithdrawAsync();
    }

    private async Task DepoistAsync()
    {
        var apiToken = "sUxtpDWiNoqUxKfqJoHCL69TS4omJRB1fBoaw4PfJT4Boncf8cLj6lYtN5DS";
        var notitfyToken = "NotifyToken";
        var orderNo = Guid.NewGuid();
        var amount = 500.00m;        


        var depositSettings = new AlfaPayDepositSettings(
            apiToken,
            notitfyToken,
            orderNo,            
            PaidName: null);
        try
        {
            var depositReturn = await _paymentClient.DepositAsync(
                "tgs.payment.alfapay.deposit",
                amount,
                JsonSerializer.Serialize(depositSettings));
            _logger.LogInformation($"Deposit Return: {depositReturn}");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex.Message);
        }
    }

    private async Task WithdrawAsync()
    {        
        var apiToken = "3p3FCtslWkFu9iFkAnFjOKRLWZoqAy58iaDldaCZ1M4jNmyVJA0fhkSe5IIv";
        var notifyToken = "sPknBbCd38DZWHhEZ2DyHFdyvgkDwULFl7tefGILMYwOUIaFjhqIsUps3T98";
        var accountNumber = "9517585997";
        var bankId = "GCASH";
        var bankOwner = "王小明";
        var orderNo = Guid.NewGuid();        
        var amount = 100M;

        var withdrawSettings = new AlfaPayWithdrawSettings(
            apiToken,
            notifyToken,
            accountNumber,
            bankId,
            bankOwner,
            orderNo);
        try
        {
            var withdrawReturn = await _paymentClient.WithdrawAsync(
                "tgs.payment.alfapay.withdraw",
                amount,
                JsonSerializer.Serialize(withdrawSettings));
            _logger.LogInformation($"Withdraw Return: {withdrawReturn}");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex.Message);
        }
    }
}
