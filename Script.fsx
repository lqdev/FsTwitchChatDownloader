#r "newtonsoft.json/12.0.3/lib/netstandard2.0/Newtonsoft.Json.dll"

open System.Net.Http
open Newtonsoft.Json
open System.Threading.Tasks

type Credentials =
    | ClientId of string
    | Token of (string * string)
    | Both of (string * (string * string))

type VideoId = string
type OffsetSeconds = int
type Duration = int
type Url = string
type BuildUrl = VideoId -> OffsetSeconds -> Url
type MakeRequest<'a> = Url -> Credentials -> 'a
type Unwrap<'a, 'b> = 'a -> 'b
type FullChat = VideoId -> Credentials -> Duration -> OffsetSeconds -> OffsetSeconds -> Comment list

type Commenter =
    { [<JsonProperty("display_name")>]
      DisplayName: string }

type Message =
    { [<JsonProperty("body")>]
      Body: string }

type Comment =
    { [<JsonProperty("_id")>]
      Id: string
      [<JsonProperty("commenter")>]
      Commenter: Commenter
      [<JsonProperty("message")>]
      Message: Message }

type Chat = { Comments: Comment list }

type Video =
    { [<JsonProperty("_id")>]
      Id: VideoId
      [<JsonProperty("length")>]
      Duration: Duration }


let (buildUrl: BuildUrl) =
    fun videoId offsetSeconds ->
        sprintf "https://api.twitch.tv/v5/videos/%s/comments?content_offset_seconds=%i" videoId offsetSeconds

let getChatAsync: MakeRequest<Async<Chat>> =
    fun url credentials ->

        let chat =
            async {
                use client = new HttpClient()

                match credentials with
                | ClientId id -> client.DefaultRequestHeaders.Add("client-id", id)
                | Token (scheme, token) ->
                    client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue(scheme, token)
                | Both (id, (scheme, token)) ->
                    client.DefaultRequestHeaders.Add("client-id", id)
                    client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue(scheme, token)

                let! req = client.GetStringAsync(url) |> Async.AwaitTask
                let body = JsonConvert.DeserializeObject<Chat>(req)
                return body
            }

        chat

let getVideoAsync: MakeRequest<Async<Video>> =
    fun url credentials ->

        let video =
            async {
                use client = new HttpClient()

                match credentials with
                | ClientId id -> client.DefaultRequestHeaders.Add("client-id", id)
                | Token (scheme, token) ->
                    client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue(scheme, token)
                | Both (id, (scheme, token)) ->
                    client.DefaultRequestHeaders.Add("client-id", id)
                    client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue(scheme, token)

                let! req = client.GetStringAsync(url) |> Async.AwaitTask

                let body =
                    JsonConvert.DeserializeObject<Video>(req)

                return body
            }

        video

let rec (downloadChat:FullChat) = 
    fun videoId credentials duration offset interval conversation =
        let url = buildUrl videoId offset

        let increase = min (duration - offset) interval

        match offset with
        | x when x = duration -> conversation
        | x when x < duration ->
            let comments =
                getChatAsync url credentials
                |> Async.RunSynchronously

            Task.Delay(3000) |> ignore
            printfn "Processed offset %i" offset
            downloadChat videoId credentials duration (offset + increase) interval (conversation @ comments.Comments)
        | _ -> []
