using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Mi5hmasH.GameProfile;

namespace idSaveDataResignerWpf.GameProfile;

public class IdTechGameProfile : INotifyPropertyChanged, IGameProfile
{
    /// <summary>
    /// Gets or sets metadata information related to the game profile.
    /// </summary>
    public GameProfileMeta Meta { get; set; } = new("IdTech7", new Version(1, 0, 0, 0));

    /// <summary>
    /// The title of a game.
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
    /// Steam AppID.
    /// </summary>
    public uint? SteamAppId
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(nameof(SteamAppId));
        }
    }

    /// <summary>
    /// Game Profile Icon encoded with Base64.
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
        SteamAppId = profile.SteamAppId;
        Base64GpIcon = profile.Base64GpIcon;
        GameCode = profile.GameCode;
    }

    public bool Equals(IdTechGameProfile other)
    {
        return Meta.Equals(other.Meta) &&
               GameTitle == other.GameTitle &&
               SteamAppId == other.SteamAppId &&
               Base64GpIcon == other.Base64GpIcon &&
               GameCode == other.GameCode;
    }

    public int GetHashCodeStable()
        => HashCode.Combine(Meta, GameTitle, SteamAppId, Base64GpIcon, GameCode);

    // This is a workaround to avoid the default GetHashCode() implementation in objects where all fields are mutable.
    private readonly Guid _uniqueId = Guid.NewGuid();
    
    public override int GetHashCode()
        => _uniqueId.GetHashCode();

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is IdTechGameProfile castedObj && Equals(castedObj);

    public static bool operator ==(IdTechGameProfile left, IdTechGameProfile right)
        => left.Equals(right);

    public static bool operator !=(IdTechGameProfile left, IdTechGameProfile right)
        => !(left == right);

    // MVVM support
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}