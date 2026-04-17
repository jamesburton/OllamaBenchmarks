using System;

public interface IPermissionChecker
{
    bool HasPermission(string userId, string permission);
}

public class ResourceController
{
    private readonly IPermissionChecker _permissionChecker;

    public ResourceController(IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public string AccessResource(string userId, string resource)
    {
        if (_permissionChecker.HasPermission(userId, "read"))
        {
            return $"Granted: {resource}";
        }

        throw new UnauthorizedAccessException();
    }
}