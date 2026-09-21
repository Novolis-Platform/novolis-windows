namespace Novolis.Windows.Sessions;

/// <summary>Describes a Windows session that can host an interactive user.</summary>
public sealed record WindowsSessionInfo(
    uint SessionId,
    string UserName,
    string DomainName,
    bool IsActive);
