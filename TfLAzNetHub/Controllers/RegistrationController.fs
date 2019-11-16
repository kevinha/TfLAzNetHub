namespace TfLAzNetHub.Controllers

open System.Collections.Generic
open System.Net.Http
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
        

    [<HttpPut>]
    member __.Put(id:string, deviceUpdate:DeviceRegistration) =
        let reg = FcmRegistrationDescription(deviceUpdate.Handle)
        reg.RegistrationId <- id
        reg.Tags = (HashSet<string>(deviceUpdate.Tags) :> ISet<string>)

        async { let! _ = Notifications.hub.CreateRegistrationAsync(reg) |> Async.AwaitTask
                return () } |> Async.RunSynchronously
        Task.FromResult (OkResult())

    [<HttpDelete>]
    member __.Delete(id: string) =
        async { let! _ = Notifications.hub.DeleteRegistrationAsync(id) |> Async.AwaitTask
                return () } |> Async.RunSynchronously
        Task.FromResult (OkResult())