namespace TfLAzNetHub.Controllers

open Microsoft.Azure.NotificationHubs;
open Microsoft.Azure.NotificationHubs.Messaging;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TfLAzNetHub.Models
open System.Threading.Tasks

[<CLIMutable>]
type DeviceRegistration = {
    Platform: string
    Handle: string
    Tags: string array
}

[<ApiController>]
[<Route("[controller]")>]
type RegistrationController (logger : ILogger<WeatherForecastController>) =
    inherit ControllerBase()

    [<HttpPost>]
    member __.Post(handle: string) : Task<string> =
        match Option.ofObj handle with
        | Some h ->
            let regs = async {
                            let! regs = Notifications.hub.GetRegistrationsByChannelAsync(h, 100) |> Async.AwaitTask
                            return regs
                       } |> Async.RunSynchronously
            match regs |> List.ofSeq with
            | [] -> async {
                            let! regId = Notifications.hub.CreateRegistrationIdAsync() |> Async.AwaitTask
                            return regId
                       } |> Async.StartAsTask
            
            | [r] -> Task.FromResult r.RegistrationId
            
            | r::rs ->
                rs |> List.iter (fun x -> async { Notifications.hub.DeleteRegistrationAsync x |> ignore } |> Async.RunSynchronously )
                Task.FromResult r.RegistrationId
                
        | None -> async {
                           let! regId = Notifications.hub.CreateRegistrationIdAsync() |> Async.AwaitTask
                           return regId
                      } |> Async.StartAsTask
            

