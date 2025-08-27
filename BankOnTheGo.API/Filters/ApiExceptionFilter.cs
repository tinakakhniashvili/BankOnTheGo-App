using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BankOnTheGo.Domain.Models;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var response = new Response
        {
            IsSuccess = false,
            Message = context.Exception.Message,
            Status = "Error"
        };

        context.Result = new JsonResult(response)
        {
            StatusCode = 500
        };
    }
}