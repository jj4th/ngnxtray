/**
 * 
 * NgnxTray.exe
 * 
 * A system tray application providing simple actions related to nginx
 * installations on Windows.  Requires .Net 3.5.
 * 
 * Installation:
 *      Place in the directory where you installed nginx, and double click to
 *      execute.  Click the system tray icon to start, stop, or restart nginx.
 *      Editing the conf/nginx.conf file is also supported.  Notepad.exe is
 *      used as the conf file editor.
 * 
 * Version:
 *      1.0.0.0
 *      
 * Revision Log:
 *      2014-08-10: 1.0.0.0 - Initial Release
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 John J. Andrichak IV
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace NgnxTray
{
    // Instead of creating a form, run an application context.
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NgnxTrayCtx());
        }
    }

    public class NgnxTrayCtx : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private string location;

        public NgnxTrayCtx()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = ngnxtray.Properties.Resources.nginx,
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true
            };
            // Add Tray Icon Items
            trayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Start Nginx", null, NginxStart),
                new ToolStripMenuItem("Stop Nginx", null, NginxStop),
                new ToolStripMenuItem("Restart Nginx", null, NginxRestart),
                new ToolStripMenuItem("Edit Nginx Conf", null,  NginxConf),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit", null, Exit)
            });
            trayIcon.MouseClick += trayIcon_MouseUp;

            // Use reflection to determine the path, this is where we'll expect Nginx to be.
            location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        // Respond to a left click also.
        private void trayIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo cmd = typeof(NotifyIcon).GetMethod("ShowContextMenu",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                cmd.Invoke(trayIcon, null);
            }
        }

        // Event handlers for the different menu options.
        void NginxStart(object sender, EventArgs e)
        {
            var path = Path.Combine(location, "nginx.exe");
            Process.Start(path);
        }

        void NginxStop(object sender, EventArgs e)
        {
            var path = Path.Combine(location, "nginx.exe");
            ProcessStartInfo cmd = new ProcessStartInfo(path, "-s stop");
            Process.Start(cmd);
        }

        void NginxRestart(object sender, EventArgs e)
        {
            var path = Path.Combine(location, "nginx.exe");
            ProcessStartInfo cmd = new ProcessStartInfo(path, "-s reload");
            Process.Start(cmd);
        }

        void NginxConf(object sender, EventArgs e)
        {
            var conf = Path.Combine(location, "conf\\nginx.conf");
            ProcessStartInfo cmd = new ProcessStartInfo("notepad.exe", conf);
            Process.Start(cmd);
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}
