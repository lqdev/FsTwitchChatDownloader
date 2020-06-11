# FsTwitchChatDownloader

F# utility for downloading chats from Twitch. This solution uses v5 of the Twitch API.

## Prerequisites

- .NET Core SDK
- Twitch credentials (Client ID + Token)

## Use

Add path to your NuGet packages at the top of the file. See [default locations for global packages](https://docs.microsoft.com/nuget/Consume-Packages/managing-the-global-packages-and-cache-folders)

```fsharp
#I "/Users/<YOUR-USER-NAME>/.nuget/packages"
```

The sample below downloads the full chat for a specified video. The chat is downloaded at 60 second intervals. 

```fsharp
let videoId = "YOUR-VIDEO-ID"
let credentials = Both ("YOUR-CLIENT-ID",("Oauth","YOUR-TOKEN"))
let videoUrl = 
    sprintf "https://api.twitch.tv/v5/videos/%s" videoId |> Url

let videoDetails = getVideoAsync videoUrl credentials |> Async.RunSynchronously

let fullChat = 
    downloadChat videoId credentials videoDetails.Duration 0 60 []
```
