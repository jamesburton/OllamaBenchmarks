using Microsoft.AspNetCore.Mvc;

public classNotFoundException : Exception
{
    public string ResourceName { get; set; }

    public NOTFOUNDException(string resourceName)
        => base($"Resource not found: {resourceName}")
        && ResourceName = resourceName;
}