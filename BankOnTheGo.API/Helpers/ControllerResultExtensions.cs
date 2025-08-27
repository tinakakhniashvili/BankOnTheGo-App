using BankOnTheGo.Domain.Models;
using BankOnTheGo.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankOnTheGo.API.Helpers
{
    public static class ControllerResultExtensions
    {
        public static IActionResult HandleResult<T>(this ControllerBase controller, ServiceResult<T> result)
        {
            if (result.Success)
                return controller.Ok(result.Data);

            return controller.BadRequest(new Response
            {
                Status = "Error",
                Message = result.Error
            });
        }
    }
}