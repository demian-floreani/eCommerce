﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RNN.Services
{
    public interface IFacebookIntegrationService
    {
        Task PostLink();
    }
}
