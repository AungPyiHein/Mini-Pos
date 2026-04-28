using Microsoft.AspNetCore.Authorization;
using POS.Shared.Models;

namespace POS.Backend.Common
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public UserRole[] AllowedRoles { get; }

        public RoleRequirement(params UserRole[] allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }
    }

    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly ICurrentUserService _currentUser;

        public RoleHandler(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            if (requirement.AllowedRoles.Contains(_currentUser.Role))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
