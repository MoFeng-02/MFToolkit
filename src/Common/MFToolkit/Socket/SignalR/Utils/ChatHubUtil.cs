using MFToolkit.CommonTypes.Enumerates;
using MFToolkit.Socket.SignalR.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace MFToolkit.Socket.SignalR.Utils;
public class ChatHubUtil
{
    /// <summary>
    /// 连接
    /// </summary>
    public static HubConnection? Connection { get; internal set; }
    /// <summary>
    /// 连接token，如果取消则代表不继续连接，除非连接
    /// </summary>
    private static CancellationTokenSource _connectionTokenSource = null!;
    /// <summary>
    /// 是否已经启动了连接
    /// </summary>
    public static bool IsStart;
    /// <summary>
    /// 注册异常委托操作
    /// <para>Exception：异常</para>
    /// <para>string：出现异常的操作</para>
    /// </summary>
    public static Action<Exception, string?> ExceptionAction = null!;
    /// <summary>
    /// 是否启动重连
    /// </summary>
    public static bool IsStartReconnection { get; private set; }
    /// <summary>
    /// 获取连接状态
    /// </summary>
    /// <returns></returns>
    public static HubConnectionState? GetConnectState()
    {
        if (Connection == null) return null;
        return Connection.State;
    }
    /// <summary>
    /// 断开连接，即表示手动离线
    /// </summary>
    /// <param name="action">异常委托</param>
    public static async Task DisconnectAsync()
    {
        if (Connection == null) return;
        try
        {
            // 如果已经处于断开
            if (_connectionTokenSource.IsCancellationRequested && GetConnectState() == HubConnectionState.Disconnected) return;
            // 取消以防止取消后自动重连
            _connectionTokenSource.Cancel();
            await Connection.StopAsync();
            IsStart = false;
        }
        catch (Exception ex)
        {
            ExceptionAction?.Invoke(ex, nameof(DisconnectAsync));
        }
        finally
        {
            //await Connection.DisposeAsync();
            //Connection = null;
        }
    }
    /// <summary>
    /// 获取/设置是否已经启动自动重连
    /// </summary>
    /// <returns></returns>
    public static bool ToIsStartReconnection()
    {
        if (IsStartReconnection) return true;
        IsStartReconnection = true;
        // 此前需要通关一次，但是在本方法中会设置已经启动自动连接
        return false;
    }
    /// <summary>
    /// 连接到聊天集成中心
    /// </summary>
    /// <param name="url">连接URI</param>
    /// <param name="token">Token</param>
    /// <returns></returns>
    public static HubConnection ConnectionBuild(string url, Func<string?>? token = null)
    {
        if (Connection != null) return Connection;
        _connectionTokenSource?.Dispose();
        Connection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                if (token == null) return;
                options.AccessTokenProvider = () => Task.FromResult(token?.Invoke());
#if DEBUG
                // DEBUG 模式下忽略SSL
                options.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                        // always verify the SSL certificate
                        clientHandler.ServerCertificateCustomValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    return message;
                };
#endif
            })
            .Build();
        _connectionTokenSource = new();
        return Connection;
    }
    /// <summary>
    /// 连接到聊天集成中心
    /// </summary>
    /// <param name="url">连接URI</param>
    /// <param name="isAutoConnection">是否自动重连</param>
    /// <param name="token">Token</param>
    /// <param name="connectionAction">重连时的状态
    /// <para>int: 连接次数</para>
    /// <para>HubConnectionState?: 连接状态</para>
    /// </param>
    /// <returns></returns>
    public static async Task ConnectionBuildAsync(string url, bool isAutoConnection = false, string? token = null, Action<int, HubConnectionState?>? connectionAction = null)
    {
        if (Connection != null) return;
        _connectionTokenSource?.Dispose();
        Connection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
#if DEBUG
                // DEBUG 模式下忽略SSL
                options.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                        // always verify the SSL certificate
                        clientHandler.ServerCertificateCustomValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    return message;
                };
