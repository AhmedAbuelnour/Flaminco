﻿using Flaminco.RedisChannels.Options;
using Flaminco.RedisChannels.Subscribers;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace WebApplication1.Publishers
{
    public class TESTRedisPublisher : ChannelPublisher
    {
        protected override RedisChannel Channel => RedisChannel.Literal("test-channel");
        public TESTRedisPublisher(IOptions<RedisChannelConfiguration> options) : base(options)
        {

        }
    }
}
