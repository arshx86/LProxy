#region

using System.Windows.Forms;
using Leaf.xNet;
using LProxy.Properties;
using Newtonsoft.Json;

#endregion

namespace LProxy.Util
{
    internal class Utils
    {
        /// <summary>
        ///     Send toast notification.
        /// </summary>
        /// <param name="Title">Title of toast.</param>
        /// <param name="Description">Description of toast.</param>
        public static void SendToast(string Title, string Description)
        {
            NotifyIcon ni = new NotifyIcon
            {
                BalloonTipIcon = ToolTipIcon.Info,
                Icon = Resources.icons8_anonymous_mask,
                BalloonTipTitle = Title,
                BalloonTipText = Description
            };
            ni.Visible = true;
            ni.ShowBalloonTip(3000);
        }

        /// <summary>
        ///     Queries an IP/Proxy to retrieve information about.
        /// </summary>
        /// <param name="Host">IP or Proxy to check.</param>
        /// <returns>Result class of query.</returns>
        public static IPResult QueryProxy(string Host)
        {
            using (var client = new HttpRequest())
            {
                var response = client.Get($"http://ip-api.com/json/{Host}");
                var json = response.ToString();
                var result = JsonConvert.DeserializeObject<IPResult>(json);
                return result;
            }
        }
    }
}