#endif
            })
            .Build();
        _connectionTokenSource = new();
        // 如果已经启动自动重连
        if (ToIsStartReconnection()) return;
        if (!isAutoConnection) return;
        await Reconnect(connectionAction);
    }
    /// <summary>
    /// 启动连接
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task StartConnection(Action<ChatMessageModel, ChatContactType>? action = null)
    {
        if (Connection == null) return;
        ReceiveMessage ??= action;
        Connection.Remove(nameof(ReceiveMessage));

        Connection.On(nameof(ReceiveMessage), ReceiveMessage);
        try
        {
            await Connection.StartAsync();
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
        }
        IsStart = GetConnectState() == HubConnectionState.Connected;
    }
    /// <summary>
    /// 最大重试次数
    /// </summary>
    const int maxRetryAttempts = 5;
    /// <summary>
    /// 重新连接
    /// </summary>
    /// <param name="connectionAction">重连时的状态
    /// <para>int: 连接次数</para>
    /// <para>HubConnectionState?: 连接状态</para>
    /// </param>
    /// <returns></returns>
    public static async Task Reconnect(Action<int, HubConnectionState?>? connectionAction = null)
    {
        IsStart = false;
        // 如果已经启动
        int retryCount = 0;
        if (Connection == null) return;
        while (Connection?.State == HubConnectionState.Disconnected && !_connectionTokenSource.IsCancellationRequested && !_connectionTokenSource.Token.IsCancellationRequested)
        {

            Console.WriteLine($"Attempting to reconnect... (Attempt {retryCount + 1})");


            // 尝试重新启动连接
            try
            {
                await StartConnection();

            }
            catch (Exception ex)
            {

            }
            retryCount++;
            connectionAction?.Invoke(retryCount, GetConnectState());
            // 延时一段时间再进行重连，可以根据需求调整延时时间
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (GetConnectState() == HubConnectionState.Connected) await Console.Out.WriteLineAsync("重新连接成功");
        }
    }
    /// <summary>
    /// 接收信息的委托
    /// </summary>
    public static Action<ChatMessageModel, ChatContactType>? ReceiveMessage;

    #region 发送单个接收者的信息
    /// <summary>
    /// 发送信息方法
    /// </summary>
    /// <param name="message">信息</param>
    /// <param name="sendMethodName">发送方法名称</param>
    /// <param name="exceptionmAtion">异常委托</param>
    /// <exception cref="Exception">InvokeAsync 方法会返回一个在服务器方法返回时完成的 Task。 返回值（如果有）作为 Task 的结果提供。 服务器上的方法所引发的任何异常都会产生出错的 Task。 使用 await 语法等待服务器方法完成，并使用 try...catch 语法处理错误。</exception>
    /// <returns>发送成功为true，否则false</returns>
    public static async Task<bool> SendMessageAsync(ChatMessageModel message, string sendMethodName = nameof(SendMessageAsync), Action<Exception>? exceptionmAtion = null)
    {
        if (Connection == null || GetConnectState() == (HubConnectionState.Disconnected | HubConnectionState.Connecting) || !IsStart) return false;
        try
        {
            // 发送单个
            await Connection.InvokeAsync(sendMethodName, message);
            return true;
        }
        catch (Exception ex)
        {
            exceptionmAtion?.Invoke(ex);
            return false;
        }
    }
    #endregion
    #region 发送组信息
    /// <summary>
    /// 发送组信息方法
    /// </summary>
    /// <param name="message">信息</param>
    /// <param name="sendMethodName">发送方法名称</param>
    /// <param name="exceptionmAtion">异常委托</param>
    /// <exception cref="Exception">InvokeAsync 方法会返回一个在服务器方法返回时完成的 Task。 返回值（如果有）作为 Task 的结果提供。 服务器上的方法所引发的任何异常都会产生出错的 Task。 使用 await 语法等待服务器方法完成，并使用 try...catch 语法处理错误。</exception>
    /// <returns>发送成功为true，否则false</returns>
    public static async Task<bool> SendGroupMessageAsync(ChatMessageModel message, string sendMethodName = nameof(SendGroupMessageAsync), Action<Exception>? exceptionmAtion = null)
    {
        if (Connection == null || GetConnectState() == (HubConnectionState.Disconnected | HubConnectionState.Connecting)) return false;
        try
        {
            // 发送群聊
            await Connection.InvokeAsync(sendMethodName, message);
            return true;
        }
        catch (Exception ex)
        {
            exceptionmAtion?.Invoke(ex);
            return false;
        }
    }
    #endregion
}