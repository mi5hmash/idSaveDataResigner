using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using idSaveDataResignerCore.GamingPlatforms;
using Mi5hmasH.GameProfile;

namespace idSaveDataResignerWpf.GameProfile;

public class IdTechGameProfile : IEquatable<IdTechGameProfile>, INotifyPropertyChanged, IGameProfile
{
    /// <summary>
    /// Gets or sets metadata information related to the game profile.
    /// </summary>
    public GameProfileMeta Meta { get; set; } = new("IdTech7", new Version(1, 0, 0, 0));

    /// <summary>
    /// Gets or sets the title of a game.
    /// </summary>
    public string? GameTitle
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(GameTitle));
        }
    }

    /// <summary>
    /// Gets or sets the GamingPlatform.
    /// </summary>
    public GamingPlatform Platform
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(Platform));
        }
    } = GamingPlatform.Other;

    /// <summary>
    /// Gets or sets the application identifier associated with this instance.
    /// </summary>
    public string? AppId
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(AppId));
        }
    }

    /// <summary>
    /// Gets or sets the Game Profile Icon encoded with Base64.
    /// </summary>
    public string? Base64GpIcon
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(Base64GpIcon));
        }
    }

    /// <summary>
    /// Gets or sets the code that is used during deencryption.
    /// </summary>
    public string? GameCode
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(GameCode));
        }
    }

    /// <summary>
    /// Copies the game profile data from the specified object if it is an instance of IdTechGameProfile.
    /// </summary>
    /// <param name="other">The object from which to copy game profile data.</param>
    public void Set(object other)
    {
        if (other is not IdTechGameProfile profile) return;
        GameTitle = profile.GameTitle;
        AppId = profile.AppId;
        Base64GpIcon = profile.Base64GpIcon;
        Platform = profile.Platform;
        GameCode = profile.GameCode;
    }

    public bool Equals(IdTechGameProfile? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;

        var sc = StringComparer.Ordinal;
        return Meta.Equals(other.Meta) &&
               sc.Equals(GameTitle, other.GameTitle) &&
               sc.Equals(AppId, other.AppId) &&
               sc.Equals(Base64GpIcon, other.Base64GpIcon) &&
               Platform == other.Platform &&
               sc.Equals(GameCode, other.GameCode);
    }

    public int GetHashCodeStable()
    {
        var hc = new HashCode();
        var sc = StringComparer.Ordinal;
        // Add fields to the hash code computation
        hc.Add(Meta);
        hc.Add(GameTitle, sc);
        hc.Add(AppId, sc);
        hc.Add(Base64GpIcon, sc);
        hc.Add(Platform);
        hc.Add(GameCode, sc);
        return hc.ToHashCode();
    }

    // This is a workaround to avoid the default GetHashCode() implementation in objects where all fields are mutable.
    private readonly Guid _uniqueId = Guid.NewGuid();
    
    public override int GetHashCode()
        => _uniqueId.GetHashCode();

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is IdTechGameProfile castedObj && Equals(castedObj);

    public static bool operator ==(IdTechGameProfile? left, IdTechGameProfile? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(IdTechGameProfile? left, IdTechGameProfile? right) 
        => !(left == right);

    // MVVM support
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}