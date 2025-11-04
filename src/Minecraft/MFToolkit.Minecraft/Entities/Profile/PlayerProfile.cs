using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MFToolkit.Minecraft.Entities.Cape;
using MFToolkit.Minecraft.Entities.Skin;

namespace MFToolkit.Minecraft.Entities.Profile;

/// <summary>
/// Minecraft玩家档案
/// </summary>
public class PlayerProfile
{
    /// <summary>
    /// 玩家UUID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// 玩家名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// 玩家皮肤信息
    /// </summary>
    [JsonPropertyName("skins")]
    public List<SkinInfo>? Skins { get; set; }
    
    /// <summary>
    /// 玩家披风信息
    /// </summary>
    [JsonPropertyName("capes")]
    public List<CapeInfo>? Capes { get; set; }
    
    /// <summary>
    /// 玩家是否拥有Minecraft
    /// </summary>
    [JsonPropertyName("owns_minecraft")]
    public bool? OwnsMinecraft { get; set; }
    
    /// <summary>
    /// 玩家是否可以更改名称
    /// </summary>
    [JsonPropertyName("can_change_name")]
    public bool? CanChangeName { get; set; }
    
    /// <summary>
    /// 玩家是否已更改过名称
    /// </summary>
    [JsonPropertyName("name_change_allowed")]
    public bool? NameChangeAllowed { get; set; }
    
    /// <summary>
    /// 玩家上次名称更改时间
    /// </summary>
    [JsonPropertyName("last_name_change")]
    public DateTimeOffset? LastNameChange { get; set; }
    
    /// <summary>
    /// 获取当前使用的皮肤
    /// </summary>
    public SkinInfo? CurrentSkin => Skins?.Find(skin => skin.State?.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) ?? false);
    
    /// <summary>
    /// 获取当前使用的披风
    /// </summary>
    public CapeInfo? CurrentCape => Capes?.Find(cape => cape.State?.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) ?? false);
}
