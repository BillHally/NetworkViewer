open System

open System.Net
open System.Net.NetworkInformation

[<EntryPoint>]
let main argv =
    let gate = obj()

    [|
        for i in 1..255 do
            yield
                async {
                    use ping = new Ping()

                    let ip = sprintf "192.168.0.%d" i
                    let! result = Async.AwaitTask (ping.SendPingAsync(ip))
                    if result.Status = IPStatus.Success then
                        let! host = Async.AwaitTask (Dns.GetHostEntryAsync(result.Address))

                        lock gate
                            (
                                fun () ->
                                    let addressesString =
                                        match host.AddressList with
                                        | [||] -> ""
                                        | xs ->
                                             " " +
                                                (
                                                    xs
                                                    |> Array.map string
                                                    |> (fun xs -> String.Join(" ", xs))
                                                )

                                    printfn "%O: %s%s" result.Address host.HostName addressesString)
                }
    |]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore<unit[]>

    0
