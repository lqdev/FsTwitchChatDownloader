# FsTwitchChatDownloader

F# utility for downloading chats from Twitch. This solution uses v5 of the Twitch API.

## Prerequisites

- .NET Core SDK
- Twitch credentials (Client ID + Token)

## Use

Add path to your NuGet packages at the top of the `Script.fsx` file. See [default locations for global packages](https://docs.microsoft.com/nuget/Consume-Packages/managing-the-global-packages-and-cache-folders)

```fsharp
#I "/Users/<YOUR-USER-NAME>/.nuget/packages"
```

At the end of `Script.fsx`, add the following code. The sample below downloads the full chat for a specified video. Downloading starts at second 0. The chat is downloaded at 60 second intervals and keeps appending the results to the list of comments, which initially is empty. Then, a `Chat` object is created and stored in a file called `chat.json`.

```fsharp
open System.IO

let videoId:VideoId = "YOUR-VIDEO-ID"
let credentials = Both ("YOUR-CLIENT-ID",("OAuth","YOUR-TOKEN"))

let videoUrl:Url =
    sprintf "https://api.twitch.tv/v5/videos/%s" videoId

let videoDetails = getVideoAsync videoUrl credentials |> Async.RunSynchronously

let comments =
    downloadChat videoId credentials videoDetails.Duration 0 60 []

let fullChat = { Comments = comments }

File.WriteAllText("chat.json", (fullChat |> JsonConvert.SerializeObject))
```

Then, in the command line, run

```dotnetcli
dotnet fsi exec Script.fsx
```
