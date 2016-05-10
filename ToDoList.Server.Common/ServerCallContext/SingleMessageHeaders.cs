// -----------------------------------------------------------------------
// <copyright file="SingleMessageHeaders.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ToDoList.Server.Common.ServerCallContext
{
    /// <summary>
    /// Wrapper for various-related MessageHeaders.
    /// <para>
    /// Attention: Getters and setters are asymmetric.
    /// </para>
    /// <para>
    /// Getting one of the properties always returns a non-null value.
    /// Setting one of the properties may silently fail if the message headers are in the wrong state.
    /// Read the value back if you need want to make sure it has been set correctly. Due to way this class
    /// is called in the application, that's more useful than throwing an exception.
    /// </para>
    /// </summary>
    public class SingleMessageHeaders
    {
        public const string MetaDataNs = "/ServiceCallContext/2012/05/1";

        private const string ServiceModelDiagnosticsNs = @"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics";
        private const string ServerInstanceHeaderName = "ServerInstance";
 
        private const string SystemHeaderName = "SystemIdentifier";
        private const string SystemIdHeaderName = "SystemId";
        private const string ActivityIdName = "ActivityId";


        private readonly Func<MessageHeaders> _headersCallback;

        public SingleMessageHeaders(Message message)
            : this(() => message.Headers)
        { }

        public SingleMessageHeaders(Func<MessageHeaders> headersCallback)
        {
            _headersCallback = headersCallback;
        }

        /// <summary>
        /// Gets the request ID.
        /// <para>
        /// Attention: The setter is a No-Op.
        /// </para>
        /// </summary>
        public string RequestId
        {
            get
            {
                if (Headers.RelatesTo != null)
                {
                    return Headers.RelatesTo.ToString();
                }
                if (Headers.MessageId != null)
                {
                    return Headers.MessageId.ToString();
                }
                return string.Empty;
            }
            set
            {
                // Nothing to do as we use the MessageID / RelatesTo headers that WCF manages
            }
        }

        /// <summary>
        /// ActivityId Header (is propagated for end-to-end tracing)
        /// </summary>
        public string ActivityId
        {
            get
            {
                return GetHeader(Headers, ActivityIdName, ServiceModelDiagnosticsNs);
            }
            set { SetHeader(Headers, ActivityIdName, value, ServiceModelDiagnosticsNs); }
        }

        

        /// <summary>
        /// Gets or sets the SystemIdentifier
        /// </summary>
        public string SystemIdentifier
        {
            get { return GetHeader(Headers, SystemHeaderName); }
            set { SetHeader(Headers, SystemHeaderName, value); }
        }

       
        /// <summary>
        /// Gets or sets the SystemId
        /// </summary>
        public long SystemId
        {
            get
            {
                long id = 0;

                var sysIdAsString = GetHeader(Headers, SystemIdHeaderName);
                if (!long.TryParse(sysIdAsString, out id))
                    id = 0;

                return id;
            }

            set
            {
                SetHeader(Headers, SystemIdHeaderName, value.ToString());
            }
        }


        /// <summary>
        /// Gets or sets the server instance header.
        /// <para>
        /// Attention:
        /// </para>
        /// <para>
        /// Setting a value may silently fail if the message headers are in the wrong state.
        /// Read the value back if you need want to make sure it has been set correctly.
        /// </para>
        /// </summary>
        public string ServerInstance
        {
            get { return GetHeader(Headers, ServerInstanceHeaderName); }
            set { SetHeader(Headers, ServerInstanceHeaderName, value); }
        }

        private T GetHeader<T>(MessageHeaders headers, string name)
        {
            if (headers != null)
            {
                var index = headers.FindHeader(name, MetaDataNs);
                if (index >= 0)
                {
                    var res = headers.GetHeader<T>(name, MetaDataNs);
                    return res != null ? res : default(T);
                }
            }

            return default(T);
        }

        private string GetHeader(MessageHeaders headers, string name)
        {
            if (headers != null)
            {
                var index = headers.FindHeader(name, MetaDataNs);
                if (index >= 0)
                {
                    return headers.GetHeader<string>(name, MetaDataNs) ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private string GetHeader(MessageHeaders headers, string name, string @namespace)
        {
            if (headers != null)
            {
                var index = headers.FindHeader(name, @namespace);
                if (index >= 0)
                {
                    return headers.GetHeader<string>(name, @namespace) ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private void SetHeader(MessageHeaders headers, string name, string value)
        {
            SetHeader(headers, name, (object)value);
        }

        private void SetHeader(MessageHeaders headers, string name, object value)
        {
            // Happens if called on the client before the call to server. See class documentation.
            if (headers == null)
            {
                return;
            }

            // Happens if service WSDL is accessed from a browser. See class documentation.
            if (headers.MessageVersion.Envelope == EnvelopeVersion.None)
            {
                return;
            }

            var header = MessageHeader.CreateHeader(name, MetaDataNs, value);

            var index = headers.FindHeader(name, MetaDataNs);
            if (index >= 0)
            {
                headers.RemoveAt(index);
                headers.Insert(index, header);
            }
            else
            {
                headers.Add(header);
            }
        }

        private void SetHeader(MessageHeaders headers, string name, string value, string @namespace)
        {
            // Happens if called on the client before the call to server. See class documentation.
            if (headers == null)
            {
                return;
            }

            // Happens if service WSDL is accessed from a browser. See class documentation.
            if (headers.MessageVersion.Envelope == EnvelopeVersion.None)
            {
                return;
            }

            var header = MessageHeader.CreateHeader(name, @namespace, value);

            var index = headers.FindHeader(name, @namespace);
            if (index >= 0)
            {
                headers.RemoveAt(index);
                headers.Insert(index, header);
            }
            else
            {
                headers.Add(header);
            }
        }

        private MessageHeaders Headers
        {
            get { return _headersCallback(); }
        }
    }
}