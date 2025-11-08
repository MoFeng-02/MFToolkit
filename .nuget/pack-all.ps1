# 定义项目路径
$projects = @(
    #"../src/Common/MFToolkit"
    #"../src/Common/MFToolkit.AspNetCore",
    #"../src/Common/MFToolkit.Abstractions",
    #"../src/Common/MFToolkit.AutoGenerator"
    "../src/AvaloniaUI/MFToolkit.Avaloniaui"
    #"../src/Minecraft/MFToolkit.Minecraft"
    #1"../src/Applet/WeChat/MFToolkit.WeChat",
    #1"../src/Applet/Integration/MFToolkit.Integration.Applet",
   # "../src/SqlEntityCore/MFToolkit.SqlSugarCore.Extensions"
)

# 定义输出目录
$outputDir = "./nupkgs"

# 如果输出目录不存在，则创建
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir
}

# 打包所有项目
foreach ($project in $projects) {
    dotnet pack $project --configuration Release --output $outputDir
}

# 定义 NuGet 服务器和 API Key
$nugetSource = "https://api.nuget.org/v3/index.json"
# 从环境变量读取Key
$apiKey = $apiKey = $env:NUGET_API_KEY

# 获取所有 .nupkg 文件
$nupkgFiles = Get-ChildItem -Path $outputDir -Filter *.nupkg

# 按项目名称分组，并筛选每个项目的最新版本
$latestPackages = $nupkgFiles | Group-Object { $_.Name -replace '\.\d+\.\d+\.\d+.*\.nupkg$', '' } | ForEach-Object {
    $_.Group | Sort-Object Name -Descending | Select-Object -First 1
}

# 上传每个项目的最新版本
foreach ($nupkg in $latestPackages) {
    dotnet nuget push $nupkg.FullName --source $nugetSource --api-key $apiKey
    Write-Host "Uploaded latest package: $($nupkg.Name)"
}