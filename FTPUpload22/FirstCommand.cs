using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace VSIXProject1
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FirstCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("9f67272d-2151-459e-a56c-6ad534deb95e");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private FirstCommand ( AsyncPackage package , OleMenuCommandService commandService )
        {
            this.package = package ?? throw new ArgumentNullException ( nameof ( package ) );
            commandService = commandService ?? throw new ArgumentNullException ( nameof ( commandService ) );

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand ( menuItem );
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FirstCommand Instance
        {
            get;
            private set;
        }

        private string GetActiveFilePath ( Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider )
        {
            Task<object> t = serviceProvider.GetServiceAsync ( typeof ( EnvDTE.DTE ) );
            t.Wait ( );
            EnvDTE80.DTE2 applicationObject = t.Result as EnvDTE80.DTE2;


            foreach ( EnvDTE.SelectedItem selectedItem in applicationObject.SelectedItems )
            {
                if ( selectedItem.ProjectItem == null ) return null;
                var projectItem = selectedItem.ProjectItem;
                var fullPathProperty = projectItem.Properties.Item ( "FullPath" );
                if ( fullPathProperty == null ) return null;
                var fullPath = fullPathProperty.Value.ToString ( );
                return fullPath;
            }

            return null;

        }


        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync ( AsyncPackage package )
        {
            // Switch to the main thread - the call to AddCommand in FirstCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync ( package.DisposalToken );

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FirstCommand ( package , commandService );
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute ( object sender , EventArgs e )
        {
            ThreadHelper.ThrowIfNotOnUIThread ( );
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "FirstCommand";

            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox (
            //    this.package ,
            //    message ,
            //    title ,
            //    OLEMSGICON.OLEMSGICON_INFO ,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK ,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST );


            string file = GetActiveFilePath ( ServiceProvider );



            System.IO.FileInfo fi = new System.IO.FileInfo ( file );

            System.IO.DirectoryInfo d = fi.Directory;

            int back = 0;
            while ( true )
            {

                try
                {
                    if ( d == null ) break;
                    if ( System.IO.File.Exists ( d.FullName + System.IO.Path.DirectorySeparatorChar + "ftp.ext.settings" ) )
                    {
                        string [ ] bits = System.IO.File.ReadAllLines ( d.FullName + System.IO.Path.DirectorySeparatorChar + "ftp.ext.settings" );
                        string server = bits [ 0 ];
                        string username = bits [ 1 ];
                        string password = bits [ 2 ];
                        string baseFolder = bits [ 3 ];


                        string [ ] fileBits = file.Split ( System.IO.Path.DirectorySeparatorChar );

                        string remoteFolder = baseFolder + "/" + string.Join ( "/" , fileBits.Skip ( fileBits.Length - ( back + 1 ) ) );
                        remoteFolder = string.Join ( "/" , remoteFolder.Split ( '/' ).Take ( remoteFolder.Split ( '/' ).Length - 1 ).ToArray ( ) ).Replace ( System.IO.Path.DirectorySeparatorChar.ToString ( ) , "/" ).Replace ( "//" , "/" ) + "/";


                        ProcessStartInfo pInfo = new ProcessStartInfo ( );
                        pInfo.FileName = @"c:\windows\ncftpput.exe";
                        pInfo.Arguments = "-u " + username + " -p \"" + password + "\" -m " + server + " " + remoteFolder + " " + file;
                        pInfo.CreateNoWindow = true;
                        Process p = Process.Start ( pInfo );

                        p.WaitForExit ( );
                        if ( p.ExitCode == 0 )
                        {


                            VsShellUtilities.ShowMessageBox (
                   this.package ,
                   "Sent " + new System.IO.FileInfo ( file ).Name + " to " + server + " " + remoteFolder ,
                   "FTP Success" ,
                   OLEMSGICON.OLEMSGICON_INFO ,
                   OLEMSGBUTTON.OLEMSGBUTTON_OK ,
                   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST );
                        }
                        else
                        {
                            VsShellUtilities.ShowMessageBox (
                   this.package ,
                   "Problem " + new System.IO.FileInfo ( file ).Name + " to " + server + " " + remoteFolder ,
                   "FTP Problem" ,
                   OLEMSGICON.OLEMSGICON_WARNING ,
                   OLEMSGBUTTON.OLEMSGBUTTON_OK ,
                   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST );

                        }

                        break;
                        // now send the back boy

                    }
                    d = d.Parent;
                }
                catch ( Exception ex )
                {
                    VsShellUtilities.ShowMessageBox (
                  this.package , ex.Message , "ERROR" ,
                   OLEMSGICON.OLEMSGICON_WARNING ,
                   OLEMSGBUTTON.OLEMSGBUTTON_OK ,
                   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST );

                    break;
                }
                back += 1;

            }
        }
    }
}
