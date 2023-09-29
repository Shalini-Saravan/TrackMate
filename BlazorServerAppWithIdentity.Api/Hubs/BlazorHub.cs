using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Api.Hubs
{
    [Authorize(AuthenticationSchemes ="Bearer")]
    public class BlazorHub : Hub
    {
        private static List<ConnectedUser> ConnectedUsers = new List<ConnectedUser>();
        public void InitializeUserList()
        {
            var list = (from user in ConnectedUsers
                        select user.UserIdentifier).ToList();
        }
        public override async Task OnConnectedAsync()
        {

            string UserId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = ConnectedUsers.Where(cu => cu.UserIdentifier == UserId).FirstOrDefault();
            System.Diagnostics.Debug.WriteLine(" -- hub connected -- " + UserId);
            if (user == null) // user does not exist
            {
                ConnectedUser connecteduser = new ConnectedUser
                {
                    UserIdentifier = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    connections = new List<Connection> { new Connection { ConnectionId = Context.ConnectionId } }
                };
                
                ConnectedUsers.Add(connecteduser);
            }
            else
            {
                user.connections.Add(new Connection { ConnectionId = Context.ConnectionId });
            }
            await Clients.User(Context.UserIdentifier??"").SendAsync("Connected");
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            System.Diagnostics.Debug.WriteLine(" -- hub disconnected -- " + Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ConnectedUsers.Find(o => o.UserIdentifier == Context.User.FindFirst(ClaimTypes.NameIdentifier).Value).connections.RemoveAll(o => o.ConnectionId == Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessageToUser(string userIdentifier, string message)
        {
           await Clients.User(userIdentifier).SendAsync("privateNotification",
                                       userIdentifier, message);

        }



    }
}
