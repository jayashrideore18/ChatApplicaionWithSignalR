﻿using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;


namespace chatAppllicationSignalR_19_09_2020
{
    public class ChatHub : Hub
    {
        //public static string ConnC = ConfigurationManager.ConnectionStrings["conStr"].ToString();
        //public void Hello()
        //{
        //    Clients.All.hello();
        //}
        static List<Users> ConnectedUsers = new List<Users>();
        static List<Messages> CurrentMessage = new List<Messages>();
        ConnClass ConnC = new ConnClass();
        public void Connect(string userName)
        {
            var id = Context.ConnectionId;

            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                string UserImg = GetUserImage(userName);
                string logintime = DateTime.Now.ToString();
                ConnectedUsers.Add(new Users { ConnectionId = id, UserName = userName, UserImage = UserImg, LoginTime = logintime });

                // send to caller
                Clients.Caller.onConnected(id, userName, ConnectedUsers, CurrentMessage);

                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, userName, UserImg, logintime);
            }
        }

        public void SendMessageToAll(string userName, string message, string time)
        {
            string UserImg = GetUserImage(userName);
            // store last 100 messages in cache
            AddMessageinCache(userName, message, time, UserImg);

            // Broad cast message
            Clients.All.messageReceived(userName, message, time, UserImg);

        }

        private void AddMessageinCache(string userName, string message, string time, string UserImg)
        {
            CurrentMessage.Add(new Messages { UserName = userName, Message = message, Time = time, UserImage = UserImg });

            if (CurrentMessage.Count > 100)
                CurrentMessage.RemoveAt(0);

            // Refresh();
        }

        public string GetUserImage(string username)
        {
            string RetimgName = "images/dummy.png";
            
                string query = "select photo from tbl_Users where userName='" + username + "'";
                string ImageName = ConnC.GetColumnVal(query, "photo");

                if (ImageName != "")
                    RetimgName = "images/" + ImageName;
            
           
            return RetimgName;
        }


        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                ConnectedUsers.Remove(item);

                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.UserName);

            }
            return base.OnDisconnected(stopCalled);
        }
    }
}