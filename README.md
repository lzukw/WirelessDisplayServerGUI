# WirelessDisplayServerGUI

The purpose of this program is to run the program 'WirelessDisplayServer'
in background. A GUI is displayed, that shows

- the IP-Adress of this Computer running WirelessDispalyServer, and
- the log-ouput of the WirelessDisplayServer

## Configuration

During development configuration is stored in the file App.config. After
publishing the program with `dotnet publish` this configuration is stored
in the file `WirelessDisplayServerGUI.dll.config`, and can be edited there.

The necessary confifuration consists only of: 

- A relative path to the the executable version of WirelessDisplayServer

## Running the program

Just use `dotnet run`.

## Creating an executable

From within the folder `WirelessDisplayServerGUI` run the following commands:

```
mkdir ..\WirelessDisplayServerGUI_executable
dotnet publish -c Release -o ..\WirelessDisplayServerGUI_executable -r win-x64 --self-contained
```

The paremter `--self-contained` creates a 'stand-alone' executable version. This 
paremeter can be omitted, if .NET-Core version 3.1 is installed on the target system.
All necessary files are put in the directory `WirelessDisplayServerGUI_executable`.
The executable to start is called `WirelessDisplayServerGUI.exe`. The configuration
can still be changed, by changing the contents of `WirelessDisplayServerGUI.dll.config`
(which is just the original `App.config`-file copied and renamed).

Create a link for example on the desktop, that links to the file 
`WirelessDisplayServerGUI.exe` in the folder `WirelessDisplayServerGUI_executable`. 
The program can be run by double-clicking this link. You can also create a 
start-menu-entry, by creating the link in `%AppData%\Microsoft\Windows\Start Menu\Programs`.

## Technical Details

Program created with:
```
mkdir WirelessDisplayServerGUI
cd WirelessDisplayServerGUI
dotnet new wpf
```

This creates the following files:

- `App.xaml` and `App.xaml.cs`: These files together define the class `App`.
- `MainWindow.xaml` and `MainWindow.xaml.cs`: These files define the class `MainWindow`.
- `WirelessDisplayServerGUI.csproj` and `AssemblyInfo.cs`.

From these files only `MainWindow.xaml` and `MainWindow.xaml.cs` were modified.

MainWindow.xaml contains two relecant UI-Elemnts:

- `labelIp` ...to display the local IP-Address.
- `textblockLog` ...to display the log-output or WirelessDisplayServer.

### Configuration

- Added file `App.config`, containing the key-value-pair `PathToWirelessDisplayServerExe`.
- Added `using System.Configuration;` to `MainWindow.xaml.cs`.
- Configuration parameters are read with 
  `ConfigurationManager.AppSettings["PathToWirelessDisplayServerExe"]`

### MainWindow.xaml.cs

This file contains the 'Interaction logic for `MainWindow.xaml`' by means of a
partial class `MainWindow`. (The other parts of this class are created by the
compiler from the file `MainWindow.xaml` and are only present in the 
`obj`-directory).

In `MainWindow.xaml.cs` the whole program is realized. In the Constructor of
the `MainWindow`-class the following things are done:

- The local IPv4-Address is retrieved by using static methods from 
  `System.Net.Dns`. The IPv4-Address then is displayed to the user (inserted
  into labelIp).
- Configuration is read in from `App.config` containing the filepath to the
  WirelessDisplayServer-executable. 
- Eventually still running WirelessDisplayServer-processes are killed.
- A new WirelessDisplayServer-process is created and run in background.
- Two event-handlers are registered: Each time the started 
  WirelessDisplayServer-background-process writes a line to its stdout or stderr,
  the events `OutputDataReceived` or `ErrorDataReceived` occur. Both events call
  the method `void backgroundProcess_DataReceived(object sender, DataReceivedEventArgs e)`,
  where the line written by the process to stdout/stderr is contained in `e.Data`.
  In this method, the line is appended to `textblockLog` and so displayed to
  the user.

When the user closes the MainWindow (and the whole program), the event-handler
`void mainWindow_Closing(object sender, object e)` is called. Here the
WirelessDisplayServer-background-process is killed.