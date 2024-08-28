using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrangersChat2.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> _connections = new Dictionary<string, string>();
        private static readonly List<string> _waitingList = new List<string>();

        public override async Task OnConnectedAsync()
        {
            // Add the new connection to the waiting list
            _waitingList.Add(Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var partnerConnectionId))
            {
                // Notify the partner that their partner has disconnected
                if (partnerConnectionId != null)
                {
                    await Clients.Client(partnerConnectionId).SendAsync("PartnerDisconnected");
                }

                // Remove the connection from the paired list
                _connections.Remove(partnerConnectionId);
            }

            // Remove the disconnected user from the lists
            _connections.Remove(Context.ConnectionId);
            _waitingList.Remove(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task StartChat()
        {
            var currentConnectionId = Context.ConnectionId;

            // Check if already paired
            if (_connections.ContainsKey(currentConnectionId))
            {
                var partnerConnectionId = _connections[currentConnectionId];
                if (partnerConnectionId != null)
                {
                    await Clients.Client(currentConnectionId).SendAsync("ConnectedToPartner", partnerConnectionId);
                    return;
                }
            }

            // Try to find a partner from the waiting list
            var availableConnections = _waitingList.Except(new[] { currentConnectionId }).ToList();

            if (availableConnections.Any())
            {
                var newPartnerConnectionId = availableConnections.First(); // Pick the first available client

                // Pair the users
                _connections[currentConnectionId] = newPartnerConnectionId;
                _connections[newPartnerConnectionId] = currentConnectionId;

                // Remove from waiting list
                _waitingList.Remove(currentConnectionId);
                _waitingList.Remove(newPartnerConnectionId);

                // Notify both clients
                await Clients.Client(currentConnectionId).SendAsync("ConnectedToPartner", newPartnerConnectionId);
                await Clients.Client(newPartnerConnectionId).SendAsync("ConnectedToPartner", currentConnectionId);
            }
            else
            {
                // Notify the client that no partners are available
                await Clients.Client(currentConnectionId).SendAsync("NoAvailablePartners");
            }
        }

        public async Task RefreshChat()
        {
            var currentConnectionId = Context.ConnectionId;

            // End the current chat if already paired
            if (_connections.TryGetValue(currentConnectionId, out var partnerConnectionId) && partnerConnectionId != null)
            {
                // Notify the partner that the current user has disconnected
                await Clients.Client(partnerConnectionId).SendAsync("PartnerDisconnected");

                // Clear the connection pairing
                _connections.Remove(partnerConnectionId);
                _connections.Remove(currentConnectionId);

                // Add both users back to the waiting list
                _waitingList.Add(partnerConnectionId);
            }

            // Try to connect the user with a new partner
            await StartChat();
        }

        public async Task SendMessage(string message)
        {
            var currentConnectionId = Context.ConnectionId;

            if (_connections.TryGetValue(currentConnectionId, out var partnerConnectionId) && partnerConnectionId != null)
            {
                await Clients.Client(partnerConnectionId).SendAsync("ReceiveMessage", message);
            }
        }

        public async Task EndChat()
        {
            var currentConnectionId = Context.ConnectionId;

            if (_connections.TryGetValue(currentConnectionId, out var partnerConnectionId) && partnerConnectionId != null)
            {
                // Notify the partner that the current user has disconnected
                await Clients.Client(partnerConnectionId).SendAsync("PartnerDisconnected");

                // Notify the current user that they have disconnected
                await Clients.Client(currentConnectionId).SendAsync("YouDisconnected");

                // Clear the connection pairing
                _connections.Remove(partnerConnectionId);
                _connections.Remove(currentConnectionId);

                // Add both users back to the waiting list
                _waitingList.Add(currentConnectionId);
                _waitingList.Add(partnerConnectionId);
            }
            else
            {
                // Notify the current user that they have disconnected
                await Clients.Client(currentConnectionId).SendAsync("YouDisconnected");

                // Remove the user from the waiting list
                _connections.Remove(currentConnectionId);
                _waitingList.Add(currentConnectionId);
            }
        }
    }
}
