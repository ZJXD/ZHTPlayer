using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Model
{
    public class ErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; }
        public int Code { get; set; }
    }
    public delegate void ErrorDelegate(object sender, ErrorEventArgs e);
}
