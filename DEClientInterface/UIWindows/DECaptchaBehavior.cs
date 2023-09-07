// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.UIWindows.DECaptchaBehavior
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using DEClientInterface;
using DEClientInterface.Logic;
using DEClientInterface.UIExtensions;
using ExcavatorSharp.Captcha;

namespace DEClientInterface.UIWindows
{
	public class DECaptchaBehavior : Window, IComponentConnector
	{
		internal ComboBox DECaptchaTypeSelectBox;

		internal Button HelpButton_CaptchaType;

		internal TextBox DECaptchaDetectionSubstring_TB;

		internal Button HelpButton_CaptchaDetectionSubstring;

		internal TextBox DECaptchaWaitAfterAndBeforeResolving;

		internal Button HelpButton_WaitAfterAndBeforeResolvingButton;

		internal Button DeleteCAPTCHASettings;

		internal Button SaveCAPTCHASettings;

		private bool _contentLoaded;

		private DEProjectCubeProperties ParentWindowLink { get; set; }

		private CAPTCHASolverSettings CaptchaSettingsLink { get; set; }

		private UIOverlayController overlayController { get; set; }

		private Window helpWindow { get; set; }

		public DECaptchaBehavior(DEProjectCubeProperties ParentWindowLink, CAPTCHASolverSettings CaptchaSettingsLink = null)
		{
			this.ParentWindowLink = ParentWindowLink;
			this.CaptchaSettingsLink = CaptchaSettingsLink;
			InitializeComponent();
			if (CaptchaSettingsLink != null)
			{
				DECaptchaDetectionSubstring_TB.Text = this.CaptchaSettingsLink.ForceSpecifiedCaptchaDetectionSubstring;
				/*DECaptchaWaitAfterAndBeforeResolving.Text = this.CaptchaSettingsLink.WaitAfterAndBeforeResolving.ToString();*/
			}
		}

		private void SaveCAPTCHASettings_Click(object sender, RoutedEventArgs e)
		{
			bool flag = false;
			DECaptchaDetectionSubstring_TB.MarkAsCorrectlyCompleted();
			string text = DECaptchaDetectionSubstring_TB.Text.Trim();
			if (text.Length == 0)
			{
				DECaptchaDetectionSubstring_TB.MarkAsUncorrectlyCompleted();
				flag = true;
			}
			DECaptchaWaitAfterAndBeforeResolving.MarkAsCorrectlyCompleted();
			string text2 = DECaptchaWaitAfterAndBeforeResolving.Text.Trim();
			int result = 0;
			if (text2 != string.Empty && !int.TryParse(text2, out result))
			{
				DECaptchaWaitAfterAndBeforeResolving.MarkAsUncorrectlyCompleted();
				flag = true;
			}
			if (!flag)
			{
				/*CAPTCHASolverSettings actualCAPTCHAResolverSettings = new CAPTCHASolverSettings(CaptchaSolvingService.ManualCaptchaSolver, CaptchaType.UnknownCaptchaAutoDetect, text, 5, 2, 400, string.Empty, new Dictionary<string, string>(), result);
				ParentWindowLink.DETaskCrawlingServer_CEFCaptchaBehavior_TextBlock.Text = "CAPTCHA resolver specified";
				ParentWindowLink.ActualCAPTCHAResolverSettings = actualCAPTCHAResolverSettings;*/
				Close();
			}
		}

		private void DeleteCAPTCHASettings_Click(object sender, RoutedEventArgs e)
		{
			ParentWindowLink.ActualCAPTCHAResolverSettings = null;
			ParentWindowLink.DETaskCrawlingServer_CEFCaptchaBehavior_TextBlock.Text = string.Empty;
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			if (!_contentLoaded)
			{
				_contentLoaded = true;
				Uri resourceLocator = new Uri("/DEClientInterface;component/uiwindows/decaptcharesolversettings.xaml", UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				DECaptchaTypeSelectBox = (ComboBox)target;
				break;
			case 2:
				HelpButton_CaptchaType = (Button)target;
				break;
			case 3:
				DECaptchaDetectionSubstring_TB = (TextBox)target;
				break;
			case 4:
				HelpButton_CaptchaDetectionSubstring = (Button)target;
				break;
			case 5:
				DECaptchaWaitAfterAndBeforeResolving = (TextBox)target;
				break;
			case 6:
				HelpButton_WaitAfterAndBeforeResolvingButton = (Button)target;
				break;
			case 7:
				DeleteCAPTCHASettings = (Button)target;
				DeleteCAPTCHASettings.Click += DeleteCAPTCHASettings_Click;
				break;
			case 8:
				SaveCAPTCHASettings = (Button)target;
				SaveCAPTCHASettings.Click += SaveCAPTCHASettings_Click;
				break;
			default:
				_contentLoaded = true;
				break;
			}
		}
	}
}