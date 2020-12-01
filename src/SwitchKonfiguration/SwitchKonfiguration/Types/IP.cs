namespace SwitchKonfiguration.Types
{
    public class IPv4
    {
        #region PrivateFields

        private int _firstBlock;
        private int _secondBlock;
        private int _thirdBlock;
        private int _fourthBlock;
        private bool _isValid;

        #endregion

        #region Properties

        public int FirstBlock
        {
            get
            {
                return _firstBlock;
            }
            set
            {
                if(value >= 0 && value <= 255)
                {
                    _firstBlock = value;
                }
                else
                {
                    _isValid = false;
                }
            }
        }

        public int SecondBlock
        {
            get
            {
                return _secondBlock;
            }
            set
            {
                if (value >= 0 && value <= 255)
                {
                    _secondBlock = value;
                }
                else
                {
                    _isValid = false;
                }
            }
        }

        public int ThirdBlock
        {
            get
            {
                return _thirdBlock;
            }
            set
            {
                if (value >= 0 && value <= 255)
                {
                    _thirdBlock = value;
                }
                else
                {
                    _isValid = false;
                }
            }
        }

        public int FourthBlock
        {
            get
            {
                return _fourthBlock;
            }
            set
            {
                if (value >= 0 && value <= 255)
                {
                    _fourthBlock = value;
                }
                else
                {
                    _isValid = false;
                }
            }
        }

        #endregion

        #region Constructors

        public IPv4()
        {
            _firstBlock = -1;
            _secondBlock = -1;
            _thirdBlock = -1;
            _fourthBlock = -1;
            _isValid = false;
        }

        public IPv4(int block1, int block2, int block3, int block4)
        {
            _isValid = true;
            FirstBlock = block1;
            SecondBlock = block2;
            ThirdBlock = block3;
            FourthBlock = block4;
        }

        public IPv4(string iPv4String)
        {
            _isValid = SetByString(iPv4String);
        }

        #endregion

        #region PublicMethods

        public bool SetByString(string toConvert)
        {
            _isValid = true;
            int count = 0;
            int tmp;
            bool check;

            foreach(string block in toConvert.Split('.'))
            {
                if (!_isValid) break;
                check = int.TryParse(block, out tmp);
                if (!check)
                {
                    _isValid = false;
                    _firstBlock = -1;
                    _secondBlock = -1;
                    _thirdBlock = -1;
                    _fourthBlock = -1;
                    break;
                }
                switch (count)
                {
                    case 0:
                        FirstBlock = tmp;
                        break;

                    case 1:
                        SecondBlock = tmp;
                        break;

                    case 2:
                        ThirdBlock = tmp;
                        break;

                    case 3:
                        FourthBlock = tmp;
                        break;

                    default:
                        _isValid = false;
                        break;
                }
            }
            return _isValid;
        }

        public bool checkValidation()
        {
            _isValid = true;
            FirstBlock = _firstBlock;
            SecondBlock = _secondBlock;
            ThirdBlock = _thirdBlock;
            FourthBlock = _fourthBlock;
            return _isValid;
        }

        public override string ToString()
        {
            return $"{_firstBlock}.{_secondBlock}.{_thirdBlock}.{_fourthBlock}";
        }

        #endregion
    }
}