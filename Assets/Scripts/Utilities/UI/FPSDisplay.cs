﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SanAndreasUnity.Utilities {
	
	public class FPSDisplay : MonoBehaviour {

		private static int fpsTextureWidth = 75;
		private static int fpsTextureHeight = 25;
		private static float fpsMaximum = 60.0f;
		/*private static float fpsGreen = 50.0f;
        private static float fpsRed = 23.0f;*/
		private float fpsDeltaTime = 0.0f;
		private Texture2D fpsTexture = null;
		private Color[] colors = null;
		private float[] fpsHistory = new float[fpsTextureWidth];
		private int fpsIndex = 0;

		private static bool _showFPS = true;

		GUIStyle labelStyle = null;



		void Awake () {

			fpsTexture = new Texture2D(fpsTextureWidth, fpsTextureHeight, TextureFormat.RGBA32, false, true);

			colors = new Color[fpsTexture.width * fpsTexture.height];

		}
		
		void Update () {

			// FPS counting
			fpsDeltaTime += (Time.unscaledDeltaTime - fpsDeltaTime) * 0.1f;

			if (Input.GetKeyDown(KeyCode.F10))
				_showFPS = !_showFPS;
			
			if (_showFPS)
				UpdateTexture(1.0f / fpsDeltaTime);
			
		}

		void UpdateTexture(float fps)
		{

			UnityEngine.Profiling.Profiler.BeginSample("Reset texture pixels");
			int numPixels = fpsTexture.width * fpsTexture.height;
			Color backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.66f); // Half-transparent background for FPS graph
			for (int i = 0; i < numPixels; i++)
				colors[i] = backgroundColor;
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Set pixels");
			fpsTexture.SetPixels(colors);
			UnityEngine.Profiling.Profiler.EndSample();

			// Append to history storage
			fpsHistory[fpsIndex] = fps;

			int f = fpsIndex;

			if (fps > fpsHistory.Average())
				fpsMaximum = fps;

			// Draw graph into texture
			UnityEngine.Profiling.Profiler.BeginSample("Set fps history pixels");
			for (int i = fpsTexture.width - 1; i >= 0; i--)
			{
				float graphVal = (fpsHistory[f] > fpsMaximum) ? fpsMaximum : fpsHistory[f]; //Clamps
				int height = (int)(graphVal * fpsTexture.height / (fpsMaximum + 0.1f)); //Returns the height of the desired point with a padding of 0.1f units

				float p = fpsHistory[f] / fpsMaximum,
				r = Mathf.Lerp(1, 1 - p, p),
				g = Mathf.Lerp(p * 2, p, p);

				fpsTexture.SetPixel(i, height, new Color(r, g, 0));
				f--;

				if (f < 0)
					f = fpsHistory.Length - 1;
			}
			UnityEngine.Profiling.Profiler.EndSample();

			// Next entry in rolling history buffer
			fpsIndex++;
			if (fpsIndex >= fpsHistory.Length)
				fpsIndex = 0;

			UnityEngine.Profiling.Profiler.BeginSample("Apply texture");
			fpsTexture.Apply(false, false);
			UnityEngine.Profiling.Profiler.EndSample();

		}

		void OnGUI() {

			// if (Event.current.type != EventType.Repaint)
			// 	return;

			if (null == labelStyle)
				labelStyle = new GUIStyle("label") { alignment = TextAnchor.MiddleLeft };

			if (_showFPS)
			{
				float msec = fpsDeltaTime * 1000.0f;
				float fps = 1.0f / fpsDeltaTime;

				// Show FPS counter
				Rect rect = GUIUtils.GetCornerRect(ScreenCorner.BottomRight, 100, 25, new Vector2(15 + fpsTexture.width, 10));
				UnityEngine.Profiling.Profiler.BeginSample("Draw label");
				GUI.Label(rect, string.Format("{0:0.}fps ({1:0.0}ms)", fps, msec), labelStyle);
				UnityEngine.Profiling.Profiler.EndSample();
				
				// Show FPS history
				UnityEngine.Profiling.Profiler.BeginSample("Draw texture");
				GUI.DrawTexture(GUIUtils.GetCornerRect(ScreenCorner.BottomRight, fpsTexture.width, fpsTexture.height, new Vector2(5, fpsTexture.height - 15)), fpsTexture);
				UnityEngine.Profiling.Profiler.EndSample();
				
			}

		}

	}

}
