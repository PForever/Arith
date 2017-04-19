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
            rangeList.ForEach(i => ar.Add(i));

            //var p = rangeList.Where(s => ar.Remove(rnd.Next(10))).ToList();
            ar.ToList().ForEach(s => Console.Write($"{s} "));
            var variable = ar.First;
            var variable1 = variable.Next;
            for (var i = 0; i < 3; i++)
            {
                variable = variable.Next;
            }
            ar.AddBefor(variable1, 20);
            Console.WriteLine();
            ar.ToList().ForEach(s => Console.Write($"{s} "));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Пример ввода:");
            var str1 = "-sin15x log-63.4x *-57 (cos-12x+ 7)^(sin8/cos8x) + 7^-cos(-x^2.24)(log(xsin0.46x))"; //"-sin(-3log(- 3 sin -1.5157 x ^ 3.5x / -x ^ 25.27 log-x)(18x + 3.87)/x^-20)";
            double dbX = -0.395;
            Console.WriteLine($"{str1}");
            Calc expressionCalc = null;
            try
            {
                expressionCalc = new Calc(str1);
                Console.WriteLine($"x = {dbX}\n{str1} = {expressionCalc.CalcResult(dbX)}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            var str2 = expressionCalc?.PrintExpression();
            Console.WriteLine(new Calc(str2.Replace(',', '.')).CalcResult(dbX));
            Console.ReadLine();

            //Console.WriteLine();

            //Test.Test1(new UserList<int>(new int[10]));

            //do
            //{
            //    Console.Clear();
            //    try
            //    {
            //        Console.WriteLine($"Пример ввода:\n{str1}");
            //        Console.WriteLine("Введите выражение:");

            //        var str = Console.ReadLine().Replace(',', '.');
            //        Console.Write("Введите x = ");
            //        if(!Double.TryParse(Console.ReadLine().Replace('.', ','), out double dbX)) {throw new FormatException("x должен быть числом.");}
            //        Console.WriteLine($"{str} = {(new Calc(str, dbX)).Result}");

            //    }
            //    catch (Exception e)
            //    {
            //        Console.ForegroundColor = ConsoleColor.Red;
            //        Console.WriteLine(e.Message);
            //        Console.ResetColor();
            //    }
            //    finally
            //    {
            //        Console.WriteLine("Для выхода нажмите Esc/любую клавишу для продолжения.");
            //    }
            //} while (Console.ReadKey().Key != ConsoleKey.Escape);
        }
    }

    class Calc
    {
        private static readonly char UnoMinus = '-';
        private static readonly char OpenBracket = '(';
        private static readonly char CloseBracket = ')';
        private static readonly char CharVariable = 'x';
        private enum FuncEnum { Nun, Sin, Cos, Log, Min };

        private int _braketsNumber;

        public IOperand OperandResult;

        public Calc(string startValue)
        {
            _braketsNumber = 0;
            var node = new UserList<char>(startValue.Where(ch => ch != ' ')).First;
            OperandResult = StrResult(default(char), ref node, out char value, out int preor);
            //Result = StrResult(default(char), ref node, out char value, out int preor).SetIntValue(xValue);
            if (_braketsNumber != 0) throw new Exception("Не закрыто " + _braketsNumber + " cкобок");
        }

        public double CalcResult(double xValue) => OperandResult.SetIntValue(xValue);
        public string PrintExpression() => OperandResult.Print;


        private IOperand StrResult(char preOperand, ref IValuesAndLink<char> sValue, out char lastOperand, out int lastOperandPreor)
        {
            if (sValue == null)
            {
                lastOperand = default(char);
                lastOperandPreor = -1;
                return null;
            }
            var a = FindValue(ref sValue);
            var operand = FindOperand(ref sValue, out int operandPreor);

            if (operand == CloseBracket || preOperand != default(char) && operandPreor < 2)
            {
                lastOperand = operand;
                lastOperandPreor = operandPreor;
                return a;
            }

            do
            {
                if (operandPreor == 0)
                {
                    var m = StrResult(operand, ref sValue, out lastOperand, out lastOperandPreor);
                    if (m == null && operand == default(char)) return a;
                    var p = DuoOperand(a, m, operand);
                    return p;
                }

                var b = FindValue(ref sValue);
                var nextOperand = FindOperand(ref sValue, out int nextOperandPreor);


                if (operandPreor >= nextOperandPreor)
                {
                    if(b == null && operand == default(char)) break;
                    a = DuoOperand(a, b, operand);
                    operand = nextOperand;
                    operandPreor = nextOperandPreor;
                }
                else
                {
                        a = DuoOperand(a,
                            DuoOperand(b, StrResult(operand, ref sValue, out char tempOperand, out operandPreor),
                                nextOperand), operand);
                        operand = tempOperand;
                }
            } while (operand != CloseBracket && sValue != null);
            lastOperand = default(char);
            lastOperandPreor = -1;
            return a;
        }

        private char FindOperand(ref IValuesAndLink<char> charter, out int intPreor)
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
                    if(charter.Next == null)throw new Exception("Отсутствует слагаемое в конце");
                    charter = charter.Next;
                    intPreor = 0;
                    return charter.Previous.Value;
                case '*':
                case '/':
                    if (charter.Next == null) throw new Exception("Отсутствует множитель в конце");

                    charter = charter.Next;
                    intPreor = 1;
                    return charter.Previous.Value;
                case '^':
                    if (charter.Next == null) throw new Exception("Отсутствует показатель в конце");
                    charter = charter.Next;
                    intPreor = 2;
                    return charter.Previous.Value;
                case ')':
                    if (_braketsNumber == 0) throw new Exception("Лишня скобка");
                    _braketsNumber--;
                    var temp = charter.Value;
                    charter = charter.Next;
                    intPreor = -1;
                    return temp;
                default:
                    intPreor = 1;
                    return '*';
            }
        }
        private IOperand FindValue(ref IValuesAndLink<char> charter)
        {
            if (charter == null)
            {
                return null;
            }
            //if(charter == null) throw new Exception("Закрывайте скобки");
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
                _braketsNumber++;
                aValue = StrResult(default(char), ref charter, out char tempOper, out int tempPreor);
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
                    if (strNumbs == "") throw new Exception("не указан аргумент функции");
                    aValue = new Const(strNumbs == "" ? 1 : Double.Parse(strNumbs.Replace('.', ',')));
                }
                if (blUno2) aValue = UnoOperand(aValue, FuncEnum.Min);
            }
            aValue = UnoOperand(aValue, funcValue);
            if (blUno1) aValue = UnoOperand(aValue, FuncEnum.Min);
            return aValue;
        }

        private void FindFunc(ref IValuesAndLink<char> nodeValue, out FuncEnum funcValue)
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
                        if (strFunc.Length > 0) throw new Exception("Введён неверный оператор");
                        break;
                    }
            }
        }


        IOperand DuoOperand(IOperand a, IOperand b, char operand)
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

        IOperand UnoOperand(IOperand a, FuncEnum operand)
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

        bool NumbersPredicate(char ch) => (ch >= '0') && (ch <= '9') || (ch == '.');
    }

    interface IOperand
    {
        double SetIntValue(double value);
        string Print { get; }
        IOperand Uncover { get; }

    }

    class Const : IOperand
    {
        private readonly double _doubValue;

        public Const(double value)
        {
            _doubValue = value;
        }

        public IOperand Uncover => null;
        public double SetIntValue(double value) => _doubValue;

        public string Print => _doubValue.ToString();
    }

    class Variable : IOperand
    {
        private double _doubValue;
        public IOperand Uncover => null;

        public double SetIntValue(double value)
        {
            _doubValue = value;
            return _doubValue;
        }

        public string Print => "x";
    }

    abstract class UnoMethod : IOperand
    {
        private readonly IOperand _value1;
        private double _resultValue;
        public IOperand Uncover => this; //TODO ??????

        protected UnoMethod(IOperand a)
        {
            _value1 = a;
        }

        protected abstract double Operation(double a);
        protected abstract string PrintOperation(string a);
        public double SetIntValue(double value)
        {
            _resultValue = Operation(_value1.SetIntValue(value));
            return _resultValue;
        }


        public string Print => PrintOperation(_value1.Print);
    }

    class Sin : UnoMethod
    {
        public Sin(IOperand a) : base(a)
        {
        }

        protected override double Operation(double a) => Math.Sin(a);
        protected override string PrintOperation(string a) => "sin(" + a + ")";
    }

    class Cos : UnoMethod
    {
        public Cos(IOperand a) : base(a)
        {
        }

        protected override double Operation(double a) => Math.Cos(a);
        protected override string PrintOperation(string a) => "cos(" + a + ")";
    }

    class Ln : UnoMethod
    {
        public Ln(IOperand a) : base(a)
        {
        }

        protected override double Operation(double a) => Math.Log(a);
        protected override string PrintOperation(string a) => "log(" + a + ")";
    }

    class UnoMin : UnoMethod
    {
        public UnoMin(IOperand a) : base(a)
        {
        }

        protected override double Operation(double a) => -a;
        protected override string PrintOperation(string a) => "-" + a;
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
        public abstract string PrintOperation(string a, string b);

        public double SetIntValue(double value)
        {
            _resultValue = Operation(_value1.SetIntValue(value), _value2.SetIntValue(value));
            return _resultValue;
        }

        public string Print => PrintOperation(_value1.Print, _value2.Print);
    }

    class Min : Method
    {
        public Min(IOperand a, IOperand b) : base(a, b)
        {
        }

        public override double Operation(double a, double b) => (a - b);
        public override string PrintOperation(string a, string b) => "(" + a + ") - (" + b + ")"; // TODO a - (b)
    }

    class Add : Method
    {
        public Add(IOperand a, IOperand b) : base(a, b)
        {
        }

        public override double Operation(double a, double b) => a + b;
        public override string PrintOperation(string a, string b) => "(" + a + ") + (" + b + ")";
    }

    class Umn : Method
    {
        public Umn(IOperand a, IOperand b) : base(a, b)
        {
        }

        public override double Operation(double a, double b) => a * b;
        public override string PrintOperation(string a, string b) => "(" + a + ") * (" + b + ")";
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
        public override string PrintOperation(string a, string b) => "(" + a + ") / (" + b + ")";
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
        public override string PrintOperation(string a, string b) => "(" + a + ") ^ (" + b + ")";

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
            Count++;
            var newIndex = _arr[_lastIndex]._nextIndex;
            var node = (ValuesAndLink) elementNode;

            Removing(_arr[_lastIndex]._nextIndex);
            if (node._index == _lastIndex) _lastIndex = newIndex;

            Count++;
            _arr[newIndex].Value = elenet;
            _arr[newIndex]._enable = true;
            _arr[newIndex]._nextIndex = node._nextIndex;
            node._nextIndex = newIndex;
            _arr[newIndex]._previousIndex = node._index;
        }
        public void AddBefor(IValuesAndLink<T> elementNode, T elenet)
        {
            Count++;
            var newIndex = _arr[_lastIndex]._nextIndex;
            var node = (ValuesAndLink)elementNode;

            Removing(_arr[_lastIndex]._nextIndex);
            if (node._index == _firstIndex) _firstIndex = newIndex;

            Count++;
            node.Previous._nextIndex = newIndex;
            _arr[newIndex].Value = elenet;
            _arr[newIndex]._enable = true;
            _arr[newIndex]._nextIndex = node._index;
            node._previousIndex = newIndex;
            _arr[newIndex]._previousIndex = node._previousIndex;

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
            {
                array[arrayIndex++] = variable.Current;
            }

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