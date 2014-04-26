using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RSAS.Networking;
using RSAS.Networking.Messages;
using RSAS.Utilities;
using RSAS.Plugins.Frameworks;
using RSAS.Logging;

namespace RSAS.ServerSide
{
    class UserAuthenticator
    {
        static List<Connection> unauthenticatedConnections = new List<Connection>();
        static Dictionary<string, User> users = new Dictionary<string, User>();

        public static void LoadCredentials(string userDirectory)
        {
            if (Directory.Exists(userDirectory))
            {
                foreach (string usernamePath in Directory.GetDirectories(userDirectory))
                {
                    string username = Path.GetFileName(usernamePath);
                    string pathToUserPasswordHash = Settings.BuildUserCredentialsFilePath(username);
                    if (File.Exists(pathToUserPasswordHash))
                    {
                        string hash = File.ReadAllText(pathToUserPasswordHash);
                        User user = new User(username, hash);
                        users.Add(username, user);
                    }
                    else
                    {
                        throw new FileNotFoundException("The file '" + Settings.USERCREDENTIALSFILENAME + "' was not found for the user '" + username + "'");
                    }
                }
            }
            else
            {
                throw new DirectoryNotFoundException("The directory containing user definitions was not found. Ensure the 'users' directory exists at " + Settings.USERPATH);
            }
        }

        public static void CreateCredentials(string userDirectory, string username, string password)
        {
            if (Directory.Exists(userDirectory))
            {
                if (!Directory.Exists(Settings.BuildUserPath(username)))
                {
                    Directory.CreateDirectory(Settings.BuildUserPath(username));
                    Directory.CreateDirectory(Settings.BuildUserPluginPath(username));

                    string hash = SecurityUtilities.PBKDF2(password);

                    StreamWriter sw = new StreamWriter(Settings.BuildUserCredentialsFilePath(username));
                    sw.Write(hash);
                    sw.Close();

                    User user = new User(username, hash);
                    users.Add(username, user);
                }
            }
            else
            {
                throw new DirectoryNotFoundException("The directory containing user definitions was not found. Ensure the 'users' directory exists at " + Settings.USERPATH);
            }
        }

        public static void DeleteCredentials(string userDirectory, string username)
        {
            if (Directory.Exists(userDirectory))
            {
                if (Directory.Exists(Settings.BuildUserPath(username)))
                {
                    List<string> dirs = Directory.EnumerateDirectories(Settings.BuildUserPluginPath(username)).ToList<string>();

                    if (dirs.Count > 0)
                        throw new UserDirectoryNotEmptyException("The plugin directory for user '" + username + "' is not empty");
                    else
                        Directory.Delete(Settings.BuildUserPath(username), true);
                }
            }
            else
            {
                throw new DirectoryNotFoundException("The directory containing user definitions was not found. Ensure the 'users' directory exists at " + Settings.USERPATH);
            }
        }

        public static void ModifyCredentials(string userDirectory, string username, string password)
        {
            if (Directory.Exists(userDirectory))
            {
                if (Directory.Exists(Settings.BuildUserPath(username)))
                {
                    string hash = SecurityUtilities.PBKDF2(password);

                    StreamWriter sw = new StreamWriter(Settings.BuildUserCredentialsFilePath(username));
                    sw.Write(hash);
                    sw.Close();

                    User user = new User(username, hash);

                    if (users.ContainsKey(username))
                        users[username] = user;
                    else
                        users.Add(username, user);
                }
                else
                {
                    throw new DirectoryNotFoundException("The directory for the user '" + username + "' could not be found");
                }
            }
            else
            {
                throw new DirectoryNotFoundException("The directory containing user definitions was not found. Ensure the 'users' directory exists at " + Settings.USERPATH);
            }
        }

        static bool CheckCredentials(string username, string password)
        {
            if (users.ContainsKey(username))
            {
                try
                {
                    return (users[username].AuthenticationKey == SecurityUtilities.PBKDF2(password, SecurityUtilities.SaltFromKey(users[username].AuthenticationKey)));
                }
                catch (ArgumentException e)
                {
                    TextLogger.TimestampedLog(LogType.Information, "User " + username + " attempted to apply an invalid key, possibly indicating an attack: " + e.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static void AuthenticateConnection(object sender, ServerClientConnectedEventArgs e)
        {
            Connection con = e.Connection;
            UserAuthenticator.unauthenticatedConnections.Add(con);
            con.MessageReceived += CheckAuthenticationResponse;
            con.SendMessage(new AuthenticationRequest());
            
        }

        static void CheckAuthenticationResponse(object sender, ConnectionMessageReceivedEventArgs e)
        {
            Connection con = sender as Connection;
            if (e.Message.GetType() == typeof(AuthenticationResponse))
            {
                AuthenticationResponse message = e.Message as AuthenticationResponse;
                if (CheckCredentials(message.Username, message.Password))
                {
                    //unhook from future messages, work here is done
                    con.MessageReceived -= CheckAuthenticationResponse;
                    unauthenticatedConnections.Remove(con);

                    //let the client know the result was successful
                    con.SendMessage(new AuthenticationResult(true));

                    //associate new connection with user instance
                    users[message.Username].AssociateWithConnection(con);
                }
                else
                {
                    //let the client know the result was unsuccessful
                    con.SendMessage(new AuthenticationResult(false));

                    //disconnect
                    con.Disconnect();
                }
            }
        }
    }
}
