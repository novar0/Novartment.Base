using System;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Фабрика, создающая тело сущности соответствующее указанным параметрам содержимого.
	/// Поддерживает базовые сущности вида
	/// DataEntityBody, CompositeEntityBody, TextEntityBody,
	/// MessageEntityBody, DeliveryStatusEntityBody, DispositionNotificationEntityBody, DigestEntityBody и ReportEntityBody.
	/// </summary>
	public static class EntityBodyFactory
	{
		/// <summary>
		/// Создаёт тело сущности соответствующее указанным параметрам содержимого.
		/// </summary>
		/// <param name="contentProperties">Основные свойства содержимого, в соответствии с которыми будет создано тело.</param>
		/// <returns>Тело сущности, созданное в соответствии с указанными параметрами содержимого.</returns>
		public static IEntityBody Create (EssentialContentProperties contentProperties)
		{
			string charset = null;
			string boundary = null;
			string reportType = null;

			if (contentProperties.Parameters != null)
			{
				foreach (var parameter in contentProperties.Parameters)
				{
					var isCharset = MediaParameterNames.Charset.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
					if (isCharset)
					{
						charset = parameter.Value;
					}
					else
					{
						var isBoundary = MediaParameterNames.Boundary.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
						if (isBoundary)
						{
							boundary = parameter.Value;
						}
						else
						{
							var isReportType = MediaParameterNames.ReportType.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
							if (isReportType)
							{
								reportType = parameter.Value;
							}
						}
					}
				}
			}

			IEntityBody body = null;
			switch (contentProperties.MediaType)
			{
				case ContentMediaType.Text:
					body = new TextEntityBody (charset, contentProperties.TransferEncoding);
					break;
				case ContentMediaType.Message:
					var isRfc822 = MessageMediaSubtypeNames.Rfc822.Equals (contentProperties.MediaSubtype, StringComparison.OrdinalIgnoreCase);
					if (isRfc822)
					{
						body = new MessageEntityBody ();
					}
					else
					{
						var isDeliveryStatus = MessageMediaSubtypeNames.DeliveryStatus.Equals (contentProperties.MediaSubtype, StringComparison.OrdinalIgnoreCase);
						if (isDeliveryStatus)
						{
							body = new DeliveryStatusEntityBody ();
						}
						else
						{
							var isDispositionNotification = MessageMediaSubtypeNames.DispositionNotification.Equals (contentProperties.MediaSubtype, StringComparison.OrdinalIgnoreCase);
							if (isDispositionNotification)
							{
								body = new DispositionNotificationEntityBody ();
							}
						}
					}

					break;
				case ContentMediaType.Multipart:
					var isDigest = MultipartMediaSubtypeNames.Digest.Equals (contentProperties.MediaSubtype, StringComparison.OrdinalIgnoreCase);
					if (isDigest)
					{
						body = new DigestEntityBody (boundary);
					}
					else
					{
						body = MultipartMediaSubtypeNames.Report.Equals (contentProperties.MediaSubtype, StringComparison.OrdinalIgnoreCase) ?
							new ReportEntityBody (reportType, boundary) :
							new CompositeEntityBody (boundary);
					}

					break;
			}

			return body ?? new DataEntityBody (contentProperties.TransferEncoding);
		}
	}
}
