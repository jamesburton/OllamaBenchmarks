using System;

public record Version(int Major, int Minor, int Patch)
{
    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}

public static class VersionHelper
{
    public static bool IsCompatible(Version required, Version actual)
    {
        if (required == null) throw new ArgumentNullException(nameof(required));
        if (actual == null) throw new ArgumentNullException(nameof(actual));

        // Major version must match exactly
        if (actual.Major != required.Major)
        {
            return false;
        }

        // actual must be greater than or equal to required (component-wise)
        if (actual.Minor > required.Minor)
        {
            return true;
        }

        if (actual.Minor == required.Minor)
        {
            return actual.Patch >= required.Patch;
        }

        return false;
    }
}