module BotMethods
open Microsoft.FSharp.Core
open System.Text.RegularExpressions

type YTVideo(title : string, published : string, channel : string, videoLength : string, shortTime : string, viewsCount : string, link : string) =
    let validString str =        
        if System.String.IsNullOrWhiteSpace str then
            raise (System.ArgumentException("Properties cannot be null"))
    do 
        validString title
        validString published
        validString channel
        validString videoLength
        validString viewsCount
        validString link
    member val Title = title with get, set
    member val Published = published with get, set
    member val Channel = channel with get, set
    member val Time = videoLength with get, set
    member val ShortTime = System.TimeSpan.Parse shortTime with get, set
    member val Views = viewsCount with get, set
    member val Link = link with get, set

let FindProp (section : string) (start : string) (finish : string) : string =     
    let startPos = section.IndexOf(start) + start.Length
    section.[startPos..(section.IndexOf(finish, startPos)-1)]

let VideoEndPos (section : string) (nav :string) = section.IndexOf(nav) + nav.Length

let LimitBounds (value : int) =
    match value with
    | v when v < 1 -> 1
    | v when v > 10 -> 10
    | _ -> value

let GetNumberEmojis (value : int) =
    let numberEmojis = readOnlyDict['0', ":zero:" ; '1', ":one:" ; '2', ":two:" ; '3', ":three:" ; '4', ":four:" ; '5', ":five:" ; '6', ":six:" ; '7', ":seven:" ; '8', ":eight:" ; '9', ":nine:" ]
    let numbers = sprintf "%i" value
    [| for i in 0..(numbers.Length - 1) do yield numberEmojis.[numbers.[i]] |] |> String.concat ""

let GetPageData (searchTerm : string) : string =    
    let url = "https://www.youtube.com/results?search_query=" + searchTerm.Replace(' ', '+')
    let request =  System.Net.WebRequest.Create(url)
    use response = request.GetResponse()
    use recieveStream = response.GetResponseStream()
    use readStream = new System.IO.StreamReader(recieveStream)
    let data = readStream.ReadToEnd()

    data.[data.IndexOf("estimatedResults")..]

let FormatTime (time : string) =
    match Regex.Matches(time, ":").Count with
    | 2 ->  time
    | 1 -> sprintf "00:%s" time
    | _ -> sprintf "00:00:%s" time


let GetVideo (pageSection) : YTVideo =   

    let title = FindProp pageSection "\"title\":{\"runs\":[{\"text\":\"" "\"}]" 
    let channel = FindProp pageSection "\"longBylineText\":{\"runs\":[{\"text\":\"" "\","
    let published = FindProp pageSection "\"publishedTimeText\":{\"simpleText\":\"" "\"}"
    let length = FindProp pageSection "\"lengthText\":{\"accessibility\":{\"accessibilityData\":{\"label\":\"" ":{"
    let time = FindProp length "" "\"}},"
    let shortTime = FormatTime <| FindProp length "},\"simpleText\":\"" "\"},"
    let view = FindProp pageSection "\"viewCountText\":{\"simpleText\":\"" "\"}"
    let nav = FindProp pageSection "\"navigationEndpoint\":{\"" "\"}},"
    let link = "/watch" + FindProp pageSection "\"url\":\"/watch" "\","

    YTVideo(title,channel,published,time,shortTime,view,link)

let MatchWord (search : string) (word : string) =
    match search.ToLower() with
    | x when x.Contains(word.ToLower()) -> true
    | _ -> false

let RestrictCheck (title : string) (searchTerm : string) (restriction : string) =
    let r =
        match restriction.ToLower() with        
        | "partial" -> 1
        | "full" -> searchTerm.Split ' ' |> Array.length
        | _ -> 0

    title.Split ' ' |> Array.filter (fun w -> MatchWord searchTerm w) |> Array.length |> (fun x -> x >= r)

let VideoBounds (videoTime : System.TimeSpan) (min : System.TimeSpan) (max : System.TimeSpan) =
    match videoTime with
    | t when t > max || t < min -> false
    | _ -> true

let GetVideos (restrict : string) (search : string) (min : System.TimeSpan) (max : System.TimeSpan) : YTVideo[] =
    let pageJson = GetPageData search
    [| for section in pageJson.Split ",{\"videoRenderer\":{" -> GetVideo section |]
    |> Array.filter (fun video -> RestrictCheck video.Title search restrict)
    |> Array.filter (fun video -> VideoBounds video.ShortTime min max)