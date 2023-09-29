using System.Collections.Generic;

namespace BlazorServerAppWithIdentity.Api.Hubs
{
    public class ConnectedUser
    {
        public string UserIdentifier { get; set; }

        public List<Connection> connections { get; set; }
    }
    public class Connection
    {
        public string ConnectionId { get; set; }

    }
}
