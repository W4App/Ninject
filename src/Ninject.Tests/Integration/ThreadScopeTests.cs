﻿using System;
using System.Threading;
using Ninject.Activation.Caching;
using Ninject.Infrastructure.Disposal;
using Ninject.Tests.Fakes;
using Xunit;

namespace Ninject.Tests.Integration.ThreadScopeTests
{
	public class ThreadScopeContext
	{
		protected readonly StandardKernel kernel;

		public ThreadScopeContext()
		{
			var settings = new NinjectSettings { CachePruningIntervalMs = Int32.MaxValue };
			kernel = new StandardKernel(settings);
		}
	}

	public class WhenServiceIsBoundWithThreadScope : ThreadScopeContext
	{
		[Fact]
		public void FirstActivatedInstanceIsReusedWithinThread()
		{
			kernel.Bind<IWeapon>().To<Sword>().InThreadScope();

			IWeapon weapon1 = null;
			IWeapon weapon2 = null;

			ThreadStart callback = () =>
			{
				weapon1 = kernel.Get<IWeapon>();
				weapon2 = kernel.Get<IWeapon>();
			};

			var thread = new Thread(callback);

			thread.Start();
			thread.Join();

			weapon1.ShouldNotBeNull();
			weapon2.ShouldNotBeNull();
			weapon1.ShouldBeSameAs(weapon2);
		}

		[Fact]
		public void ScopeDoesNotInterfereWithExternalRequests()
		{
			kernel.Bind<IWeapon>().To<Sword>().InThreadScope();

			IWeapon weapon1 = kernel.Get<IWeapon>();
			IWeapon weapon2 = null;

			ThreadStart callback = () => weapon2 = kernel.Get<IWeapon>();

			var thread = new Thread(callback);

			thread.Start();
			thread.Join();

			weapon1.ShouldNotBeNull();
			weapon2.ShouldNotBeNull();
			weapon1.ShouldNotBeSameAs(weapon2);
		}

		[Fact]
		public void InstancesActivatedWithinScopeAreDeactivatedAfterThreadIsGarbageCollectedAndCacheIsPruned()
		{
			kernel.Bind<NotifiesWhenDisposed>().ToSelf().InThreadScope();
			var cache = kernel.Components.Get<ICache>();

			NotifiesWhenDisposed instance = null;

			ThreadStart callback = () => instance = kernel.Get<NotifiesWhenDisposed>();

			var thread = new Thread(callback);
			var threadReference = new WeakReference(thread);

			thread.Start();
			thread.Join();

			thread = null;

			GC.Collect();

			threadReference.WaitUntilGarbageCollected();
			cache.Prune();

			instance.ShouldNotBeNull();
			instance.IsDisposed.ShouldBeTrue();
		}
	}
}