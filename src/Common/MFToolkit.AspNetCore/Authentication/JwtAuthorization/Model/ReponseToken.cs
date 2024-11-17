﻿#nullable disable
namespace MFToolkit.AspNetCore.Authentication.JwtAuthorization.Model;
public sealed class ReponseToken
{
    /// <summary>
    /// 使用Token
    /// </summary>
    public string Token { get; set; }
    /// <summary>
    /// 刷新Token
    /// </summary>
    public string RefreshToken { get; set; }
    /// <summary>
    /// Token 有效期
    /// </summary>
    public double Timetamp { get; set; }
    /// <summary>
    /// 刷新 Token 有效期
    /// </summary>
    public double RefreshTimetamp { get; set; }
}