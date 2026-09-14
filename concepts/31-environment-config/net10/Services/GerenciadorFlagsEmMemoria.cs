using System.Collections.Concurrent;
using Microsoft.FeatureManagement;

namespace EnvironmentDemo.Services;

public class GerenciadorFlagsEmMemoria
{
    private readonly IFeatureManager _featureManager;
    private readonly ConcurrentDictionary<string, bool> _overrides = new();

    public GerenciadorFlagsEmMemoria(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public async Task<bool> IsEnabledAsync(string feature)
    {
        if (_overrides.TryGetValue(feature, out var val))
        {
            return val;
        }
        return await _featureManager.IsEnabledAsync(feature);
    }

    public void DefinirOverride(string feature, bool enabled)
    {
        _overrides[feature] = enabled;
    }

    public void LimparOverrides()
    {
        _overrides.Clear();
    }
}
