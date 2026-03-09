using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviGo.Common.Exceptions
{
    public class BusinessException(string message) : Exception(message)
    {
    }
}
