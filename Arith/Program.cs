using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arithmetic
{
    class Test
    {
        public static void Test1(UserList<int> ar)
        {
            var rnd = new Random();
            var rangeList = Enumerable.Range(0, 10).ToList();
            rangeList.ForEach(i => ar.Add(rnd.Next(10)));
            var p = rangeList.Where(s => ar.Remove(rnd.Next(10))).ToList();
            ar.ToList().ForEach(s => Console.Write($"{s} "));
            Console.WriteLine();
            p.ForEach(s => Console.Write($"{s} "));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var str = "sin15x log-6x *-5 (cos-12x+ 7)^(sin8/cos8x) + 6"; //"-sin(-3log(- 3 sin -1.5157 x ^ 3.5x / -x ^ 25.27 log-x)(18x + 3.87)/x^-20)";
            double dbX = -0.395;
            try
            {
                Console.WriteLine($"x = {dbX}\n{str} = {Calc.Start(str, dbX)}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Console.ReadLine();
        }
    }

    class Calc
    {
        private static readonly char UnoMinus = '-';
        private static readonly char OpenBracket = '(';
        private static readonly char CloseBracket = ')';
        private static readonly char CharVariable = 'x';
        private enum FuncEnum { Nun, Sin, Cos, Log, Min };


        public static double Start(string startValue, double xValue)
        {
            var node = new UserList<char>(startValue.Where(ch => ch != ' ')).First;
            return StrResult(default(char), ref node, out char value).SetIntValue(xValue);
        }

        private static IOperand StrResult(char preOperand, ref IValuesAndLink<char> sValue, out char lastOperand)
        {
            var a = FindValue(ref sValue);
            var operand = FindOperand(ref sValue, out int operandPreor);

            if (operand == CloseBracket || preOperand != default(char) && operandPreor < 2)
            {
                lastOperand = operand;
                return a;
            }

            do
            {
                var b = FindValue(ref sValue);
                var nextOperand = FindOperand(ref sValue, out int nextOperandPreor);


                if (operandPreor >= nextOperandPreor)
                {

                    a = DuoOperand(a, b, operand);
                    operand = nextOperand;
                }
                else
                {
                    if (operandPreor == 1)
                    {
                        a = DuoOperand(a, DuoOperand(b, StrResult(operand, ref sValue, out char tempOperand), nextOperand), operand);
                        operand = tempOperand;
                    }
                    else a = DuoOperand(a, StrResult(default(char), ref sValue, out nextOperand), operand);
                }
            } while (operand != CloseBracket && sValue != null);
            lastOperand = default(char);
            return a;
        }

        private static char FindOperand(ref IValuesAndLink<char> charter, out int intPreor)
        {
            if (charter == null)
            {
                intPreor = 0;
                return default(char);
            }
            switch (charter.Value)
            {
                case '+':
                case '-':
                    charter = charter.Next;
                    intPreor = 0;
                    return charter.Previous.Value;
                case '*':
                case '/':
                    charter = charter.Next;
                    intPreor = 1;
                    return charter.Previous.Value;
                case '^':
                    charter = charter.Next;
                    intPreor = 2;
                    return charter.Previous.Value;
                case ')':
                    var temp = charter.Value;
                    if (charter.Next != null) charter = charter.Next;
                    intPreor = -1;
                    return temp;
                default:
                    intPreor = 1;
                    return '*';
            }
        }
        private static IOperand FindValue(ref IValuesAndLink<char> charter)
        {
            var blUno1 = false;
            var blUno2 = false;
            IOperand aValue;
            var strNumbs = "";
            if (charter.Value == UnoMinus)
            {
                blUno1 = true;
                charter = charter.Next;
            }
            FindFunc(ref charter, out FuncEnum funcValue);
            if (charter.Value == OpenBracket)
            {
                charter = charter.Next;
                aValue = StrResult(default(char), ref charter, out char tempOperand);
            }
            else
            {

                if (charter.Value == UnoMinus)
                {
                    blUno2 = true;
                    charter = charter.Next;
                }

                while (charter != null && NumbersPredicate(charter.Value))
                {
                    strNumbs += charter.Value;
                    charter = charter.Next;
                }
                if (charter?.Value == CharVariable)
                {
                    aValue = strNumbs == "" ? (IOperand)new Variable() : new Umn(new Const(Double.Parse(strNumbs.Replace('.', ','))), new Variable());
                    charter = charter.Next;
                }
                else
                {
                    if (strNumbs == "") throw new Exception("Некорректная запись");
                    aValue = new Const(strNumbs == "" ? 1 : Double.Parse(strNumbs.Replace('.', ',')));
                }
                if (blUno2) aValue = UnoOperand(aValue, FuncEnum.Min);
            }
            aValue = UnoOperand(aValue, funcValue);
            if (blUno1) aValue = UnoOperand(aValue, FuncEnum.Min);
            return aValue;
        }

        private static void FindFunc(ref IValuesAndLink<char> nodeValue, out FuncEnum funcValue)
        {
            funcValue = FuncEnum.Nun;
            var strFunc = "";
            while (nodeValue != null && !NumbersPredicate(nodeValue.Value) && nodeValue.Value != UnoMinus && nodeValue.Value != OpenBracket && nodeValue.Value != CharVariable)
            {
                strFunc += nodeValue.Value;
                nodeValue = nodeValue.Next;
            }

            switch (strFunc)
            {
                case "sin":
                    {
                        funcValue = FuncEnum.Sin;
                        break;
                    }
                case "cos":
                    {
                        funcValue = FuncEnum.Cos;
                        break;
                    }
                case "log":
                    {
                        funcValue = FuncEnum.Log;
                        break;
                    }
                default:
                    {
                        if (strFunc.Length > 0) throw new Exception("Некорректная запись");
                        break;
                    }
            }
        }


        static IOperand DuoOperand(IOperand a, IOperand b, char operand)
        {
            IOperand value;
            switch (operand)
            {
                case '+':
                    value = new Add(a, b);
                    break;
                case '-':
                    value = new Min(a, b);
                    break;
                case '*':
                    value = new Umn(a, b);
                    break;
                case '/':
                    value = new Del(a, b);
                    break;
                case '^':
                    value = new Up(a, b);
                    break;
                default:
                    throw new Exception("Некорректная запись");
            }
            return value;
        }

        static IOperand UnoOperand(IOperand a, FuncEnum operand)
        {
            IOperand value;
            switch (operand)
            {
                case FuncEnum.Nun:
                    value = a;
                    break;
                case FuncEnum.Sin:
                    value = new Sin(a);
                    break;
                case FuncEnum.Cos:
                    value = new Cos(a);
                    break;
                case FuncEnum.Log:
                    value = new Ln(a);
                    break;
                case FuncEnum.Min:
                    value = new UnoMin(a);
                    break;
                default:
                    throw new Exception("Некорректная запись");
            }
            return value;
        }

        static bool NumbersPredicate(char ch) => (ch >= '0') && (ch <= '9') || (ch == '.');
    }

    interface IOperand
    {
        double SetIntValue(double value);
    }

    class Const : IOperand
    {
        private readonly double _doubValue;

        public Const(double value)
        {
            _doubValue = value;
        }


        public double SetIntValue(double value) => _doubValue;
    }

    class Variable : IOperand
    {
        private double _doubValue;

        public double SetIntValue(double value)
        {
            _doubValue = value;
            return _doubValue;
        }
    }

    abstract class UnoMethod : IOperand
    {
        private readonly IOperand _value1;
        private double _resultValue;

        protected UnoMethod(IOperand a)
        {
            _value1 = a;
        }

        protected abstract double Operation(double a);

        public double SetIntValue(double value)
        {
            _resultValue = Operation(_value1.SetIntValue(value));
            return _resultValue;
        }
    }

    class Sin : UnoMethod
    {
        public Sin(IOperand a) : base(a)
        {
        }

        protected override double Operation(double a) => Math.Sin(a);
    }

    class Cos : UnoMethod
    {
        public Cos(IOperand a) : base(a)
        {
        }

        protected override double Operation(double a) => Math.Cos(a);
    }

    class Ln : UnoMethod
    {
        public Ln(IOperand a) : base(a)
        {
        }

        protected override double Operation(double a) => Math.Log(a);
    }

    class UnoMin : UnoMethod
    {
        public UnoMin(IOperand a) : base(a)
        {
        }

        protected override double Operation(double a) => -a;
    }

    abstract class Method : IOperand
    {
        private readonly IOperand _value1;
        private readonly IOperand _value2;
        private double _resultValue;

        protected Method(IOperand a, IOperand b)
        {
            _value1 = a;
            _value2 = b;
        }

        public abstract double Operation(double a, double b);

        public double SetIntValue(double value)
        {
            _resultValue = Operation(_value1.SetIntValue(value), _value2.SetIntValue(value));
            return _resultValue;
        }
    }

    class Min : Method
    {
        public Min(IOperand a, IOperand b) : base(a, b)
        {
        }

        public override double Operation(double a, double b) => (a - b);
    }

    class Add : Method
    {
        public Add(IOperand a, IOperand b) : base(a, b)
        {
        }

        public override double Operation(double a, double b) => a + b;
    }

    class Umn : Method
    {
        public Umn(IOperand a, IOperand b) : base(a, b)
        {
        }

        public override double Operation(double a, double b) => a * b;
    }

    class Del : Method
    {
        public Del(IOperand a, IOperand b) : base(a, b)
        {
        }

        public override double Operation(double a, double b)
        {
            if (Math.Abs(b) < 0.000000000000000000000001) throw new Exception("не делите на ноль");
            return a / b;
        }
    }
    class Up : Method
    {
        public Up(IOperand a, IOperand b) : base(a, b)
        {
        }

        public override double Operation(double a, double b)
        {
            return Math.Pow(a, b);
        }
    }
    interface IValuesAndLink<T>
    {
        IValuesAndLink<T> Next { get; }
        IValuesAndLink<T> Previous { get; }
        T Value { get; set; }
        UserList<T> List { get; }
    }
    class UserList<T> : ICollection<T>
    {
        private class ValuesAndLink : IValuesAndLink<T>
        {
            public UserList<T> List { get; private set; }
            internal int _index;
            internal T _value;
            internal bool _enable;
            internal int _nextIndex;
            internal int _previousIndex;

            public ValuesAndLink(int index, UserList<T> list)
            {
                _index = index;
                _value = default(T);
                _nextIndex = index == list.Capasity - 1 ? 0 : index + 1;
                _previousIndex = index == 0 ? list.Capasity - 1 : index - 1;
                List = list;
                _enable = false;
            }

            public ValuesAndLink(int index, T value, UserList<T> list)
            {
                _index = index;
                _value = value;
                _nextIndex = index == list.Capasity - 1 ? 0 : index + 1;
                _previousIndex = index == 0 ? list.Capasity - 1 : index - 1;
                List = list;
                _enable = true;
            }

            public ValuesAndLink(int index, T value, int nextIndex, int previousIndex, UserList<T> list)
            {
                _index = index;
                _value = value;
                _nextIndex = nextIndex;
                _previousIndex = previousIndex;
                List = list;
                _enable = true;
            }

            public T Value
            {
                get { return _value; }
                set { _value = value; }
            }

            public ValuesAndLink Next
            {
                get {return List._arr[_nextIndex]; }
                internal set { List._arr[_nextIndex] = value; } 
            }

            public ValuesAndLink Previous
            {
                get { return List._arr[_previousIndex]; }
                internal set { List._arr[_previousIndex] = value; }
            }

            IValuesAndLink<T> IValuesAndLink<T>.Next => List._arr[_nextIndex]._enable == false ? null : List._arr[_nextIndex];
            IValuesAndLink<T> IValuesAndLink<T>.Previous => List._arr[_previousIndex]._enable == false ? null : List._arr[_previousIndex];

        }

        private ValuesAndLink[] _arr;
        private int _firstIndex;
        private int _lastIndex;
        private int _capasity;
        private int _count;


        public int Capasity
        {
            get {return _capasity; } 
            set
            {
                if (value <= Count) throw new IndexOutOfRangeException();

                if (value > Capasity)
                {
                    var temp = _arr;
                    var maxNumber = Capasity;
                    _arr = new ValuesAndLink[value];

                    Array.Copy(temp, _arr, maxNumber); // TODO >, <
                    _arr[_firstIndex].Previous._nextIndex = maxNumber;
                    _arr[_firstIndex]._previousIndex = value - 1;
                    _capasity = value;
                    for (int index = maxNumber; index < value; index++)
                        _arr[index] = new ValuesAndLink(index, this);
                }
                else if (value < Capasity)
                {
                    _capasity = value;
                    var temp = new ValuesAndLink[value];
                    var index = 0;
                    var lastValue = _arr[_firstIndex];
                    //IValuesAndLink<T> lastItems = _arr[_firstIndex];
                    while (lastValue._enable)
                    {
                        temp[index] = new ValuesAndLink(index, lastValue.Value, this);
                        index++;
                        lastValue = lastValue.Next;
                    }
                    _firstIndex = 0;
                    _lastIndex = index == 0 ? 0 : index - 1;
                    while (index < value)
                    {
                        temp[index] = new ValuesAndLink(index, this);
                        index++;
                        lastValue = lastValue.Next;
                    }
                    _arr = temp;
                }
            }
        }
        public int Count
        {
            get { return _count; }
            private set
            {
                if (value >= Capasity)
                    Capasity <<= 1;
                if (_count == 0) _lastIndex = _arr[_lastIndex]._previousIndex;
                _count = value;
            }
        }

        public bool IsReadOnly { get; }

        public IValuesAndLink<T> First => _arr[_firstIndex];
        public IValuesAndLink<T> Last => _arr[_lastIndex];

        public UserList(IEnumerable<T> values)
        {
            _count = values.Count();
            _capasity = (int)Math.Pow(2, Math.Ceiling(Math.Log(_count + 1) / Math.Log(2)));
            _arr = new ValuesAndLink[Capasity];
            var index = 0;
            foreach (var value in values)
            {
                _arr[index] = new ValuesAndLink(index, value, this);
                index++;
            }

            _firstIndex = 0;
            _lastIndex = index == 0 ? 0 : index - 1;

            for (; index < Capasity; index++)
                _arr[index] = new ValuesAndLink(index, this);
        }

        public void AddAfter(IValuesAndLink<T> elementNode, T elenet)
        {

            var tempIndex = _arr[_lastIndex]._nextIndex;
            Removing(_arr[_lastIndex]._nextIndex);
            _arr[tempIndex].Value = elenet;
            _arr[tempIndex]._enable = true;
            _arr[tempIndex].Next = ((ValuesAndLink)elementNode).Next;
            ((ValuesAndLink) elementNode).Next = _arr[tempIndex];
            _arr[tempIndex].Previous = (ValuesAndLink)elementNode;
        }
        public void AddBefor(IValuesAndLink<T> elementNode, T elenet)
        {
            var tempIndex = _arr[_lastIndex]._nextIndex;
            Removing(_arr[_lastIndex]._nextIndex);
            _arr[tempIndex].Value = elenet;
            _arr[tempIndex].Next = (ValuesAndLink)elementNode;
            _arr[tempIndex].Previous = (ValuesAndLink)elementNode.Previous;
            ((ValuesAndLink)elementNode).Previous = _arr[tempIndex];

        }
        public void AddFirst(T elenet)
        {
            Count++;
            _arr[_firstIndex].Previous._value = elenet;
            _arr[_firstIndex].Previous._enable = true;
            _firstIndex = _arr[_firstIndex].Previous._index;
        }
        public void AddLast(T elenet)
        {
            Count++;
            _arr[_lastIndex].Next._value = elenet;
            _arr[_lastIndex].Next._enable = true;
            _lastIndex = _arr[_lastIndex].Next._index;
        }

        public void Add(T item) => AddLast(item);

        public void Clear()
        {
            var variable = _arr[_firstIndex];
            while (variable._enable)
            {
                variable._enable = false;
                variable = variable.Next;
            }
        }

        public bool Contains(T item)
        {
            var flag = false;
            var variable = GetEnumerator();
            while (variable.MoveNext())
            {
                if (!variable.Current.Equals(item)) continue;
                flag = true;
                break;
            }
            return flag;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var variable = GetEnumerator();
            while (variable.MoveNext())
                array[arrayIndex++] = variable.Current;
        }

        public IValuesAndLink<T> Find(T element)
        {
            var variable = _arr[_firstIndex];
            while (variable._enable)
            {
                if (variable.Value.Equals(element))
                    return variable;
                variable = variable.Next;
            }
            return null;
        }

        public IValuesAndLink<T> FindLast(T element)
        {
            var variable = _arr[_lastIndex];
            while (variable._enable)
            {
                if (variable.Value.Equals(element))
                    return variable;
                variable = variable.Previous;
            }
            return null;
        }

        public bool Remove(T element)
        {
            var flag = false;
            var variable = _arr[_firstIndex];
            while (variable._enable)
            {
                if (variable.Value.Equals(element))
                {
                    Removing(variable._index);
                    flag = true;
                    break;
                }
                variable = variable.Next;
            }
            return flag;
        }

        public bool Remove(IValuesAndLink<T> elementNode)
        {
            var flag = false;
            var variable = _arr[_firstIndex];
            while (variable._enable)
            {
                if (variable.Equals(elementNode))
                {
                    Removing(variable._index);
                    flag = true;
                    break;
                }
                variable = variable.Next;
            }
            return flag;
        }

        public void RemoveFirst() => Removing(_firstIndex);
        public void RemoveLast() => Removing(_lastIndex);

        private void Removing(int indexRemove)
        {
            if (indexRemove == _firstIndex) _firstIndex = _arr[indexRemove]._nextIndex;
            else if (indexRemove == _lastIndex) _lastIndex = _arr[indexRemove]._previousIndex;
            _arr[indexRemove]._enable = false;
            _arr[indexRemove].Previous._nextIndex = _arr[indexRemove]._nextIndex;
            _arr[indexRemove].Next._previousIndex = _arr[indexRemove]._previousIndex;
            Count--;
        }

        public IEnumerator<T> GetEnumerator()
        {
            IValuesAndLink<T> value = _count > 0 ? _arr[_firstIndex] : null;
            while (value != null)
            {
                yield return value.Value;
                value = value.Next;
            };
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


}