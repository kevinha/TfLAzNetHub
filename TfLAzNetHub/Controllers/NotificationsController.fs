namespace TfLAzNetHub.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TfLAzNetHub.Models

type NotificationsController(logger : ILogger<WeatherForecastController>) =
    inherit ControllerBase()
    
    [<HttpPost>]
    member __.Post(pns:string, [<FromBody>] message:string, to_tag:string) =
        let user = __.Request.HttpContext.User.Identity.Name
        let userTags = [|"username:" + toTag; "from:" + user |]
        
        let notif = "{ \"data\" : {\"message\":\"" + "From " + user + ": " + message + "\"}}"
        let outcome = async {
                        Notifications.hub.