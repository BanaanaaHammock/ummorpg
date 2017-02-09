// We implemented a chat system that works directly with UNET. The chat supports
// different channels that can be used to communicate with other players:
// 
// - **Local Chat:** by default, all messages that don't start with a **/** are
// addressed to the local chat. If one player writes a local message, then all
// players around him _(all observers)_ will be able to see the message.
// - **Whisper Chat:** a player can write a private message to another player by
// using the **/ name message** format.
// - **Guild Chat:** we implemented guild chat support with the **/g message**
// command. Please note that the guild feature itself is still in development,
// so the message will not be read by anyone just yet.
// - **Info Chat:** the info chat can be used by the server to notify all
// players about important news. The clients won't be able to write any info
// messages.
// 
// _Note: the channel names, colors and commands can be edited in the Inspector
// by selecting the Player prefab and taking a look at the PlayerChat
// component._
// 
// A player can also click on a chat message in order to reply to it.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ChannelInfo {
    public string command; // /w etc.
    public string identifierOut; // for sending
    public string identifierIn; // for receiving
    public Color color;

    public ChannelInfo(string _command, string _identifierOut, string _identifierIn, Color _color) {
        command = _command;
        identifierOut = _identifierOut;
        identifierIn = _identifierIn;
        color = _color;
    }
}

[System.Serializable]
public class MessageInfo {
    public string content; // the actual message
    public string replyPrefix; // copied to input when clicking the message
    public Color color;

    public MessageInfo(string sender, string identifier, string message, string _replyPrefix, Color _color) {
        // construct the message (we don't really need to save all the parts,
        // also this will save future computations)
        content = "<b>" + sender + identifier + ":</b> " + message;
        replyPrefix = _replyPrefix;
        color = _color;
    }
}

[NetworkSettings(channel=Channels.DefaultUnreliable)]
public class PlayerChat : NetworkBehaviour {
    // channels
    [Header("Channels")]
    [SerializeField] ChannelInfo chanWhisper = new ChannelInfo("/w", "(TO)", "(FROM)", Color.magenta);
    [SerializeField] ChannelInfo chanLocal = new ChannelInfo("", "", "", Color.white);
    [SerializeField] ChannelInfo chanGuild = new ChannelInfo("/g", "(Guild)", "(Guild)", Color.cyan);
    [SerializeField] ChannelInfo chanInfo = new ChannelInfo("", "(Info)", "(Info)", Color.red);

    [Header("Other")]
    public int maxLength = 70;

    [Client]
    public override void OnStartLocalPlayer() {
        // test messages
        AddMessage(new MessageInfo("", chanInfo.identifierIn, "Just type a message here to chat!", "", chanInfo.color));
        AddMessage(new MessageInfo("", chanInfo.identifierIn, "  Use /g for guild chat", "",  chanInfo.color));
        AddMessage(new MessageInfo("", chanInfo.identifierIn, "  Use /w NAME to whisper a player", "",  chanInfo.color));
        AddMessage(new MessageInfo("", chanInfo.identifierIn, "  Or click on a message to reply", "",  chanInfo.color));
        AddMessage(new MessageInfo("Someone", chanGuild.identifierIn, "Anyone here?", "/g ",  chanGuild.color));
        AddMessage(new MessageInfo("Someone", chanWhisper.identifierIn, "Are you there?", "/w Someone ",  chanWhisper.color));
        AddMessage(new MessageInfo("Someone", chanLocal.identifierIn, "Hello!", "",  chanLocal.color));
    }

    // submit tries to send the string and then returns the new input text
    [Client]
    public string OnSubmit(string s) {
        // not empty and not only spaces?
        if (!Utils.IsNullOrWhiteSpace(s)) {
            // command in the commands list?
            // note: we don't do 'break' so that one message could potentially
            //       be sent to multiple channels (see mmorpg local chat)
            var lastcommand = "";
            if (s.StartsWith(chanWhisper.command)) {
                // whisper
                var parsed = ParsePM(chanWhisper.command, s);
                var user = parsed[0];
                var msg = parsed[1];
                if (!Utils.IsNullOrWhiteSpace(user) && !Utils.IsNullOrWhiteSpace(msg)) {
                    if (user != name) {
                        lastcommand = chanWhisper.command + " " + user + " ";
                        CmdMsgWhisper(user, msg);
                    } else print("cant whisper to self");
                } else print("invalid whisper format: " + user + "/" + msg);
            } else if (!s.StartsWith("/")) {
                // local chat is special: it has no command
                lastcommand = "";
                CmdMsgLocal(s);
            } else if (s.StartsWith(chanGuild.command)) {
                // guild
                var msg = ParseGeneral(chanGuild.command, s);
                lastcommand = chanGuild.command + " ";
                CmdMsgGuild(msg);
            }

            // input text should be set to lastcommand
            return lastcommand;
        }

        // input text should be cleared
        return "";
    }

