### 关于

1. 本包基于MIT开源协议
2. 本包开源地址为：https://github.com/MoFeng-02/ZeroUI

### 如何使用

1. 在App.axaml文件中如此引用
```xaml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="Zero.App.App"
             RequestedThemeVariant="Default">
    <!-- "Default" ThemeVariant follows system theme variant. "Dark" or "Light" are other available options. -->
    <Application.Styles>
        <!-- 无需自己引入FluentTheme，在本UI中已经引入了FluentTheme -->
        <!-- <FluentTheme /> -->
        <!-- 引入UI库 -->
        <StyleInclude Source="avares://ZeroUI/Including/Index.axaml" />
    </Application.Styles>
</Application>
```
2. 如何使用
```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:control="https://github.com/MoFeng-02/ZeroUI"
             mc:Ignorable="d" d:DesignWidth="600" d:DesignHeight="450"
             x:Class="Zero.App.Views.TestView"
             Name="Test">
    <Grid>
        <control:ZeroProgressBar Value="50"
                                 Height="15" CornerRadius="10">
            <TextBlock FontSize="15" Foreground="White" VerticalAlignment="Center"
                       HorizontalAlignment="Center">
            </TextBlock>
        </control:ZeroProgressBar>
    </Grid>
</UserControl>
```