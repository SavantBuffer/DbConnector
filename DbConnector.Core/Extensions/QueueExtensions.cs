//Copyright 2019 Robert Orama

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;

namespace DbConnector.Core.Extensions
{
    public static class QueueExtensions
    {
        /// <summary>
        /// Enqueues the elements of the specified collection into the <see cref="Queue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type to use.</typeparam>
        /// <param name="queue">The <see cref="Queue{T}"/> to use.</param>
        /// <param name="collection">
        /// The collection whose elements should be enqueued into the <see cref="Queue{T}"/>. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.
        /// </param>
        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> collection)
        {
            foreach (var obj in collection)
                queue.Enqueue(obj);
        }
    }
}
