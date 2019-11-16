namespace TfLAzNetHub.Models

open Microsoft.Azure.NotificationHubs

module Notifications =
    
    let hub = NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://tfl-gladys.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=z7Mck3Tf11637PwIlPT8BFzGLr4lr/Dlmly0kNj0f5U=", "insights")

