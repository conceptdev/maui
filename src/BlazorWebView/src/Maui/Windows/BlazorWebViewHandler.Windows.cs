﻿using System;
using System.IO;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Handlers;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WebView2Control>
	{
		private WebView2WebViewManager? _webviewManager;

		protected override WebView2Control CreateNativeView()
		{
			return new WebView2Control();
		}

		protected override void DisconnectHandler(WebView2Control nativeView)
		{
			if (_webviewManager != null)
			{
				// Dispose this component's contents and block on completion so that user-written disposal logic and
				// Blazor disposal logic will complete first. Then call base.Dispose(), which will dispose the WebView2
				// control. This order is critical because once the WebView2 is disposed it will prevent and Blazor
				// code from working because it requires the WebView to exist.
				_webviewManager?
					.DisposeAsync()
					.AsTask()
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

				_webviewManager = null;
			}
		}

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet ||
				_webviewManager != null)
			{
				return;
			}
			if (NativeView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
			}

			var assetConfig = Services!.GetRequiredService<BlazorAssetsAssemblyConfiguration>()!;

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

			var fileProvider = new ManifestEmbeddedFileProvider(assetConfig.AssetsAssembly, root: contentRootDir);

			_webviewManager = new WinUIWebViewManager(NativeView, new WinUIWebView2Wrapper(NativeView), Services!, MauiDispatcher.Instance, fileProvider, hostPageRelativePath);
			if (RootComponents != null)
			{
				foreach (var rootComponent in RootComponents)
				{
					// Since the page isn't loaded yet, this will always complete synchronously
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}
			_webviewManager.Navigate("/");
		}
	}
}
