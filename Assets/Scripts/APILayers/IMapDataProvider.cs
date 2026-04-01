using System.Collections;

/// <summary>
/// 
/// </summary>
public interface IMapDataProvider
{
    string ProviderID { get; }
    void EnableProvider();
    void DisableProvider();
    bool IsActive { get; }
}