﻿using System;
using Newtonsoft.Json;
using NLog;
using UniversalDashboard.Models;
using System.Management.Automation;

namespace UniversalDashboard.Cmdlets
{
	[Cmdlet(VerbsCommon.New, "UDPage")]
    public class NewPageCommand : CallbackCmdlet
    {
		private readonly Logger Log = LogManager.GetLogger(nameof(NewPageCommand));

		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "static")]
		public string Name { get; set; }
	    [Parameter(Position = 1)]
		public FontAwesomeIcons Icon { get; set; }
	    [Parameter(Position = 2, Mandatory = true, ParameterSetName = "static")]
		public ScriptBlock Content { get; set; }
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "dynamic")]
		public string Url { get; set; }

		protected override void EndProcessing()
		{
			var page = new Page();
			page.Name = Name;
			page.Url = Url;
			page.Icon = FontAwesomeIconsExtensions.GetIconName(Icon);
			page.Id = Id;
			page.AutoRefresh = AutoRefresh;
			page.RefreshInterval = RefreshInterval;

			try
			{
				if (ParameterSetName == "dynamic")
				{
                    if (!Url.StartsWith("/"))
                    {
                        Url = "/" + Url;
                    }

					page.Callback = GenerateCallback(Id);
				}
				else
				{
					var components = Content.Invoke();

					foreach (var component in components)
					{
						var dashboardComponent = component.BaseObject as Component;
						if (dashboardComponent != null)
						{
							page.Components.Add(dashboardComponent);
						}
					}
				}
			}
			catch (Exception ex)
			{
				WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.SyntaxError, page));

				page.Error = new Error
				{
					Message = ex.Message,
					Location = this.MyInvocation.PositionMessage
				};
			}

			Log.Debug(JsonConvert.SerializeObject(page));

			WriteObject(page);
		}
	}
}
