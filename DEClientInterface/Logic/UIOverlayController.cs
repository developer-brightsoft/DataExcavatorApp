// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Logic.UIOverlayController
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DEClientInterface.Logic
{
	public class UIOverlayController
	{
		private bool MainWindowOverlayHidingPrevented = false;

		private Window actualParentWindow { get; set; }

		private Window overlayChildWindow { get; set; }

		private Border overlayShadow { get; set; }

		public UIOverlayController(Window actualWindow, Border overlayShadow)
		{
			actualParentWindow = actualWindow;
			this.overlayShadow = overlayShadow;
			actualParentWindow.Closing += MainWindow_Closing1;
			actualParentWindow.Activated += MainWindow_Activated;
		}

		public void ShowOverlay(Window showedModalLink)
		{
			overlayShadow.Visibility = Visibility.Visible;
			overlayChildWindow = showedModalLink;
			overlayChildWindow.StateChanged += OverlayShowedWindow_StateChanged;
			overlayChildWindow.Activated += OverlayShowedWindow_Activated;
		}

		public void HideOverlay()
		{
			if (MainWindowOverlayHidingPrevented)
			{
				MainWindowOverlayHidingPrevented = false;
				return;
			}
			if (overlayShadow != null)
			{
				overlayShadow.Visibility = Visibility.Hidden;
			}
			if (overlayChildWindow != null)
			{
				overlayChildWindow.StateChanged -= OverlayShowedWindow_StateChanged;
				overlayChildWindow.Activated -= OverlayShowedWindow_Activated;
				overlayChildWindow = null;
			}
			if (actualParentWindow != null)
			{
				actualParentWindow.Activate();
			}
		}

		public void PreventOverlayHiding()
		{
			MainWindowOverlayHidingPrevented = true;
		}

		private void MainWindow_Activated(object sender, EventArgs e)
		{
			if (overlayChildWindow == null)
			{
				return;
			}
			if (overlayChildWindow.WindowState == WindowState.Minimized)
			{
				Task.Run(delegate
				{
					Thread.Sleep(250);
					actualParentWindow.Dispatcher.Invoke(delegate
					{
						overlayChildWindow.WindowState = WindowState.Normal;
						overlayChildWindow.Activate();
						overlayChildWindow.Focus();
					});
				});
			}
			else
			{
				overlayChildWindow.Activate();
				overlayChildWindow.Focus();
			}
		}

		private void MainWindow_Closing1(object sender, CancelEventArgs e)
		{
			actualParentWindow.Closing -= MainWindow_Closing1;
			actualParentWindow.Activated -= MainWindow_Activated;
			if (overlayChildWindow != null)
			{
				overlayChildWindow.Close();
				overlayChildWindow = null;
			}
		}

		private void OverlayShowedWindow_Activated(object sender, EventArgs e)
		{
			if (actualParentWindow.Visibility != 0)
			{
				actualParentWindow.Activate();
			}
			else if (actualParentWindow.WindowState == WindowState.Minimized)
			{
				actualParentWindow.WindowState = WindowState.Normal;
			}
		}

		private void OverlayShowedWindow_StateChanged(object sender, EventArgs e)
		{
			if (overlayChildWindow.WindowState == WindowState.Minimized)
			{
				actualParentWindow.WindowState = WindowState.Minimized;
			}
			if (overlayChildWindow.WindowState == WindowState.Normal && actualParentWindow.WindowState == WindowState.Minimized)
			{
				actualParentWindow.WindowState = WindowState.Normal;
			}
		}
	}
}