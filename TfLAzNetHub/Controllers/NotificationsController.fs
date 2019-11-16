namespace TfLAzNetHub.Controllers

open System.Net
open System.Net.Http
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TfLAzNetHub.Models
open Microsoft.Azure.NotificationHubs

type NotificationsController(logger : ILogger<WeatherForecastController>) =
    inherit ControllerBase()
    
    [<HttpPost>]
    member __.Post(pns:string, [<FromBody>] message:string, toTag:string) =
        task {
            let user = __.Request.HttpContext.User.Identity.Name
            let userTags = [|"username:" + toTag; "from:" + user |]
        
            let notif = "{ \"data\" : {\"message\":\"" + "From " + user + ": " + message + "\"}}"
            let! outcome = Notifications.hub.SendFcmNativeNotificationAsync(notif, userTags)
            let res = match Option.ofObj outcome with
                        | Some o when o.State = NotificationOutcomeState.Abandoned || o.State = NotificationOutcomeState.Unknown ->
                            new HttpResponseMessage(HttpStatusCode.InternalServerError)

                        | Some _ ->
                            new HttpResponseMessage(HttpStatusCode.OK)

                        | None ->
                            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            return res
        }
