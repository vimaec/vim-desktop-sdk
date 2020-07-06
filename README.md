# VIM Desktop Application SDK

About VIM and the VIM Desktop Application SDK

The VIM Desktop Application SDK (https://github.com/vimaec/vim-desktop-sdk) is a C# API for writing plug-ins that can automate the VIM Windows Desktop application and add new functionality. 

VIM (Virtual Information Modeling) is both a platform and open file-format designed especially for the efficient interchange of BIM and 3D data into virtual environments and real-time engines running on multiple platform

The VIM Desktop Application is a WPF .NET application with an integrated high-performance rendering engine built from the ground up using the state of the art Vulkan API and is designed especially for large scale architectural projects. 

# SDK Requirements

The following are required to get started building an SDK:

* Visual Studio Community 2019 – download for free at https://visualstudio.microsoft.com/vs/community/
* VIM Desktop Application – register for a free account at https://portal.vimaec.com

# Creating VIM Plug-ins

A VIM plug-in is any class that has the `[VimPlugin]` attribute and implements the `IVimPlugin` interface from the Vim.Desktop.Api.dll assembly. VIM plug-ins are loaded from .NET assemblies whose name matches the pattern `*.Plugin.dll` and that resides in the `%userprofile%\VIM\Desktop Plugins` folder. On assembly (class library) may contain multiple plug-ins. 

## VIM Libraries

The VIM API SDK consists of most file in the VIM Desktop Application folder (e.g. C:\Program Files (x86)\VIMaec LLC\VIM\Viewer) that match the pattern `Vim.*.dll`. These are all managed libraries (aka .NET Assemblies), meaning that they are .NET Class Libraries, amd cam be used with various .NET languages. 

Here is the full list: 

* Vim.BFast.dll
*	Vim.Buffers.dll
*	Vim.DotNetUtilities.dll
*	Vim.G3d.dll
*	Vim.Geometry.dll
*	Vim.LinqArray.dll
*	Vim.Math3d.dll
* For internal use only: Vim.Collaboration.Client.dll
* For internal use only: Vim.Collaboration.Core.dll

## The IVimPlugin Interface

The VIM Desktop application communicates with the plug-in by calling functions exposed by the plug-in by implementing the `IVimPlugin` interface. These are:

* `void Init(IVimApi api)` – Called when the plug-in is initialized 
*	`void OnOpenFile(string fileName)` – Called when a new VIM file is loaded
*	`void OnCloseFile()` – Called when a VIM file is unloaded
*	`void OnFrameUpdate(float deltaTime)` – Called on each frame

The `IVimApi` is an interface provided upon initialization to the plug-in that provides a way for the plug-in to communicate with and control the Desktop application further.  

For more information on the API see the file `DesktopApi.cs`.
