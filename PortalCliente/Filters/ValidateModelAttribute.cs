using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PortalCliente.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                // Get the first validation error
                var firstError = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?.ErrorMessage ?? "Validation failed";

                // Set TempData for error message
                controller.TempData["toastMessage"] = $"{firstError}||error";
            }
        }

        base.OnActionExecuting(context);
    }
}