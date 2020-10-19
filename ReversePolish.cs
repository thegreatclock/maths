using System;
using System.Collections.Generic;

namespace GreatClock.Common.Maths {

	/// <summary>
	/// A Math statement analyzer and value calculator.
	/// </summary>
	public sealed class ReversePolish {

		/// <summary>
		/// Method that retrieve the number value of the variable.
		/// </summary>
		/// <param name="variableName">Variable Name</param>
		/// <returns></returns>
		public delegate double VariableGetterDelegate(string variableName);

		/// <summary>
		/// The variable value getter function.
		/// </summary>
		public VariableGetterDelegate variableGetterFunction;

		private static string operator_chars = "+-*/%^";
		private static int[][] priority = new int[][]{
			new int[]{ 0, 0, 1, 1, 1, 1},
			new int[]{ 0, 0, 1, 1, 1, 1},
			new int[]{-1,-1, 0, 0, 0, 1},
			new int[]{-1,-1, 0, 0, 0, 1},
			new int[]{-1,-1, 0, 0, 0, 1},
			new int[]{-1,-1,-1,-1,-1, 0}};
		private static string[] functions = {"abs", "acos", "asin", "atan", "atan2", "ceil", "clamp", "clamp01", "cos", "cosh", "exp", "floor",
		"lerp", "lg", "ln", "log", "max", "min", "pow", "round", "sign", "sin", "sinh", "sqrt", "tan", "tanh"};
		private static int[] function_params = {1, 1, 1, 1, 2, 1, 3, 1, 1, 1, 1, 1,
		3, 1, 1, 2, -1, -1, 2, 1, 1, 1, 1, 1, 1, 1};
		private static string illegal_chars = "~`!@#$&=;:'\"?,<>\\|[]{}";

		private List<_Item> mItems = new List<_Item>();
		private Stack<_Item> mOps = new Stack<_Item>();
		private Stack<_Item> mFuncs = new Stack<_Item>();
		private Stack<double> mResults = new Stack<double>();

		private Dictionary<string, double> mTempVariables = new Dictionary<string, double>(16);

		/// <summary>
		/// Constructor of the <see cref="ReversePolish"/> class.
		/// </summary>
		public ReversePolish() { }

		/// <summary>
		/// Constructor of the <see cref="ReversePolish"/> class.
		/// </summary>
		/// <param name="statement">a math statement contains math functions
		/// and operators together with values and variables</param>
		public ReversePolish(string statement) {
			Init(statement);
		}

		/// <summary>
		/// Set the maths statement to be calculated
		/// </summary>
		/// <param name="statement">a math statement contains math functions
		/// and operators together with values and variables</param>
		public ReversePolish SetStatement(string statement) {
			Init(statement);
			return this;
		}

		/// <summary>
		/// Calculate the statement's value.
		/// </summary>
		public double Calculate() {
			return Calc(null);
		}

		/// <summary>
		/// Calculate the statement's value with given variables.
		/// </summary>
		/// <param name="variables">Variables dictionary.</param>
		public double Calculate(Dictionary<string, double> variables) {
			mTempVariables.Clear();
			foreach (KeyValuePair<string, double> kv in variables) {
				CheckVariableKey(kv.Key);
				mTempVariables.Add(kv.Key, kv.Value);
			}
			double result = Calc(mTempVariables);
			mTempVariables.Clear();
			return result;
		}

		/// <summary>
		/// Calculate the statement's value with given variables.
		/// </summary>
		/// <param name="variables">Variables dictionaries.</param>
		public double Calculate(params Dictionary<string, double>[] variables) {
			mTempVariables.Clear();
			for (int i = 0; i < variables.Length; i++) {
				Dictionary<string, double> variable = variables[i];
				foreach (KeyValuePair<string, double> kv in variable) {
					CheckVariableKey(kv.Key);
					mTempVariables.Add(kv.Key, kv.Value);
				}
			}
			double result = Calc(mTempVariables);
			mTempVariables.Clear();
			return result;
		}

		/// <summary>
		/// Calculate the statement's value with given variables.
		/// </summary>
		/// <param name="variables">Variables.</param>
		public double Calculate(Dictionary<string, float> variables) {
			mTempVariables.Clear();
			foreach (KeyValuePair<string, float> kv in variables) {
				CheckVariableKey(kv.Key);
				mTempVariables.Add(kv.Key, (double)kv.Value);
			}
			double result = Calc(mTempVariables);
			mTempVariables.Clear();
			return result;
		}

		/// <summary>
		/// Calculate the statement's value with given variables.
		/// </summary>
		/// <param name="variables">Variables.</param>
		public double Calculate(params Dictionary<string, float>[] variables) {
			mTempVariables.Clear();
			for (int i = 0; i < variables.Length; i++) {
				Dictionary<string, float> variable = variables[i];
				foreach (KeyValuePair<string, float> kv in variable) {
					CheckVariableKey(kv.Key);
					mTempVariables.Add(kv.Key, (double)kv.Value);
				}
			}
			double result = Calc(mTempVariables);
			mTempVariables.Clear();
			return result;
		}

