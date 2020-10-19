using System;

namespace GreatClock.Common.Maths {

	public static class EquationCalc {

		public static double[] CalcRoots(params double[] coefficient) {
			if (coefficient == null || coefficient.Length < 2) {
				return null;
			}
			int n = (int)Math.Sqrt((double)coefficient.Length);
			if (coefficient.Length != n * (n + 1)) {
				// invalid param amount ! should be (n + 1) x n
				return null;
			}

			int i = n - 1;
			int j = 0;

			//LogMatrix(coefficient);
			while (i > 0) {
				// to zero
				int l = j * (n + 1);
				double v = coefficient[l + i];
				if (v != 0.0) {
					bool flag = false;
					for (int jj = j + 1; jj <= i; jj++) {
						int ll = jj * (n + 1);
						double vv = coefficient[ll + i];
						if (vv == 0.0) { continue; }
						for (int ii = 0; ii <= n; ii++) {
							coefficient[l + ii] = coefficient[l + ii] * vv - coefficient[ll + ii] * v;
						}
						flag = true;
						break;
					}
					if (!flag) {
						for (int ii = 0; ii <= n; ii++) {
							int ll = i * (n + 1);
							double vv = coefficient[ll + ii];
							coefficient[ll + ii] = coefficient[l + ii];
							coefficient[l + ii] = vv; ;
						}
					}
				}
				j++;
				if (j >= i) {
					i--;
					j = 0;
				}
				//LogMatrix(coefficient);
			}

			i = 0;
			j = 0;
			while (i < n) {
				int l = j * (n + 1);
				double v = coefficient[l + i];
				if (v == 0.0) {
					return null;
				}
				v = 1.0 / v;
				for (int ii = 0; ii <= n; ii++) {
					coefficient[l + ii] *= v;
				}
				i++;
				j++;
			}
			//LogMatrix(coefficient);

			double[] roots = new double[n];
			for (i = 0; i < n; i++) {
				double v = coefficient[i * (n + 1) + n];
				roots[i] = v;
				for (j = i + 1; j < n; j++) {
					int l = j * (n + 1);
					coefficient[l + n] -= coefficient[l + i] * v;
					coefficient[l + i] = 0.0;
				}
				//LogMatrix(coefficient);
			}
			return roots;
		}
		/*
		private static void LogMatrix(double[] m) {
			if (m == null || m.Length < 2) {
				Debug.Log("Invalid matrix");
				return;
			}
			int n = (int)Math.Sqrt((double)m.Length);
			if (m.Length != n * (n + 1)) {
				Debug.Log("Invalid matrix  " + n + "    " + m.Length);
				return;
			}
			string[] ls = new string[n];
			System.Text.StringBuilder ln = new System.Text.StringBuilder();
			for (int j = 0; j < n; j++) {
				int l = (n + 1) * j;
				for (int i = 0; i < n; i++) {
					double v = m[l + i];
					if (ln.Length > 0) {
						ln.AppendFormat(" {0} {1} * X{2}", v >= 0.0 ? "+" : "-", Math.Abs(v), i);
					} else {
						ln.AppendFormat("{0} * X{1}", v, i);
					}
				}
				ln.Append(" = ");
				ln.Append(m[l + n]);
				ls[j] = ln.ToString();
				ln.Length = 0;
			}
			Debug.Log(string.Join("\n", ls));
		}
		*/
	}

}