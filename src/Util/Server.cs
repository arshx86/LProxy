#region

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

#endregion

namespace LProxy.Util
{
    internal class Server
    {
        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer,
            int dwBufferLength);

        /// <summary>
        ///     Sets status of proxy client on PC.
        /// </summary>
        /// <param name="Host">HOST (IP:PORT) to apply as proxy.</param>
        /// <param name="Enabled">True to enable, false to disable.</param>
        public static void SetStatus(string Host, bool Enabled)
        {
            const string key = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";

            Registry.SetValue(key, "ProxyServer", Host);
            Registry.SetValue(key, "ProxyEnable", Enabled ? 1 : 0);

            InternetSetOption(IntPtr.Zero, 0x39, IntPtr.Zero, 0); // notify change
            InternetSetOption(IntPtr.Zero, 0x37, IntPtr.Zero, 0); // refresh 
        }

        /// <summary>
        ///     Checks if proxy already enabled on PC.
        /// </summary>
        /// <param name="host">Enabled proxy, if any.</param>
        /// <returns>Whether proxy is enabled.</returns>
        public static bool IsEnabled(out string host)
        {
            host = null;
            var rk = Registry.CurrentUser;
            int _enabled = (int)Registry.GetValue(
                "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings",
                "ProxyEnable", 0);
            bool enabled = _enabled == 1;

            if (enabled)
                host = (string)Registry.GetValue(
                    "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings",
                    "ProxyServer", null);

            return enabled;
        }
    }
}