		private void Init(string statement) {
			mItems.Clear();
			mOps.Clear();
			mFuncs.Clear();
			int len = statement.Length;
			int index = 0;
			char lastChr = '#';
			eItemType lastType = eItemType.None;
			while (index < len) {
				bool isValid = true;
				char chr = statement[index];
				if (OperatorIndex(chr) >= 0) {
					if ((chr == '+' || chr == '-') && lastType == eItemType.None) {
						mItems.Add(_Item.GetNumber(0.0));
					} else if (lastType == eItemType.None || lastType == eItemType.Operator) {
						throw new ReversePolishException(string.Format("unexpected operator '{0}', at index : {1} !", chr, index), statement, index);
					} else if (mOps.Count > 0) {
						_Item peek = mOps.Peek();
						if (peek.type == eItemType.Operator) {
							int op1 = OperatorIndex(peek.operate);
							int op2 = OperatorIndex(chr);
							if (op1 >= 0 && priority[op1][op2] <= 0) {
								while (mOps.Count > 0) {
									mItems.Add(mOps.Pop());
									if (mOps.Count <= 0) { break; }
									peek = mOps.Peek();
									if (peek.type != eItemType.Operator) { break; }
									op1 = OperatorIndex(peek.operate);
									if (op1 < 0) { break; }
									if (priority[op2][op1] < 0) { break; }
								}
							}
						}
					}
					mOps.Push(_Item.GetOperator(chr));
					lastType = eItemType.Operator;
					index++;
				} else if (chr == '(') {
					if (lastType == eItemType.Function) {
						mFuncs.Push(mOps.Peek());
					} else {
						mOps.Push(_Item.GetFunction(null, 0));
						mFuncs.Push(_Item.GetFunction(null, 0));
					}
					mOps.Push(_Item.GetOperator('('));
					lastType = eItemType.None;
					index++;
				} else if (chr == ')') {
					if (lastChr == '(') {
						throw new ReversePolishException(string.Format("there is nothing between '(' and ')', at index : {0} !", index), statement, index);
					}
					if (lastType == eItemType.Operator || lastType == eItemType.None) {
						throw new ReversePolishException(string.Format("unexpected ')', at index : {0} !", index), statement, index);
					}
					while (true) {
						if (mOps.Count <= 0) {
							throw new ReversePolishException(string.Format("unexpected ')', at index : {0} !", index), statement, index);
						}
						_Item sOp = mOps.Pop();
						if (sOp.type == eItemType.Operator && sOp.operate == '(') { break; }
						mItems.Add(sOp);
					}
					_Item func = mFuncs.Pop();
					if (mOps.Count > 0 && mOps.Pop().type == eItemType.Function) {
						mItems.Add(func);
						lastType = eItemType.Function;
					} else {
						lastType = eItemType.Variable;
					}
					if (func.function != null && func.funParamNum > 0 && func.funParamNum != func.funParam) {
						throw new ReversePolishException(string.Format("function '{0}' requires {1} parameters !", func.function, func.funParamNum), statement, index);
					}
					index++;
				} else if (chr == ',') {
					if (lastType == eItemType.Operator || lastType == eItemType.None) {
						throw new ReversePolishException(string.Format("unexpected ',', at index : {0} !", index), statement, index);
					}
					_Item func = mFuncs.Count > 0 ? mFuncs.Pop() : _Item.GetFunction(null, 0);
					if (func.function == null) {
						throw new ReversePolishException(string.Format("unexpected ',', at index : {0} !", index), statement, index);
					}
					while (true) {
						_Item sOp = mOps.Peek();
						if (sOp.type == eItemType.Operator && sOp.operate == '(') { break; }
						mItems.Add(mOps.Pop());
					}
					func.funParam++;
					mFuncs.Push(func);
					lastType = eItemType.None;
					index++;
				} else {
					string str = GetSegment(statement, ref index);
					if (str.Length > 0) {
						if (lastType == eItemType.Number || lastType == eItemType.Variable || lastType == eItemType.Function) {
							throw new ReversePolishException(string.Format("an operator is expected, at index : {0} !", index), statement, index);
						}
						int funcIndex = FunctionIndex(str);
						if (funcIndex >= 0) {
							if (GetNextChar(statement, index - 1) != '(') {
								throw new ReversePolishException(string.Format("no '(' followed by function : {0}, at index : {1} ！", str, index), statement, index);
							}
							mOps.Push(_Item.GetFunction(str, function_params[funcIndex]));
							lastType = eItemType.Function;
						} else if (IsNumber(str[0])) {
							double num;
							if (!double.TryParse(str, out num)) {
								throw new ReversePolishException(string.Format("unparsable number : {0}, at index : {1} !", str, (index - str.Length)), statement, index - str.Length);
							}
							mItems.Add(_Item.GetNumber(num));
							lastType = eItemType.Number;
						} else {
							mItems.Add(_Item.GetVariable(str));
							lastType = eItemType.Variable;
						}
					} else {
						isValid = false;
					}
				}
				if (isValid) { lastChr = statement[index - 1]; }
			}
			if (lastType == eItemType.Operator) {
				throw new ReversePolishException("statement end with operator", statement, -1);
			}
			if (mFuncs.Count > 0) {
				throw new ReversePolishException("one or more ')' required in the statement", statement, -1);
			}
			while (mOps.Count > 0) {
				mItems.Add(mOps.Pop());
			}
		}