    [Client]
    void AddMessage(MessageInfo mi) {
        FindObjectOfType<UIChat>().AddMessage(mi);
    }

    // parse a message of form "/command message"
    static string ParseGeneral(string command, string msg) {
        if (msg.StartsWith(command + " "))
            // remove the "/command " prefix
            return msg.Substring(command.Length + 1); // command + space
        return "";
    }

    static string[] ParsePM(string command, string pm) {
        // parse to /w content
        var content = ParseGeneral(command, pm);

        // now split the content in "user msg"
        if (content != "") {
            // find the first space that separates the name and the message
            var i = content.IndexOf(" ");
            if (i >= 0) {
                var user = content.Substring(0, i);
                var msg = content.Substring(i+1);
                return new string[] {user, msg};
            }
        }
        return new string[] {"", ""};
    }

    // networking //////////////////////////////////////////////////////////////
    [Command(channel=Channels.DefaultUnreliable)] // unimportant => unreliable
    void CmdMsgLocal(string message) {
        if (message.Length > maxLength) return;

        // it's local chat, so let's send it to all observers via ClientRpc
        RpcMsgLocal(name, message);
    }

    [Command(channel=Channels.DefaultUnreliable)] // unimportant => unreliable
    void CmdMsgGuild(string message) {
        if (message.Length > maxLength) return;

        // not implemented yet. let's show some kind of info message
        print("Guild Chat not implemented yet!");
    }

    [Command(channel=Channels.DefaultUnreliable)] // unimportant => unreliable
    void CmdMsgWhisper(string playerName, string message) {
        if (message.Length > maxLength) return;

        // find the player with that name (note: linq version is too ugly)
        foreach (var entry in NetworkServer.objects) {
            var chat = entry.Value.GetComponent<PlayerChat>();
            if (entry.Value.name == playerName && chat != null) {
                // receiver gets a 'from' message, sender gets a 'to' message
                chat.TargetMsgWhisperFrom(chat.connectionToClient, name, message);
                TargetMsgWhisperTo(connectionToClient, entry.Value.name, message);
                return;
            }
        }
    }

    // message handlers ////////////////////////////////////////////////////////
    [TargetRpc(channel=Channels.DefaultUnreliable)] // only send to one client
    public void TargetMsgWhisperFrom(NetworkConnection target, string sender, string message) {        
        // add message with identifierIn
        string identifier = chanWhisper.identifierIn;
        string reply = chanWhisper.command + " " + sender + " "; // whisper
        AddMessage(new MessageInfo(sender, identifier, message, reply, chanWhisper.color));
    }

    [TargetRpc(channel=Channels.DefaultUnreliable)] // only send to one client
    public void TargetMsgWhisperTo(NetworkConnection target, string receiver, string message) {
        // add message with identifierOut
        string identifier = chanWhisper.identifierOut;
        string reply = chanWhisper.command + " " + receiver + " "; // whisper
        AddMessage(new MessageInfo(receiver, identifier, message, reply, chanWhisper.color));
    }

    [ClientRpc(channel=Channels.DefaultUnreliable)] // send to observers
    public void RpcMsgLocal(string sender, string message) {
        // add message with identifierIn or Out depending on who sent it
        string identifier = sender != name ? chanLocal.identifierIn : chanLocal.identifierOut;
        string reply = chanWhisper.command + " " + sender + " "; // whisper
        AddMessage(new MessageInfo(sender, identifier, message, reply, chanLocal.color));
    }

    [TargetRpc(channel=Channels.DefaultUnreliable)] // only send to one client
    public void TargetMsgInfo(NetworkConnection target, string message) {
        AddMessage(new MessageInfo("", chanInfo.identifierIn, message, "", chanInfo.color));
    }
}
