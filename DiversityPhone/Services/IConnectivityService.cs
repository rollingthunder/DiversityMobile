﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public interface IConnectivityService
    {
        ConnectionStatus CurrentStatus { get; }

    }
}