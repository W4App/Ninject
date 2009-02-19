﻿#region License
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2007-2009, Enkari, Ltd.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using Ninject.Activation.Hooks;
using Ninject.Infrastructure.Disposal;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Syntax;
#endregion

namespace Ninject.Activation.Scope
{
	/// <summary>
	/// A scope used for deterministic disposal of activated instances. When the scope is
	/// disposed, all instances activated via it will be deactivated.
	/// </summary>
	public class ActivationScope : DisposableObject, IActivationScope
	{
		/// <summary>
		/// Gets or sets the parent resolution root (usually the kernel).
		/// </summary>
		public IResolutionRoot Parent { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ActivationScope"/> class.
		/// </summary>
		/// <param name="parent">The parent resolution root.</param>
		public ActivationScope(IResolutionRoot parent)
		{
			Parent = parent;
		}

		/// <summary>
		/// Determines whether the specified request can be resolved.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns><c>True</c> if the request can be resolved; otherwise, <c>false</c>.</returns>
		public bool CanResolve(IRequest request)
		{
			return Parent.CanResolve(request);
		}

		/// <summary>
		/// Resolves the specified request.
		/// </summary>
		/// <param name="service">The service to resolve.</param>
		/// <param name="constraint">The constraint to apply to the bindings to determine if they match the request.</param>
		/// <param name="parameters">The parameters to pass to the resolution.</param>
		/// <param name="isOptional"><c>True</c> if the request is optional; otherwise, <c>false</c>.</param>
		/// <returns>A series of hooks that can be used to resolve instances that match the request.</returns>
		public IEnumerable<IHook> Resolve(Type service, Func<IBindingMetadata, bool> constraint, IEnumerable<IParameter> parameters, bool isOptional)
		{
			return Resolve(CreateDirectRequest(service, constraint, parameters, isOptional));
		}

		/// <summary>
		/// Resolves the specified request.
		/// </summary>
		/// <param name="request">The request to resolve.</param>
		/// <returns>A series of hooks that can be used to resolve instances that match the request.</returns>
		public IEnumerable<IHook> Resolve(IRequest request)
		{
			return Parent.Resolve(request);
		}

		/// <summary>
		/// Creates a request for the specified service.
		/// </summary>
		/// <param name="service">The service to resolve.</param>
		/// <param name="constraint">The constraints to apply to the bindings to determine if they match the request.</param>
		/// <param name="parameters">The parameters to pass to the resolution.</param>
		/// <param name="isOptional"><c>True</c> if the request is optional; otherwise, <c>false</c>.</param>
		/// <returns>The created request.</returns>
		protected virtual IRequest CreateDirectRequest(Type service, Func<IBindingMetadata, bool> constraint, IEnumerable<IParameter> parameters, bool isOptional)
		{
			return new Request(service, constraint, parameters, () => this, isOptional);
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			return this.Get(serviceType);
		}
	}
}