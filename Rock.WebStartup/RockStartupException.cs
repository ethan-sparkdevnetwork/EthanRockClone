// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

namespace Rock.WebStartup
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class RockStartupException : Exception
    {
        private readonly Exception _exception;
        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockStartupException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RockStartupException( string message )
            : this( message, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockStartupException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="exception">The exception.</param>
        public RockStartupException( string message, Exception exception )
            : base( message, exception )
        {
            _message = message;
            _exception = exception;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message => $"{_message}:{_exception?.Message}";

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        public override string StackTrace
        {
            get
            {
                if ( _exception == null )
                {
                    return base.StackTrace;
                }

                string stackTrace = _exception.StackTrace;
                var innerException = _exception.InnerException;
                while ( innerException != null )
                {
                    stackTrace += "\n\n" + innerException.Message;
                    stackTrace += "\n" + innerException.StackTrace;
                    innerException = innerException.InnerException;
                }

                return stackTrace;
            }
        }
    }

}