		private double Calc(Dictionary<string, double> variables) {
			if (mItems.Count <= 0) { return 0.0; }
			mResults.Clear();
			for (int i = 0, imax = mItems.Count; i < imax; i++) {
				_Item item = mItems[i];
				switch (item.type) {
					case eItemType.Number:
						mResults.Push(item.number);
						break;
					case eItemType.Variable:
						double val = double.NaN;
						string variable = item.variable;
						if (variable == "E") {
							val = Math.E;
						} else if (variable == "PI") {
							val = Math.PI;
						} else if (variable == "rand") {
							val = random;
						} else if (variableGetterFunction != null) {
							val = variableGetterFunction(variable);
						} else if (!variables.TryGetValue(variable, out val)) {
							val = double.NaN;
						}
						if (double.IsNaN(val)) {
							throw new Exception(string.Format("variable '{0}' not found ...  ", variable));
						}
						mResults.Push(val);
						break;
					case eItemType.Operator:
						double num2 = mResults.Pop();
						double num1 = mResults.Pop();
						switch (item.operate) {
							case '+':
								mResults.Push(num1 + num2);
								break;
							case '-':
								mResults.Push(num1 - num2);
								break;
							case '*':
								mResults.Push(num1 * num2);
								break;
							case '/':
								mResults.Push(num1 / num2);
								break;
							case '%':
								mResults.Push(num1 % num2);
								break;
							case '^':
								mResults.Push(Math.Pow(num1, num2));
								break;
						}
						break;
					case eItemType.Function:
						double[] parameters = GetDoubles();
						for (int j = item.funParam - 1; j >= 0; j--) {
							parameters[j] = mResults.Pop();
						}
						switch (item.function) {
							case "abs":
								mResults.Push(Math.Abs(parameters[0]));
								break;
							case "acos":
								mResults.Push(Math.Acos(parameters[0]));
								break;
							case "asin":
								mResults.Push(Math.Asin(parameters[0]));
								break;
							case "atan":
								mResults.Push(Math.Atan(parameters[0]));
								break;
							case "atan2":
								mResults.Push(Math.Atan2(parameters[0], parameters[1]));
								break;
							case "ceil":
								mResults.Push(Math.Ceiling(parameters[0]));
								break;
							case "clamp":
								double cmin = parameters[1];
								double cmax = parameters[2];
								if (cmin > cmax) {
									cmin = parameters[2];
									cmax = parameters[1];
								}
								mResults.Push(parameters[0] < cmin ? cmin : (parameters[0] > cmax ? cmax : parameters[0]));
								break;
							case "clamp01":
								mResults.Push(parameters[0] < 0f ? 0f : (parameters[0] > 1f ? 1f : parameters[0]));
								break;
							case "cos":
								mResults.Push(Math.Cos(parameters[0]));
								break;
							case "cosh":
								mResults.Push(Math.Cosh(parameters[0]));
								break;
							case "exp":
								mResults.Push(Math.Exp(parameters[0]));
								break;
							case "floor":
								mResults.Push(Math.Floor(parameters[0]));
								break;
							case "lerp":
								mResults.Push((parameters[1] - parameters[0]) * parameters[2] + parameters[0]);
								break;
							case "lg":
								mResults.Push(Math.Log10(parameters[0]));
								break;
							case "ln":
								mResults.Push(Math.Log(parameters[0]));
								break;
							case "log":
								mResults.Push(Math.Log(parameters[0], parameters[1]));
								break;
							case "max":
								double max = double.MinValue;
								for (int k = 0; k < item.funParam; k++) {
									if (max < parameters[k]) { max = parameters[k]; }
								}
								mResults.Push(max);
								break;
							case "min":
								double min = double.MaxValue;
								for (int l = 0; l < item.funParam; l++) {
									if (min > parameters[l]) { min = parameters[l]; }
								}
								mResults.Push(min);
								break;
							case "pow":
								mResults.Push(Math.Pow(parameters[0], parameters[1]));
								break;
							case "round":
								mResults.Push(Math.Round(parameters[0]));
								break;
							case "sign":
								mResults.Push((double)Math.Sign(parameters[0]));
								break;
							case "sin":
								mResults.Push(Math.Sin(parameters[0]));
								break;
							case "sinh":
								mResults.Push(Math.Sinh(parameters[0]));
								break;
							case "sqrt":
								mResults.Push(Math.Sqrt(parameters[0]));
								break;
							case "tan":
								mResults.Push(Math.Tan(parameters[0]));
								break;
							case "tanh":
								mResults.Push(Math.Tanh(parameters[0]));
								break;
							default:
								mResults.Push(parameters[0]);
								break;
						}
						CacheDoubles(parameters);
						break;
				}
			}
			return mResults.Peek();
		}

