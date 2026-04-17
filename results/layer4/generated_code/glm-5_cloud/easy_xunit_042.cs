public interface IFeatureFlags
{
    bool IsEnabled(string featureName);
}

public class FeatureService
{
    private readonly IFeatureFlags _featureFlags;

    public FeatureService(IFeatureFlags featureFlags)
    {
        _featureFlags = featureFlags;
    }

    public string GetWelcomeMessage()
    {
        if (_featureFlags.IsEnabled("beta-ui"))
        {
            return "Welcome to Beta!";
        }

        return "Welcome!";
    }
}