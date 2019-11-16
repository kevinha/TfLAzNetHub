namespace TfLAzNetHub

    open System.Security.Principal
    open Microsoft.AspNetCore.Http
    open System.Text
    open System
    open System.Web
    open Microsoft.AspNetCore.Mvc
    open System.Threading
    open System.Threading.Tasks
    open System.Net
    open System.Net.Http
    open Microsoft.AspNetCore.Builder

    type SimpleAuth (next: RequestDelegate) =
        let _next = next
        
        let verifyUserAndPassword u p =
            u = p
            
        let Unauthorized() : Task<HttpResponseMessage> =
            let response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            let tsc = TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            tsc.Task;
        
        member __.Invoke(context: HttpContext) =
            let req = context.Request
            let found, values = req.Headers.TryGetValue "Authorization"
            match found, values with
            | false, _ -> context.Response.StatusCode <- int HttpStatusCode.Forbidden
                          Task.CompletedTask
            
            | true, values ->
                let uandp = values.[0].Substring("Basic ".Length)
                            |> Convert.FromBase64String
                            |> Encoding.Default.GetString
                let (u, p) = uandp.Split ":" |> (fun arr -> arr.[0], arr.[1])
                
                if verifyUserAndPassword u p then
                    context.User <- GenericPrincipal(GenericIdentity u, Array.empty<string>)
                    System.Threading.Thread.CurrentPrincipal <- context.User
                    _next.Invoke(context)
                else
                    context.Response.StatusCode <- int HttpStatusCode.Forbidden
                    Task.CompletedTask
                    
    module Extensions =

        type IApplicationBuilder with
            member __.UseSimpleAuth() =
                __.UseMiddleware<SimpleAuth>()
    