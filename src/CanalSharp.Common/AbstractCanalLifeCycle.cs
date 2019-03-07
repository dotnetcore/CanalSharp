// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CanalSharp.Common.Exception;

namespace CanalSharp.Common
{
    public abstract class AbstractCanalLifeCycle : ICanalLifeCycle
    {
        /// <summary>
        /// Whether Canal is running or not.
        /// </summary>
        protected volatile bool Running;

        public bool IsStart()
        {
            return Running;
        }

        public virtual void Start()
        {
            if (Running)
            {
                throw new CanalException($" {nameof(AbstractCanalLifeCycle)} has startup , don't repeat start");
            }

            Running = true;
        }

        public virtual void Stop()
        {
            if (!Running)
            {
                throw new CanalException($"{nameof(AbstractCanalLifeCycle)} isn't start , please check");
            }

            Running = false;
        }
    }
}