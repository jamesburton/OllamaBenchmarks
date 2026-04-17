using System;
using System.Collections.Generic;
using System.Linq;

public record Permission(string Name, string Resource, string Action);

public class RolePermissionStore
{
    private readonly Dictionary<string, List<Permission>> _rolePermissions = new();

    public void AssignPermission(string role, Permission permission)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (permission == null) throw new ArgumentNullException(nameof(permission));

        if (!_rolePermissions.TryGetValue(role, out var permissions))
        {
            permissions = new List<Permission>();
            _rolePermissions[role] = permissions;
        }

        permissions.Add(permission);
    }

    public List<Permission> GetPermissions(string role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        return _rolePermissions.TryGetValue(role, out var permissions) 
            ? new List<Permission>(permissions) 
            : new List<Permission>();
    }

    public bool CanDo(string role, string resource, string action)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (resource == null) throw new ArgumentNullException(nameof(resource));
        if (action == null) throw new ArgumentNullException(nameof(action));

        if (!_rolePermissions.TryGetValue(role, out var permissions))
        {
            return false;
        }

        return permissions.Any(p => p.Resource == resource && p.Action == action);
    }
}