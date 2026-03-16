using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.Hubs
{
    public class ChatHub : Hub
    {
        private static ConnectionMapping<string> _connections = new ConnectionMapping<string>();
        public void Send(int convID, string message)
        {

            var OtherUser = new ChatRepository().SaveReply(new ChatReplyVM
            {
                ConvID = convID,
                ReplyBody = message,
                UserID = Context.User.Identity.GetUserId(),
            });
            Clients.Caller.UploadFiles(OtherUser.Item2.ID);
            foreach (var u in OtherUser.Item1)
                foreach (var connectionId in _connections.GetConnections(u))
                {
                    Clients.Client(connectionId).addNewMessageToPage(new
                    {
                        convID,
                        message,
                        side = "left",
                        name = OtherUser.Item2.Name,
                        time = string.Format("{0:hh:mm tt}", Common.GetCurrentDate()),
                        Pic ="/Images/"+ OtherUser.Item2.Pic,
                        OtherUser.Item2.ID,
                        FilePath = ""
                    });
                }
        }

        public void SendConnection()
        {
            var a = _connections.ALL;
            Clients.All.UpdateStatus(a);
        }
        public void SendFile(long ID, string File)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            context.Clients.All.AttachFile(ID,File);
        }
        public override Task OnConnected()
        {
            string name = Context.User.Identity.GetUserId();
            _connections.Add(name, Context.ConnectionId); SendConnection();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.GetUserId();
            _connections.Remove(name, Context.ConnectionId); SendConnection();
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string name = Context.User.Identity.GetUserId();
            if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
                SendConnection();
            }
            return base.OnReconnected();
        }
    }
    public interface IUserIdProvider
    {
        string GetUserId(IRequest request);
    }
    public class ConnectionMapping<T>
    {
        private Dictionary<T, HashSet<string>> _connections = new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public List<T> ALL
        {
            get
            {
                return _connections.Keys.Distinct().ToList();
            }
        }


        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }
    }
}