﻿//------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by a tool.
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// A strongly-typed resource class, for looking up localized strings, etc.
	/// </summary>
	// This class was auto-generated by the StronglyTypedResourceBuilder
	// class via a tool like ResGen or Visual Studio.
	// To add or remove a member, edit your .ResX file then rerun ResGen
	// with the /str option, or rebuild your VS project.
	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[System.Diagnostics.DebuggerNonUserCodeAttribute ()]
	[System.Runtime.CompilerServices.CompilerGeneratedAttribute ()]
	public class Resources
	{
		private static Lazy<System.Resources.ResourceManager> _resourceMan = new Lazy<System.Resources.ResourceManager> (CreateResourceManager);

		internal Resources ()
		{
		}

		/// <summary>
		/// Returns the cached ResourceManager instance used by this class.
		/// </summary>
		[System.ComponentModel.EditorBrowsable (System.ComponentModel.EditorBrowsableState.Advanced)]
		private static System.Resources.ResourceManager CreateResourceManager ()
		{
			var template = "Novartment.Base.Net.Mime.Resources.resources";
			var assembly = typeof (Resources).Assembly;
			var names = assembly.GetManifestResourceNames ();
			string foundName = null;
			foreach (var name in names)
			{
				if (name.Equals (template, StringComparison.OrdinalIgnoreCase))
				{
					foundName = name;
					break;
				}
			}

			if (foundName == null)
			{
				foreach (var name in names)
				{
					if (name.EndsWith (template, StringComparison.OrdinalIgnoreCase))
					{
						foundName = name;
						break;
					}
				}

				if (foundName == null)
				{
					var template2 = "Resources.resources";
					foreach (var name in names)
					{
						if (name.EndsWith (template2, StringComparison.OrdinalIgnoreCase))
						{
							foundName = name;
							break;
						}
					}
				}
			}

			if (foundName == null)
			{
				throw new InvalidOperationException (string.Format ("Resource with name '{0}' not found in assembly '{1}'.", template, assembly.GetName ().Name));
			}

			return new System.Resources.ResourceManager (foundName.Substring (0, foundName.Length - 10), assembly);
		}

		/// <summary>
		/// Returns the formatted resource string.
		/// </summary>
		[System.ComponentModel.EditorBrowsable (System.ComponentModel.EditorBrowsableState.Advanced)]
		private static string GetResourceString (string key)
		{
			return _resourceMan.Value.GetString (key);
		}

		///<summary>
		///3
		///</summary>
		public static string DispositionNotificationDeletedMessage => GetResourceString ("DispositionNotificationDeletedMessage");


		///<summary>
		///3
		///</summary>
		public static string DispositionNotificationDisplayedMessage => GetResourceString ("DispositionNotificationDisplayedMessage");
	}
}
