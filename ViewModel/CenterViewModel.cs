using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace barcode_gen.ViewModel
{
    public class CenterViewModel : INotifyPropertyChanged
    {
        #region varible
        private string _tbText;
        private string _prefics = "A-";
        private string _postfics ="-Z";
        private string _from = "10";
        private string _to = "100";
        private string _step = "10";
        private string _mask = "$$$$";
        #endregion
        #region Properties
        public string TbText 
        { 
            get => _tbText;
            set
            {
                _tbText = value;
                OnPropertyChanged(nameof(TbText));
            }
        }
        public string Prefics 
        { 
            get => _prefics;
            set
            {
                _prefics = value;
                OnPropertyChanged(nameof(Prefics));
            }
        } 
        public string Postfics
        {
            get => _postfics;
            set
            {
                _postfics = value;
                OnPropertyChanged(nameof(Postfics));
            }
        }
        public string From
        {
            get => _from;
            set
            {
                _from = value;
                OnPropertyChanged(nameof(From));
            }
        }
        public string To
        {
            get => _to;
            set
            {
                _to = value;
                OnPropertyChanged(nameof(To));
            }
        }
        public string Step
        {
            get => _step;
            set
            {
                _step = value;
                OnPropertyChanged(nameof(Step));
            }
        }
        public string Mask
        {
            get => _mask;
            set
            {
                _mask = value;
                OnPropertyChanged(nameof(Mask));
            }
        }
        #endregion
        #region ctor
        public CenterViewModel()
        {
            CreateSequence = new RelayCommand(() =>
            {
                Int32.TryParse(From, out int current);
                Int32.TryParse(To, out int to);
                Int32.TryParse(Step, out int step);
                bool isGenerate = current < to;
                var collection = new List<String>();

                while (isGenerate)
                {
                    var sb = new StringBuilder();
                    if (Mask.Length > From.ToString().Length)
                    {
                        sb.Append('0', Mask.Length);
                        string sb2 = new string(current.ToString().Reverse().ToArray());
                        for (int i = 0; i < sb2.Length; i++)
                        {
                            sb[i] = sb2[i];
                        }
                        var reverse = new string(sb.ToString().Reverse().ToArray());
                        collection.Add(Prefics + reverse + Postfics);

                    }
                    else
                    {
                        collection.Add(Prefics + current + Postfics);
                    }
                    current = current + step;
                    isGenerate = current <= to;
                }

                foreach (var item in collection)
                {
                    TbText = TbText + item + "\r\n";
                }
            });
            ClearField = new RelayCommand(() => 
            {
                TbText = "";
            });
        }
        #endregion
        #region command
        public ICommand CreateSequence { get; }
        public ICommand ClearField { get; }
        #endregion
        #region onProperty
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
