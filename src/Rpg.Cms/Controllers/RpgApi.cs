using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Rpg.Experimental;
using Rpg.Experimental.Server;

namespace Rpg.Cms.Controllers
{
    /// <summary>
    /// Reads the body of a request with the rpg json settings (enums as text, dice as text). The rest of
    /// the site keeps its own json settings untouched.
    /// </summary>
    public class RpgBodyAttribute : ModelBinderAttribute
    {
        public RpgBodyAttribute()
            : base(typeof(RpgBodyModelBinder))
        {
            BindingSource = BindingSource.Body;
        }
    }

    public class RpgBodyModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            using var reader = new StreamReader(bindingContext.HttpContext.Request.Body);
            var json = await reader.ReadToEndAsync();

            try
            {
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject(json, bindingContext.ModelType, RpgServerJson.Settings());
                if (model == null)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "The request has no body");
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }

                bindingContext.Result = ModelBindingResult.Success(model);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }

    /// <summary>
    /// A request that refers to something that does not exist is the caller's mistake, not a server fault
    /// </summary>
    public class RpgExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is RpgServerException || context.Exception is RpgUnrolledDiceException || context.Exception is ArgumentException)
            {
                context.Result = new BadRequestObjectResult(new ProblemDetails
                {
                    Title = "The request could not be carried out",
                    Detail = context.Exception.Message,
                    Status = StatusCodes.Status400BadRequest
                });

                context.ExceptionHandled = true;
            }
        }
    }

    public abstract class RpgControllerBase : Controller
    {
        /// <summary>
        /// The response as json with the rpg json settings
        /// </summary>
        protected IActionResult RpgJson(object? data)
            => new ContentResult
            {
                Content = RpgServerJson.Serialize(data),
                ContentType = "application/json",
                StatusCode = StatusCodes.Status200OK
            };
    }
}
