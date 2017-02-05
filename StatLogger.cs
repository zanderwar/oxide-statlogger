using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("StatLogger", "Zanderwar", 1.0)]
    [Description("A plugin that sends statistics to endpoints")]

    public class StatLogger : RustPlugin
    {
        /// <summary>
        /// Enable Debug Mode... Will display console messages and responses 
        /// when data is sent to the endpoint
        /// </summary>
        protected bool debug = true;

        /// <summary>
        /// List of endpoints, but contain a baseUrl key
        /// </summary>
        public static Dictionary<string, string> endpoints = new Dictionary<string, string>
        {
            { "baseUrl", "http://dev.scraplands.net/api/" },
            { "currentlyOnline", "currently-online" },
            { "playerKilled", "player-killed" },
            { "playerJoined", "player-joined" },
            { "playerDisconnected", "player-disconnected" },
            { "playerSuicide", "player-suicide" },
            { "playerChat", "player-chat" },
        };

        /// <summary>
        /// Loads current online players into the website.
        /// </summary>
        void Loaded()
        {
            var onlinePlayers = new Dictionary<int, Dictionary<string,string>>();
            var count = 0;
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                onlinePlayers.Add(
                    count, 
                    new Dictionary<string, string> {
                        { "SteamID",  player.userID.ToString() },
                        { "PlayerName", player.displayName.ToString() },
                        { "Address", Regex.Replace(player.net.connection.ipaddress.ToString(), @":{1}[0-9]{1}\d*", "").ToString() }
                    }
                );

                count += 1;
            }

            this.sendData(this.getEndpoint("currentlyOnline"), onlinePlayers, "Currently Online Players has been sent to the website");
        }

        /// <summary>
        /// Sends data about a player that has just connected for use of Player List and Online Player List
        /// </summary>
        /// <param name="player"></param>
        void OnPlayerInit(BasePlayer player)
        {
            var data = new Dictionary<string, string> {
                { "SteamID",  player.userID.ToString() },
                { "PlayerName", player.displayName.ToString() },
                { "Address", Regex.Replace(player.net.connection.ipaddress.ToString(), @":{1}[0-9]{1}\d*", "").ToString() }
            };

            this.sendData(this.getEndpoint("playerJoined"), data, player.userID.ToString() + " has joined and the website has been notified");
        }

        /// <summary>
        /// Player disconnected and informing the endpoint defined about it
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reason"></param>
        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            var data = new Dictionary<string, string> {
                { "SteamID",  player.userID.ToString() }
            };

            this.sendData(this.getEndpoint("playerDisconnected"), data, player.userID.ToString() + " has disconnected and the website has been notified");
        }

        /// <summary>
        /// Sends data about a player death
        /// </summary>
        /// <param name="victim"></param>
        /// <param name="info"></param>
        void OnEntityDeath(BaseCombatEntity victim, HitInfo info)
        {

            if (victim?.ToPlayer() != null && info?.Initiator?.ToPlayer() != null)
            {
                var target = victim.ToPlayer();
                var attacker = info?.Initiator?.ToPlayer();
                
                if (target.userID == attacker.userID)
                {
                    Puts("Self-Inflicted Death: " + target.displayName);
                    return;
                }

                var weapon = info?.Weapon?.GetItem()?.info?.displayName?.english ?? "Unknown";
                var bone = info?.boneName ?? "Unknown";
                var attackerHealth = Convert.ToString(info.Initiator.Health()) ?? "Unknown";
                var distance = Convert.ToString(attacker.Distance(target.transform.position)) ?? "Unknown";

                var data = new Dictionary<string, string> {
                    { "VictimID",  target.userID.ToString() },
                    { "KillerID",  attacker.userID.ToString() },
                    { "KillerHP", attackerHealth },
                    { "Weapon", weapon },
                    { "Distance", distance },
                    { "Bone", bone }
                };

                this.sendData(this.getEndpoint("playerKilled"), data, "Sending Kill to Website");
            }
        }

        /// <summary>
        /// Sends chat messages
        /// </summary>
        /// <param name="arg"></param>
        void OnPlayerChat(ConsoleSystem.Arg arg)
        {
            BasePlayer player = (BasePlayer)arg.Connection.player;
            string message = arg.GetString(0, "");

            var data = new Dictionary<string, string>
            {
                { "SteamID",  player.userID.ToString() },
                { "Message",  message }
            };

            this.sendData(this.getEndpoint("playerChat"), data, "Sending player chat");
        }

        /// <summary>
        /// DRY method to send data to the designated endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="dictionary"></param>
        /// <param name="debugMessage"></param>
        protected void sendData(string endpoint, object dictionary, string debugMessage = null)
        {
            string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

            if (this.debug && !String.IsNullOrEmpty(debugMessage))
            {
                Puts(debugMessage);
            }

            webrequest.EnqueuePost(endpoint, "data=" + json, (code, response) => printResponse(response), this);
        }
        
        /// <summary>
        /// Callback method to print the response to console
        /// </summary>
        /// <param name="response"></param>
        protected void printResponse(string response)
        {
            if (!this.debug || String.IsNullOrEmpty(response))
            {
                return;
            }

            Puts(response);
        }

        /// <summary>
        /// Get for the endpoint list, combines baseUrl with any other endpoints defined
        /// </summary>
        /// <param name="forWhat">What endpoint do you want to connect the baseUrl with</param>
        /// <returns></returns>
        protected string getEndpoint(string forWhat)
        {
            return StatLogger.endpoints["baseUrl"] + StatLogger.endpoints[forWhat];
        }
    }
}