		private void CheckVariableKey(string key) {
			bool ok = true;
			if (key == "E" || key == "PI" || key == "rand") {
				ok = false;
			} else if (FunctionIndex(key) >= 0) {
				ok = false;
			}
			if (!ok) { throw new Exception("invalid variable name : " + key); }
		}

		private int FunctionIndex(string str) {
			for (int i = 0, imax = functions.Length; i < imax; i++) {
				if (functions[i] == str) { return i; }
			}
			return -1;
		}

		private int OperatorIndex(char chr) {
			return operator_chars.IndexOf(chr);
		}
		private bool IsNumber(char chr) {
			return chr >= '0' && chr <= '9';
		}

		private string GetSegment(string str, ref int index) {
			int length = str.Length;
			int len = 0;
			int from = -1;
			while (index < length) {
				bool done = false;
				char chr = str[index];
				if (chr == ' ' || chr == '\t') {
					if (len > 0) {
						done = true;
					}
				} else if (OperatorIndex(chr) >= 0 || chr == '(' || chr == ')' || chr == ',') {
					done = true;
				} else if ((int)chr > 127 || illegal_chars.Contains(chr.ToString())) {
					throw new ReversePolishException(string.Format("unexpected char '{0}', at index : {1} ！", chr, index), str, index);
				} else {
					if (from < 0) { from = index; }
					len++;
				}
				if (done) { break; }
				index++;
			}
			return len <= 0 ? string.Empty : str.Substring(from, len);
		}

		private char GetNextChar(string str, int index) {
			int len = str.Length;
			while (++index < len) {
				char chr = str[index];
				if (chr == ' ' || chr == '\t') { continue; }
				return chr;
			}
			return ' ';
		}

		private static int random_seed = 0;
		private double random {
			get {
				Random rand = new Random(random_seed++);
				if (random_seed > 1048576) { random_seed = 0; }
				return rand.NextDouble();
			}
		}

		private static Queue<double[]> cached_doubles = new Queue<double[]>(8);
		private static double[] GetDoubles() {
			lock (cached_doubles) { if (cached_doubles.Count > 2) { return cached_doubles.Dequeue(); } }
			return new double[32];
		}
		private static void CacheDoubles(double[] doubles) {
			if (doubles == null) { return; }
			lock (cached_doubles) { cached_doubles.Enqueue(doubles); }
		}

		private enum eItemType {
			None, Number, Operator, Variable, Function
		}

		private struct _Item {

			public eItemType type;
			public double number;
			public char operate;
			public string variable;
			public string function;
			public int funParam;
			public int funParamNum;

			public static _Item GetNumber(double number) {
				_Item item = new _Item();
				item.type = eItemType.Number;
				item.number = number;
				return item;
			}

			public static _Item GetOperator(char operate) {
				_Item item = new _Item();
				item.type = eItemType.Operator;
				item.operate = operate;
				return item;
			}

			public static _Item GetVariable(string variable) {
				_Item item = new _Item();
				item.type = eItemType.Variable;
				item.variable = variable;
				return item;
			}

			public static _Item GetFunction(string function, int funParamNum) {
				_Item item = new _Item();
				item.type = eItemType.Function;
				item.function = function;
				item.funParamNum = funParamNum;
				item.funParam = 1;
				return item;
			}

		}

	}

	/// <summary>
	/// Reverse Polish Exception.
	/// </summary>
	public class ReversePolishException : Exception {

		private string mStatement;
		private int mIndex;

		/// <summary>
		/// Constructor of ReversePolishExcpetion.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="statement">Maths statement.</param>
		/// <param name="index">Index of statement where exception occurs.</param>
		public ReversePolishException(string message, string statement, int index) : base(message) {
			mStatement = statement;
			mIndex = index;
		}
		/// <summary>
		/// Maths statement.
		/// </summary>
		public string statement { get { return mStatement; } }

		/// <summary>
		/// Index of statement where exception occurs.
		/// </summary>
		public int index { get { return mIndex; } }

	}

}