using System.Security.Claims;

namespace GreenSuppliers.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
        => Guid.Parse(principal.FindFirst("sub")!.Value);

    public static Guid GetOrganizationId(this ClaimsPrincipal principal)
        => Guid.Parse(principal.FindFirst("organizationId")!.Value);
}
