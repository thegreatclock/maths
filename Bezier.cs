using UnityEngine;

namespace GreatClock.Common.Maths {

	public class Bezier {

		public Vector3 p0, p1, p2, p3;
		private Vector3 b0 = Vector3.zero;
		private Vector3 b1 = Vector3.zero;
		private Vector3 b2 = Vector3.zero;
		private Vector3 b3 = Vector3.zero;

		private float Ax, Ay, Az;
		private float Bx, By, Bz;
		private float Cx, Cy, Cz;

		public Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
			this.p0 = p0;
			this.p1 = p1;
			this.p2 = p2;
			this.p3 = p3;
		}

		public Vector3 getPointAt(float t) {
			CheckConstant();
			float t2 = t * t;
			float t3 = t2 * t;
			float x = Ax * t3 + Bx * t2 + Cx * t + p0.x;
			float y = Ay * t3 + By * t2 + Cy * t + p0.y;
			float z = Az * t3 + Bz * t2 + Cz * t + p0.z;
			return new Vector3(x, y, z);
		}

		private void SetConstant() {
			Cx = 3f * p1.x;
			Bx = 3f * ((p3.x + p2.x) - (p0.x + p1.x)) - Cx;
			Ax = p3.x - p0.x - Cx - Bx;
			Cy = 3f * p1.y;
			By = 3f * ((p3.y + p2.y) - (p0.y + p1.y)) - Cy;
			Ay = p3.y - p0.y - Cy - By;
			Cz = 3f * p1.z;
			Bz = 3f * ((p3.z + p2.z) - (p0.z + p1.z)) - Cz;
			Az = p3.z - p0.z - Cz - Bz;
		}

		private void CheckConstant() {
			if (p0 != b0 || p1 != b1 || p2 != b2 || p3 != b3) {
				SetConstant();
				b0 = p0;
				b1 = p2;
				b2 = p2;
				b3 = p3;
			}
		}
	}

}