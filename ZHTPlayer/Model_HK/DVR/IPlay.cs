using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IPlay
    {
        event ErrorDelegate OnPlayError;

        void Play(System.Windows.Forms.Control control);

        string Name { get; set; }

        void Stop();

        bool Playing { get; }

        void Pause();

        void Continue();

        void BeginRecord(string name);

        void StopRecord();
    }
}
