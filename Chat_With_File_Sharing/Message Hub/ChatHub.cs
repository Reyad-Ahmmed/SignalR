using Chat_With_File_Sharing.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chat_With_File_Sharing.Message_Hub
{
    public class ChatHub : Hub
    {
        private static Dictionary<string, string> users = new Dictionary<string, string>(); //user names show a list
        private readonly IWebHostEnvironment env; 
        public ChatHub(IWebHostEnvironment env) { this.env = env; }
        public override async Task OnConnectedAsync()
        {

            await Clients.Caller.SendAsync("Connected", "You are connected");


        }

        // for disconnected user.
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            users.Remove(Context.ConnectionId);
            await Clients.All.SendAsync("DisconnectedUser", users.Values.ToArray());
        }

        // User Name add method. If user name is duplicate show a alert. Then enter a unique name.
        public async Task AddMe(string username)
        {
            if (users.Values.Contains(username))
            {
                await Clients.Caller.SendAsync("DuplicateUser", "Already have this user name. Please choose a different name.");
            }
            else
            {
                users.Add(Context.ConnectionId, username);
                await Clients.Caller.SendAsync("Joined", "You are joined");
                await Clients.All.SendAsync("UpdateUsers", users.Values.ToArray());
            }
        }

        // Send message and show username and message.
        public async Task Send(string user, string message)
        {
            await Clients.All.SendAsync("Message", user, message);
        }

        // Upload Image.
        public async Task Upload(string user, ImageData data)
        {
            string path =  Path.Combine(this.env.WebRootPath, "Images");
            path = Path.Combine(path, data.Filename);
            data.Image = data.Image.Substring(data.Image.LastIndexOf(',') + 1);
            string converted = data.Image.Replace('-', '+');
            converted = converted.Replace('_', '/');
            byte[] buffer = Convert.FromBase64String(converted);
            FileStream fs = new FileStream($"{path}", FileMode.Create);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
            if (data.Filename.Contains(".jpg") || data.Filename.Contains(".png") || data.Filename.Contains(".gif"))
                await Clients.All.SendAsync("Message", user, $"<a target='_blank' href='/Images/{data.Filename}'><img src='/Images/{data.Filename}' width='40px'  class='rounded'/> </a>");
            else await Clients.All.SendAsync("Message", user, $"<a target='_blank' href='/Images/{data.Filename}'><img src='/Images/docs.png' width='40px'  class='rounded'/> </a>");
        }

    }
}

