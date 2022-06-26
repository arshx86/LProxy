#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leaf.xNet;
using LProxy.Properties;
using LProxy.Util;

#endregion

namespace LProxy
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Reload();
            settingsPanel.Location = new Point(0, 135);
            shieldPanel.Location = new Point(0, 135);
        }

        private void Reload()
        {
            // check if proxy enabled
            string host = "Unkown";
            bool flag = Server.IsEnabled(out host);
            clear.Enabled = Proxies.Count > 0 || Server.IsEnabled(out _);
            if (flag)
            {
                ProxyEnabled = true;
                var query = Utils.QueryProxy(host.Split(':')[0]);
                proxylevel.Image = Resources.level_transparent;
                tooltip.SetToolTip(ipaddr, host);
                tooltip.SetToolTip(address, $"{query.Country}/{query.City}");
                tooltip.SetToolTip(proxylevel,
                    "We cannot determine the proxy level from proxy you're connected already.");
                tooltip.SetToolTip(status, "Connected");
                tooltip.SetToolTip(connect, $"Disconnect from {ToBeConnected}");

                status.Image = Resources.icons8_connected_2; // not connected
                status.Text = "Connected";
                ConnectedHost = host.Split(':').First();
                ConnectedPort = host.Split(':').Last();
                connect.Text = "Disconnect";
                connect.Enabled = true;
            }
            else
            {
                ProxyEnabled = false;
                proxylevel.Image = Resources.icons8_question_mark;
                tooltip.SetToolTip(ipaddr, "You aren't connected");
                tooltip.SetToolTip(address, "You aren't connected.");
                tooltip.SetToolTip(proxylevel, "You aren't connected.");
                tooltip.SetToolTip(status, "Not connected.");
                tooltip.SetToolTip(connect, $"Connect to {ToBeConnected}");

                status.Image = Resources.icons8_connected; // not connected
                status.Text = "Not connected";
                connect.Enabled = false;
                connect.Text = "Choose Proxy";
            }
        }

        private void connect_Click(object sender, EventArgs e)
        {
            // Enable
            if (!ProxyEnabled)
            {
                Utils.SendToast("Connected", "Connected to " + ToBeConnected);

                #region Controls

                connect.Enabled = false;
                connect.Text = "Connecting...";
                Server.SetStatus(ToBeConnected, true);
                var pType = Proxies.FirstOrDefault(x => x.Port == ToBeConnected.Split(':').Last());
                switch (pType.Type)
                {
                    case "elite":
                        proxylevel.Image = Resources.level_elite;
                        break;
                    case "anonymous":
                        proxylevel.Image = Resources.level_anonymous;
                        break;
                    case "transparent":
                        proxylevel.Image = Resources.level_transparent;
                        break;
                    default:
                        proxylevel.Image = Resources.icons8_question_mark;
                        break;
                }

                connect.Enabled = true;
                connect.Text = "Disconnect";
                ProxyEnabled = true;
                connect.BorderColor = Color.Indigo;

                #endregion
            }
            // Disable
            else
            {
                Utils.SendToast("Disconnected", "Disconnected from " + ToBeConnected);
                string t = "you really think you're gonna do it!??";

                #region Controls

                connect.Enabled = false;
                connect.Text = "Disconnecting...";
                Server.SetStatus("", false);
                proxylevel.Image = Resources.icons8_question_mark;
                connect.Enabled = true;
                connect.Text = "Connect";
                ProxyEnabled = true;
                connect.BorderColor = Color.Indigo;

                #endregion
            }

            Reload();
        }

        private void ipaddr_Click(object sender, EventArgs e)
        {
            if (!ProxyEnabled)
            {
                MessageBox.Show("You haven't connected to any proxy server.", "Not Connected", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show($"You are connected to {ConnectedHost} with port {ConnectedPort}", "Connected",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void scrapebtn_Click(object sender, EventArgs e)
        {
            if (!chkTransparent.Checked && !chkAnonymous.Checked && !chkElite.Checked)
            {
                MessageBox.Show("You must select at least one option.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            scrapebtn.Enabled = false;
            scrapebtn.Text = "Scraping...";

            string anonymityNode;
            if (chkTransparent.Checked)
                anonymityNode = "transparent";
            else if (chkElite.Checked)
                anonymityNode = "elite";
            else if (chkAnonymous.Checked)
                anonymityNode = "anonymous";
            else
                anonymityNode = "unkown";

            // HTTP [0]
            // SOCKS4 [1]
            // SOCKS5 [2]

            string typeNode;
            switch (type.SelectedIndex)
            {
                case 0:
                    typeNode = "http";
                    break;
                case 1:
                    typeNode = "socks4";
                    break;
                case 2:
                    typeNode = "socks5";
                    break;
                default:
                    typeNode = "all";
                    break;
            }

            string uri = "https://api.proxyscrape.com/v2/?request=displayproxies" +
                         "&protocol=" +
                         typeNode +
                         "&timeout=10000&country=all&ssl=all" +
                         "&anonymity=" +
                         anonymityNode;

            int howMuch;
            using (var client = new HttpRequest())
            {
                var req = client.Get(uri);
                var res = req.ToString();
                var data = res.Split('\n');
                howMuch = data.Length;
                // add all proxies to the proxybox list

                int i = proxybox.Items.Count;
                foreach (var item in data)
                    if (item.Length > 0)
                    {
                        string[] proxy = item.Split(':');
                        string hold = proxy[0] + ":" + proxy[1] +
                                      $" [{typeNode}]";
                        proxybox.Items.Add(hold);
                        Proxies.Add(new ProxyType
                        {
                            Host = proxy[0],
                            Port = proxy[1],
                            AnonLevel = anonymityNode,
                            Type = typeNode,
                            Sorted = hold,
                            Index = i
                        });
                        i++;
                    }
            }

            scrapebtn.Enabled = true;
            scrapebtn.Text = "";
            Reload();
            MessageBox.Show($"Scraped {howMuch} | {typeNode} proxy.", "Scrape Result", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void proxybox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var SelectedProxy = Proxies.FirstOrDefault(x => x.Index == proxybox.SelectedIndex);
            connect.Enabled = true;
            connect.Text = "Connect";
            ToBeConnected = SelectedProxy.Host + ":" + SelectedProxy.Port;

            switch (SelectedProxy.Type)
            {
                case "http":
                    typeIndicator.Image = Resources.icons8_secure;
                    tooltip.SetToolTip(typeIndicator, "HTTP Proxy");
                    break;
                case "socks4":
                    typeIndicator.Image = Resources.icons8_socks;
                    tooltip.SetToolTip(typeIndicator, "SOCKS4 Proxy");
                    break;
                case "socks5":
                    typeIndicator.Image = Resources.icons8_socks;
                    tooltip.SetToolTip(typeIndicator, "SOCKS5 Proxy");
                    break;
                default:
                    typeIndicator.Image = Resources.icons8_question_mark;
                    tooltip.SetToolTip(typeIndicator, "Custom");
                    break;
            }

            switch (SelectedProxy.AnonLevel)
            {
                case "elite":
                    levelIndicator.Image = Resources.level_elite;
                    proxylevel.Image = Resources.level_elite;
                    tooltip.SetToolTip(levelIndicator, "Elite Proxy");
                    tooltip.SetToolTip(proxylevel, "Elite Level");
                    break;
                case "anonymous":
                    levelIndicator.Image = Resources.level_anonymous;
                    proxylevel.Image = Resources.level_anonymous;
                    tooltip.SetToolTip(levelIndicator, "Anonymous Proxy");
                    tooltip.SetToolTip(proxylevel, "Anonymous Level");
                    break;
                case "transparent":
                    levelIndicator.Image = Resources.level_transparent;
                    proxylevel.Image = Resources.level_transparent;
                    tooltip.SetToolTip(levelIndicator, "Transparent Proxy");
                    tooltip.SetToolTip(proxylevel, "Transparent Level");
                    break;
                default:
                    levelIndicator.Image = Resources.icons8_question_mark;
                    proxylevel.Image = Resources.icons8_question_mark;
                    tooltip.SetToolTip(levelIndicator, "Custom Type");
                    tooltip.SetToolTip(proxylevel, "Custom Level");
                    break;
            }

            try
            {
                var query = Utils.QueryProxy(SelectedProxy.Host);
                if (query.Status == "success") tooltip.SetToolTip(addressIndicator, $"{query.Country}/{query.City}");
            }
            catch
            {
                tooltip.SetToolTip(addressIndicator, "Unknown");
            }

            tooltip.SetToolTip(ipaddr, ToBeConnected);
            tooltip.SetToolTip(address, addressIndicator.Text);
            tooltip.SetToolTip(connect, "Connect to " + ToBeConnected);
        }

        private void loadfromFile_Click(object sender, EventArgs e)
        {
            // create fileDialog
            var fileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Select a text file",
                CheckFileExists = true
            };
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                // read file
                var file = fileDialog.OpenFile();
                var reader = new StreamReader(file);
                var data = reader.ReadToEnd();
                // split lines
                var lines = data.Split('\n');
                // add all proxies to the proxybox list

                if (lines.All(x => x.Split(':').Length != 2))
                {
                    MessageBox.Show(
                        "The file you choosed doesn't contain any proxy. Proxies must formatted as HOST:PORT",
                        "Are you sure it's correct file?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int i = proxybox.Items.Count;
                foreach (var item in lines)
                    if (item.Length > 0)
                    {
                        string[] proxy = item.Split(':');
                        string hold = proxy[0] + ":" + proxy[1] +
                                      " [custom]";
                        proxybox.Items.Add(hold);
                        Proxies.Add(new ProxyType
                        {
                            Host = proxy[0],
                            Port = proxy[1],
                            AnonLevel = "custom",
                            Type = "custom",
                            Sorted = hold,
                            Index = i
                        });
                        i++;
                    }

                MessageBox.Show($"{i} proxy loaded successfully.", "File Load", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/arshx86");
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            // dispose current proxy
            if (Server.IsEnabled(out _))
            {
                var ds = MessageBox.Show("Clearing proxies will also dispose current proxy. Continue?", "Clear",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ds != DialogResult.Yes) return;
            }

            proxybox.SelectedIndex = 0;
            proxybox.Update();
            proxybox.Items.Clear();
            Server.SetStatus("", false);
            Proxies.Clear();
            Reload();
            clear.Image = Resources.icons8_ok;
            // wait for a while then revert
            Task.Delay(2500).ContinueWith(t => { clear.Image = Resources.icons8_broom; });
        }

        #region Static

        private struct ProxyType
        {
            public string Type;
            public string AnonLevel;
            public string Host;
            public string Port;
            public string Sorted; // hold items like 192.168.1.1:8080 HTTP aNONYMOUS
            public int Index;
        }

        private static bool ProxyEnabled;
        private static readonly List<ProxyType> Proxies = new List<ProxyType>();
        private static string ToBeConnected;
        private static string ConnectedHost;
        private static string ConnectedPort;

        #endregion

        #region Other Events

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x84)
                m.Result = (IntPtr)0x2;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            settingsPanel.Visible = true;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            settingsPanel.Visible = false;
        }

        private void guna2Panel5_Paint(object sender, PaintEventArgs e)
        {
        }

        private void type_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        private void levelIndicator_Click(object sender, EventArgs e)
        {
        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {
        }

        #endregion
    }
}