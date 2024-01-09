﻿using UnityEngine;


namespace IvoryIcicles.Testing
{
	public class CallGenerator : MonoBehaviour
	{
		public Switchboard switchboard;


		private float elapsedTime = 0f;

		private void Update()
		{
			if (elapsedTime > 1.5f)
			{
				elapsedTime = 0f;
				//switchboard.PublishConnectionRequest(Random.Range(0, switchboard.boardButtons.Length));
				return;
			}
			elapsedTime += Time.deltaTime;
		}
	}
}