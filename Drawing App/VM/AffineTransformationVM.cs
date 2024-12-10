using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing_App.VM
{
    public class AffineTransformationVM:BindableBase
    {
        private string _a00;
        public string a00
        {
            get => _a00;
            set => SetProperty(ref _a00, value);
        }
        private string _a01;
        public string a01
        {
            get => _a01;
            set => SetProperty(ref _a01, value);
        }
        private string _a02;
        public string a02
        {
            get => _a02;
            set => SetProperty(ref _a02, value);
        }
        private string _a10;
        public string a10
        {
            get => _a10;
            set => SetProperty(ref _a10, value);
        }
        private string _a11;
        public string a11
        {
            get => _a11;
            set => SetProperty(ref _a11, value);
        }
        private string _a12;
        public string a12
        {
            get => _a12;
            set => SetProperty(ref _a12, value);
        }
        public AffineTransformationVM()
        {
            _a00 = "0";
            _a01 = "0";
            _a02 = "0";
            _a10 = "0";
            _a11 = "0";
            _a12 = "0";
        }
    }
}
