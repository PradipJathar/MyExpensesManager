using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExpenseManager.API.Dto;
using ExpenseManager.API.Services;

namespace ExpenseManager.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return userId;
        }

        [HttpGet("summary/{month}/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummaryResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMonthlySummary(int month, int year)
        {
            try
            {
                var userId = GetCurrentUserId();
                var summary = await _reportService.GetMonthlySummaryAsync(userId, month, year);
                return Ok(summary);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching monthly summary." });
            }
        }

        [HttpGet("yearly/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TrendResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetYearlySummary(int year)
        {
            try
            {
                var userId = GetCurrentUserId();
                var yearly = await _reportService.GetYearlySummaryAsync(userId, year);
                return Ok(yearly);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching yearly summary." });
            }
        }

        [HttpGet("category-breakdown/{month}/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryBreakdownResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategoryBreakdown(int month, int year)
        {
            try
            {
                var userId = GetCurrentUserId();
                var breakdown = await _reportService.GetCategoryBreakdownAsync(userId, month, year);
                return Ok(breakdown);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching category breakdown." });
            }
        }

        [HttpGet("monthly-comparison/{month}/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MonthlyComparisonResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMonthlyComparison(int month, int year)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comparison = await _reportService.GetMonthlyComparisonAsync(userId, month, year);
                return Ok(comparison);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching monthly comparison." });
            }
        }

        [HttpGet("trends")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TrendResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTrends([FromQuery] int months = 6)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trends = await _reportService.GetSpendingTrendsAsync(userId, months);
                return Ok(trends);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching spending trends." });
            }
        }

        [HttpGet("budget-vs-actual/{month}/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BudgetVsActualResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBudgetVsActual(int month, int year)
        {
            try
            {
                var userId = GetCurrentUserId();
                var budgetVsActual = await _reportService.GetBudgetVsActualAsync(userId, month, year);
                return Ok(budgetVsActual);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching budget vs actual comparison." });
            }
        }
    }
}
