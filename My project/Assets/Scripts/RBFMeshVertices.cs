using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.Numerics.Matrices;
using System.Linq;
using System.Threading.Tasks;

public class RBFMeshVertices
{
	public Vector3[] meshPoints;
	public List<Vector3> supportPoints;
	public List<double>[] deltas = new List<double>[3];
	/*
		deltas[0] = deltaX 
		deltas[0] = deltaY
		deltas[0] = deltaZ
	*/

	// Function matrices
	private SquareMatrix QS; // SupportPoints x SupportPoints

	// Weights
	private ColumnVector[] Ws = new ColumnVector[3];
	/*
		Ws[0] = Wx
		Ws[1] = Wy
		Ws[2] = Wz
	*/


	public RBFMeshVertices(List<Vector3> points, Vector3[] vertices)
	{
		supportPoints = points;
		meshPoints = vertices;
	}

	// Basis Function
	private float Phi(Vector3 p, Vector3 q)
	{
		float dist = Vector3.Distance(p, q);
		return Mathf.Exp(-dist);
	}

	// Calculates QS * x = deltaS
	private ColumnVector SolveWeightByCoordinate(List<double> delta)
	{
		LUDecomposition lud = QS.LUDecomposition();
		ColumnVector deltaSV = new ColumnVector(delta);
		ColumnVector x = lud.Solve(deltaSV);
		return x;
	}
	
	private void CalculateWeights(List<Vector3> delta)
	{
		int S = supportPoints.Count;
		QS = new SquareMatrix(S);
		Parallel.For(0, S, i =>
		{
			for (int j = 0; j < S; j++)
			{
				QS[i, j] = Phi(supportPoints[i], supportPoints[j]);
			}
		});
		
		deltas[0] = delta.Select(v => (double)v.x).ToList();
		deltas[1] = delta.Select(v => (double)v.y).ToList();
		deltas[2] = delta.Select(v => (double)v.z).ToList();
		
		// Solves in parallel wieghts for each coordinate
		Parallel.For(0, 3, idx =>
		{
			Ws[idx] = SolveWeightByCoordinate(deltas[idx]);
		});
	}

	private List<Vector3> GetDelta(List<Vector3> newPositionsSP)
	{
		List<Vector3> delta = newPositionsSP.Zip(supportPoints, (C, O) => C - O).ToList();
		return delta;
	}

	public void RecalculateMesh(List<Vector3> newPositionsSP)
	{
		List<Vector3> delta = GetDelta(newPositionsSP);
		CalculateWeights(delta);
			
		Parallel.For(0, meshPoints.Length, i =>
		{
			Vector3 deltaV = new Vector3(0, 0, 0);

			for (int j = 0; j < supportPoints.Count; j++)
			{
				Vector3 weights = new Vector3((float)Ws[0][j], (float)Ws[1][j], (float)Ws[2][j]);

				deltaV += weights * Phi(meshPoints[i], supportPoints[j]);
			}
			/*
			Parallel.ForEach(supportPoints, (supportPoint, _, jl) =>
			{
				int j = (int)jl;
				Vector3 weights = new Vector3((float)Ws[0][j], (float)Ws[1][j], (float)Ws[2][j]);

				deltaV += weights * Phi(meshPoint, supportPoint);
			});
			*/
			meshPoints[i] += deltaV;
		});
		 
		supportPoints = newPositionsSP;
	}
}
