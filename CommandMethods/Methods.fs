module BotMethods

type YTVideo(title : string, published : string, channel : string, videoLength : string, viewsCount : string, link : string, charPosition : int) =
    
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
    member val Views = viewsCount with get, set
    member val Link = link with get, set
    member val CharPosition = charPosition with get, set

let FindProp (section : string) (start : string) (finish : string) : string =     
    let startPos = section.IndexOf(start) + start.Length
    section.[startPos..section.IndexOf(finish, startPos)]

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
    let jsonStart = data.IndexOf("estimatedResults")

    data.[jsonStart..]


let GetVideo (pageSection) : YTVideo =   

    let title = FindProp pageSection "\"title\":{\"runs\":[{\"text\":\"" "\"}]" 
    let channel = FindProp pageSection "\"longBylineText\":{\"runs\":[{\"text\":\"" "\","
    let published = FindProp pageSection "\"publishedTimeText\":{\"simpleText\":\"" "\"}"
    let length = FindProp pageSection "\"lengthText\":{\"accessibility\":{\"accessibilityData\":{\"label\":\"" "\"}"
    let view = FindProp pageSection "\"viewCountText\":{\"simpleText\":\"" "\"}"
    let nav = FindProp pageSection "\"navigationEndpoint\":{\"" "\"}},"
    let link = FindProp nav "\"url\":\"" "\","

    YTVideo(title,channel,published,length,view,link, VideoEndPos pageSection nav)


//let GetVideos (limit : int) (restrict : string) (search : string) (min : System.TimeSpan) (max : System.TimeSpan) : YTVideo =
    
//    let pageJson = GetPageData search
//    let amount = LimitBounds limit

//    let mutable charPos : int = 0
