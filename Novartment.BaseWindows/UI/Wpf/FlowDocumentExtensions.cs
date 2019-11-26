using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using Novartment.Base.Collections;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Методы расширения для FlowDocument.
	/// </summary>
	public static class FlowDocumentExtensions
	{
		private const string UrlRegex = @"(?<Protocol>\w+):\/\/(?<Domain>[\w@][\w.:@]+)\/?[\w\.?=%&=\-@/$,]*";

		/// <summary>
		/// Преобразует части текста, являющиейся ссылками в работающие ссылки.
		/// </summary>
		/// <param name="document">Документ, в котором надо преобразовать ссылки.</param>
		public static void ConvertFlowDocumentUrlsToLinks (this FlowDocument document)
		{
			if (document == null)
			{
				throw new ArgumentNullException (nameof (document));
			}

			Contract.EndContractBlock ();

			var links = GetLinksInDocument (document);
			foreach (var item in links)
			{
				var link = new Hyperlink (item.Start, item.Finish)
				{
					NavigateUri = new Uri (item.Text),
					ToolTip = string.Format (CultureInfo.InvariantCulture, Resources.OpenLinkTooltip, item.Text),
				};
				link.Click += HyperLinkClickHandler;
			}
		}

		private static void HyperLinkClickHandler (object sender, RoutedEventArgs e)
		{
			var uri = (sender as Hyperlink)?.NavigateUri;
			if (uri != null)
			{
				Process.Start (uri.ToString ());
			}
		}

		private static IReadOnlyList<LinkData> GetLinksInDocument (FlowDocument document)
		{
			var result = new ArrayList<LinkData> ();
			var regex = new Regex (UrlRegex);
			foreach (var run in new BlockCollectionRunsIterator (document.Blocks))
			{
				var tr = new TextRange (run.ContentStart, run.ContentEnd);
				var match = regex.Match (tr.Text);
				if (match.Success)
				{
					var startPointer = run.ContentStart.GetPositionAtOffset (match.Index);
					var endPointer = startPointer.GetPositionAtOffset (match.Index + match.Length);
					var url = match.Value;
					var linkData = new LinkData (startPointer, endPointer, url);
					result.Add (linkData);
				}
			}

			return result;
		}

		internal readonly struct LinkData
		{
			internal TextPointer Start { get; }
			internal TextPointer Finish { get; }
			internal string Text { get; }

			public LinkData (TextPointer start, TextPointer finish, string text)
			{
				this.Start = start;
				this.Finish = finish;
				this.Text = text;
			}
		}
	}
}
