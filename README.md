# OAuth2Homework 使用說明

1. 首先要安裝 .NET 6
2. 安裝完成後進入 OAuth2Homework 目錄內
3. 執行 `dotnet restore`
4. 執行 `dotnet dev-certs https`
5. 執行 `dotnet run`

這樣程式就會跑起來，然後預設網址為 `https://localhost:7022/`

> 這邊網址必須是 `https://localhost:7022/`，因為在 Line 那邊設定的 callback url 是這組