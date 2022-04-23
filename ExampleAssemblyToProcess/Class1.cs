namespace AssemblyToProcess
{
    using System;
    using GetRequiresInit;

    class Class1
    {
        public int IntField { get; set; }

        private bool _initFlag_CheckedIntField = false;
        private int _checkedIntField;
        public int CheckedIntField
        {
            get
            {
                if (!_initFlag_CheckedIntField) {
                    throw new InvalidOperationException();
                }
                return _checkedIntField;
            }
            set
            {
                _checkedIntField = value;
                _initFlag_CheckedIntField = true;
            }
        }
    }

    [GetRequiresInit]
    class Class2
    {
        public int Class2IntField { get; set; }
    }
}