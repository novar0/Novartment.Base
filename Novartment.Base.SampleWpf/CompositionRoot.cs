using System;
using System.ServiceModel;
using Autofac;
using Autofac.Features.OwnedInstances;
using Novartment.Base.UI;
using Novartment.Base.UI.Wpf;

// ссылка на веб-сервис
using ISerivceInterface = System.ServiceModel.Channels.IOutputChannel; // тут ссылка на (сгенерированный клиентский прокси) интерфейс сервиса

namespace Novartment.Base.SampleWpf
{
	public static class CompositionRoot
	{
		private static readonly IValueHolder<IContainer> _container = new LazyValueHolder<IContainer> (BuildContainer);

		public static ComponentApplication ComposeApplication ()
		{
			return _container.Value.Resolve<ComponentApplication> ();
		}

		private static IContainer BuildContainer ()
		{
			var builder = new Autofac.ContainerBuilder ();

			// Global
			builder.Register (c => new SimpleEventLog ())
				.SingleInstance ();
			builder.Register (c => new AppSettings ())
				.SingleInstance ();
			builder.Register (c => new ComponentApplication (
				c.ResolveNamed<Func<System.Windows.Window>> ("main"),
				c.ResolveNamed<Func<UserLevelExceptionData, IDialogView<bool?>>> ("ExceptionReport"))).SingleInstance ();
			builder.Register (c => new WpfDataContainer ())
				.As<IDataContainer> ();
			builder.Register (c => new OleClipboard (WpfDataContainer.ToComDataObject, WpfDataContainer.FromComDataObject))
				.As<IClipboard> ()
				.SingleInstance ();

			// ViewModel
			builder.Register ((c, p) => new MainViewModel (
				c.Resolve<ComponentApplication> (),
				c.Resolve<AppSettings> (),
				c.Resolve<SimpleEventLog> (),
				c.Resolve<IClipboard> (),
				c.Resolve<Func<IDataContainer>> (),
				c.Resolve<ISerivceInterface> (),
				c.ResolveNamed<Func<MessageBoxFormData, Owned<IDialogView<System.Windows.MessageBoxResult>>>> ("MessageBox", p)))
				.As<MainViewModel> ()
				.As<IDragDropSource> ()
				.As<IDragDropTarget> ()
				.SingleInstance ();

			builder.Register ((c, p) => new ExceptionDetailsFormViewModel (p.TypedAs<UserLevelExceptionData> ()))
				.Named<IDialogViewModel<bool?>> ("ExceptionReport");

			builder.Register ((c, p) => new MessageBoxFormViewModel (p.TypedAs<MessageBoxFormData> ()))
				.Named<IDialogViewModel<System.Windows.MessageBoxResult>> ("MessageBox");

			// View
			builder.Register (c => new MainWindow (
				c.Resolve<MainViewModel> (),
				c.Resolve<AppSettings> (),
				c.Resolve<IDragDropSource> (),
				c.Resolve<IDragDropTarget> ()))
				.Named<System.Windows.Window> ("main").SingleInstance ();
			builder.Register ((c, p) => new ExceptionDetailsForm (
				c.ResolveNamed<IDialogViewModel<bool?>> ("ExceptionReport", p)))
				.Named<IDialogView<bool?>> ("ExceptionReport");
			builder.Register ((c, p) => new MessageBoxForm (
				c.ResolveNamed<IDialogViewModel<System.Windows.MessageBoxResult>> ("MessageBox", p)))
				.Named<IDialogView<System.Windows.MessageBoxResult>> ("MessageBox");

			// web-service
			builder.Register (c =>
				{
					var cf = new ChannelFactory<ISerivceInterface> ("WCFServiceEndpoint");
					cf.Credentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
					cf.Credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
					return cf;
				})
				.SingleInstance ();
			Autofac.Integration.Wcf.RegistrationExtensions.UseWcfSafeRelease (
				builder.Register (c => c.Resolve<ChannelFactory<ISerivceInterface>> ().CreateChannel ()));

			var container = builder.Build ();

			return container;
		}
	}
}
