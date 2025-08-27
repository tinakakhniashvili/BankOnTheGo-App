using BankOnTheGo.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidateModelFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var response = new Response
            {
                IsSuccess = false,
                Message = "Validation failed",
                Status = "Error"
            };

            context.Result = new BadRequestObjectResult(response);
        }
    }
}