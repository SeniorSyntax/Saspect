using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Saspect.Autofac
{
	public static class ApplyAspectsExtensions
	{
		public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
			ApplyAspects<TLimit, TRegistrationStyle>(
				this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration
			)
		{
			registration.ActivatorData.ConfigurationActions.Add((t, builder) => applyAspects(builder));
			return registration;
		}

		public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>
			ApplyAspects<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
				this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration
			) where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
		{
			applyAspects(registration);
			return registration;
		}

		private static void applyAspects<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
			IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration)
			where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
		{
			var type = registration.ActivatorData.ImplementationType;
			var aspectedType = GeneratedAspectProxyUtil.GetAspectedType(type);
			if (aspectedType == null) 
				return;
			registration.ActivatorData.ImplementationType = aspectedType;
		}
	}
}
