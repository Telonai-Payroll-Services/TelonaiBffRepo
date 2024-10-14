namespace TelonaiWebApi.Helpers;

using System.Security.Claims;
public interface IScopedAuthorization
{
    public  void ValidateByJobId(ClaimsPrincipal principal, AuthorizationType authType, int jobId);
    public void ValidateByCompanyId(ClaimsPrincipal principal, AuthorizationType authType, int companyId);
    public void Validate(ClaimsPrincipal principal, AuthorizationType authType);
}
public class ScopedAuthorization: IScopedAuthorization
{
    public  void ValidateByJobId(ClaimsPrincipal principal, AuthorizationType authType, int jobId)
    {

        var roles = principal.Claims.Where(e => e.Type == ClaimTypes.Role);
        var scope = principal.Claims.First(e => e.Type == "custom:scope").Value;
        var desiredScope = "SystemAdmin";

        if (roles.Any(e=>e.Value==desiredScope))
            return;

        switch (authType)
        {
            case AuthorizationType.Admin:
                desiredScope = $"RoleAdminJ{jobId}";
                if (scope.Contains(desiredScope))
                    return;

                break;
            case AuthorizationType.User:
                desiredScope = $"RoleUserJ{jobId}";
                if (scope.Contains(desiredScope) || scope.Contains($"RoleAdminJ{jobId}"))
                    return;

                break;
            default:
                throw new UnauthorizedAccessException();
        }

        throw new UnauthorizedAccessException();
    }
    public  void ValidateByCompanyId(ClaimsPrincipal principal, AuthorizationType authType, int companyId)
    {        
        var scope = principal.Claims.First(e => e.Type == "custom:scope").Value;
        var desiredScope = "SystemAdmin";

        if (scope.Contains(desiredScope))
            return;

        switch (authType)
        {
            case AuthorizationType.Admin:
                desiredScope = $"{companyId}RoleAdmin";
                if (scope.Contains(desiredScope))
                    return;

                break;
            case AuthorizationType.User:
                desiredScope = $"{companyId}RoleUser";
                if (scope.Contains(desiredScope) || scope.Contains($"{companyId}RoleAdmin"))
                    return;

                break;
            default:
                throw new UnauthorizedAccessException();
        }

        throw new UnauthorizedAccessException();
    }
    public  void Validate(ClaimsPrincipal principal, AuthorizationType authType)
    {

        var email = principal.Claims.First(e => e.Type == "email").Value;
        var scope = principal.Claims.First(e => e.Type == "custom:scope").Value;
        var desiredScope = "SystemAdmin";

        if (scope.Contains(desiredScope))
            return;

        switch (authType)
        {
            case AuthorizationType.Admin:
            
            case AuthorizationType.User:
     
            default:
                throw new UnauthorizedAccessException();
        }

        throw new UnauthorizedAccessException();
    }
}
    