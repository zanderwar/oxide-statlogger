# oxide-statlogger

This is an Oxide plugin designed to send specific data to designated endpoints for collection and/or statistical purposes

### Configuration

In the code itself, you will be required to change the endpoints below to conform with your setup (if you want a custom implementation you can contact me at reece.alexander@gmail.com with your requirements for a quote)

```c#
/// <summary>
/// List of endpoints, but contain a baseUrl key
/// </summary>
public static Dictionary<string, string> endpoints = new Dictionary<string, string>
{
    { "baseUrl", "http://dev.example.net/api/" },
    { "baseEventsUrl", "http://dev.example.net/eventsapi/" },
    { "currentlyOnline", "currently-online" },
    { "playerKilled", "player-killed" },
    { "playerJoined", "player-joined" },
    { "playerDisconnected", "player-disconnected" },
    { "playerSuicide", "player-suicide" },
    { "playerChat", "player-chat" },
    { "serverInfo", "server-info" }
};
```

All endpoints are concatenated to the baseUrl used at the time, using the above as an example; when a player killed another data would be sent to http://dev.example.net/api/player-killed so and on so forth.

You should **always** rely on the players SteamID as a unique identifier when handling/correlating data on the recipient side.

### Features

- When the plugin is first initialised it will dispatch a list of all currently online players to the endpoint specified in the configuration (currentlyOnline). After storing this data, you should only rely on playerJoined and playerDisconnected to modify the online list respectively.
- Server Information (Hostname, MaxPlayers, EntityCount, Uptime etc) is sent periodically on a 2minute schedule, on the recipient side you can use this information to determine if the logger is connected (for example if you havn't received any data after 2minutes and 15 seconds then there is a problem and logger should be assumed to be disconnected)
- Kill Data (Killer Steam ID, Victim Steam ID, Distance of Kill, Weapon Used, Killers HP when the Victim died)
- Chat Data (Player Steam ID, Chat Message)
