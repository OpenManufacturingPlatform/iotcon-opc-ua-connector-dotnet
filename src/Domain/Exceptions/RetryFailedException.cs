﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;

namespace OMP.Connector.Domain.Exceptions
{
    public class RetryFailedException : RetryException
    {
        public RetryFailedException(Exception exception, int retryAttempt)
            : base("Retry of operation failed.", exception, retryAttempt) { }
    }
}