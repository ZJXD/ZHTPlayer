using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Model
{
    [Serializable]
    public abstract class CameraBase :INotifyPropertyChanged
    {
        public virtual DVR DVR { get; set; }

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
                NotifyPropertyChanged("ShowName");
            }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; NotifyPropertyChanged("Number"); }
        }

        public abstract bool Playing { get; }

        public abstract void Play(Control playView);

        public abstract void Stop();

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
