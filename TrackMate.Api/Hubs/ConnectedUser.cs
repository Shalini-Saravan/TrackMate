using System.Collections.Generic;

namespace TrackMate.Api.Hubs
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
