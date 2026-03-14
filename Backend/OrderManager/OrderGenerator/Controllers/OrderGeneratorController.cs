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
        Console.WriteLine($"\nOrdem Recebida: {JsonSerializer.Serialize(order)}");

        var result = await _orderGeneratorService.ProcessOrder(order);

        if (!result.IsValid)
        { 
            Console.WriteLine($"Ordem Inválida: {string.Join(", ", result.Errors)}");
            return BadRequest(result);
        }

        return Ok(order);
    }
}
