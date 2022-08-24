﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models
{
    public record CommandResultBase
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
    }

    
}
