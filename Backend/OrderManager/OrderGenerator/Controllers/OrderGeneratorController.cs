using Domain.Models;
using Domain.Utils;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.Text.Json;

namespace Api.OrderGenerator.Controllers;

[Route("api/[action]")]
[ApiController]
public class OrderGeneratorController : ControllerBase
{
    private readonly IOrderGeneratorService _orderGeneratorService;

    public OrderGeneratorController(IOrderGeneratorService orderGeneratorService)
    {
        _orderGeneratorService = orderGeneratorService;
    }

    /// <summary>
    /// Lista os símbolos válidos para geração de ordens
    /// </summary>
    [HttpGet]

    public async Task<IActionResult> GetValidSymbols()
    {
        var result = Constants.VALID_SYMBOLS;
        result.Sort();
        return Ok(result);
    }

    /// <summary>
    /// Gera uma ordem de compra ou venda e envia para o OrderAccumulator, através do QuickFIX/n Engine
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder(Order order)
    {
        Console.WriteLine($"Ordem Recebida: {JsonSerializer.Serialize(order)}");
        var orderValidation = await _orderGeneratorService.ValidateOrder(order);

        if (!orderValidation.IsValid)
        {
            var validationMessages = orderValidation.Errors.Select(e => e.ErrorMessage);
            return BadRequest(validationMessages);
        }

        await _orderGeneratorService.ProcessOrder(order);
        return Ok(order);
    }
}
