using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using POS.Shared.Models;

namespace POS.Backend.Common;

/// <summary>
/// Attribute used to declare which roles are allowed to access a controller or action.
/// Works together with <see cref="RbacFilter"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequireRoleAttribute : Attribute
{
    public UserRole[] AllowedRoles { get; }

    public RequireRoleAttribute(params UserRole[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

/// <summary>
/// ActionFilter that enforces RBAC after the JWT authentication and UserContextMiddleware have
/// already run.  It resolves <see cref="ICurrentUserService"/> (which has been hydrated from the
/// DB by the middleware) and checks whether the current user's role is in the allowed set declared
/// by a <see cref="RequireRoleAttribute"/> on the controller or action.
/// </summary>
public class RbacFilter : IActionFilter
{
    private readonly ICurrentUserService _currentUser;

    public RbacFilter(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Collect [RequireRole] from the action first, then fall back to the controller.
        var actionAttr = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireRoleAttribute>()
            .FirstOrDefault();

        if (actionAttr == null)
        {
            // No RBAC attribute on this endpoint — allow through.
            return;
        }

        if (!_currentUser.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!actionAttr.AllowedRoles.Contains(_currentUser.Role))
        {
            context.Result = new ForbidResult();
            return;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
