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

        /// <summary>
        /// Converts a string to an IPv4
        /// </summary>
        /// <param name="toConvert">The string that is to be converted</param>
        /// <returns>if the conversion was successfull: true</returns>
        public bool SetByString(string toConvert)
        {
            _isValid = true;
            int count = 0;
            int tmp;
            bool check;

            //tries to split a string like this: "192.168.162.100" to this: "192" "168" "162" "100"
            //then proceeds to try to fit in each block to its respective part
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
                        count++;
                        break;

                    case 1:
                        SecondBlock = tmp;
                        count++;
                        break;

                    case 2:
                        ThirdBlock = tmp;
                        count++;
                        break;

                    case 3:
                        FourthBlock = tmp;
                        count++;
                        break;

                    default:
                        _isValid = false;
                        count++;
                        break;
                }
            }
            return _isValid;
        }

        /// <summary>
        /// Checks if the currently saved IPv4 is an IPv4 or not
        /// </summary>
        /// <returns></returns>
        public bool checkValidation()
        {
            //reusing the properties to check
            _isValid = true;
            FirstBlock = _firstBlock;
            SecondBlock = _secondBlock;
            ThirdBlock = _thirdBlock;
            FourthBlock = _fourthBlock;
            return _isValid;
        }

        /// <summary>
        /// Converts the IPv4 to a string
        /// </summary>
        /// <returns>The converted string</returns>
        public override string ToString()
        {
            return $"{_firstBlock}.{_secondBlock}.{_thirdBlock}.{_fourthBlock}";
        }

        #endregion
    }
}