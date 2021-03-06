﻿using Microsoft.Extensions.Logging;
using SkunkLab.Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualRtu.Communications.Tcp
{
    public class TcpConnection
    {
        public TcpConnection(string id, string address, int port, ExponentialDelayPolicy policy, ILogger logger = null)
        {
            this.id = id;
            this.address = address;
            this.port = port;
            this.policy = policy;
            this.logger = logger;
        }

        public event System.EventHandler<TcpReceivedEventArgs> OnReceived;
        
        private string id;
        private string address;
        private int port;
        private ExponentialDelayPolicy policy;
        private ILogger logger;
        private IChannel channel;
        private CancellationTokenSource cts;

        public async Task OpenAsync()
        {
            cts = new CancellationTokenSource();
            channel = SkunkLab.Channels.ChannelFactory.Create(false, new IPEndPoint(IPAddress.Parse(address), port), 1024, 102400, cts.Token);
            channel.OnClose += Channel_OnClose;
            channel.OnError += Channel_OnError;
            channel.OnReceive += Channel_OnReceive;
            channel.OnOpen += Channel_OnOpen;
            channel.OnStateChange += Channel_OnStateChange;
            
            await channel.OpenAsync();
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            logger?.LogDebug($"Channel '{e.ChannelId}' state = '{e.State}'.");
        }

        public async Task SendAsync(byte[] message)
        {
            if(channel.State == ChannelState.Open)
            {
                try
                {
                    await channel.SendAsync(message);
                    logger?.LogDebug($"Channel '{channel.Id}' sent message");
                }
                catch(Exception ex)
                {
                    logger?.LogError(ex, $"Channel '{channel.Id}' fault during send.");
                }
            }
            else
            {
                logger?.LogInformation("Channel not open and unable to send.");
            }
        }

        public async Task CloseAsync()
        {
            if(channel != null)
            {
                await channel.CloseAsync();
                channel.Dispose();
            }

            channel = null;
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            logger?.LogInformation($"Channel {address}:{port} '{e.ChannelId}' open.");
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            OnReceived?.Invoke(this, new TcpReceivedEventArgs(this.id, e.Message));
            logger?.LogDebug($"Channel '{e.ChannelId}' received message.");
        }

        private async void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger?.LogError(e.Error, $"TCP channel '{e.ChannelId}' error.");
            try
            {
                await channel.CloseAsync();
            }
            catch(Exception ex)
            {
                logger?.LogError(ex, $"TCP channel '{e.ChannelId}' error attempting close after error.");
            }
        }

        private async void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            logger?.LogWarning($"TCP channel '{e.ChannelId}' closing.");
            try
            {
                channel.Dispose();
                channel = null;
            }
            catch(Exception ex)
            {
                logger?.LogError(ex, $"TCP channel fault attempt closed and dispose.");
            }

            policy.Delay();
            await OpenAsync();
        }
    }
}
