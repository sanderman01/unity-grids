﻿// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;
using UnityEngine;

namespace AmarokGames.GridGame {

    /// <summary>
    /// A 2D orthographic camera helper.
    /// </summary>
    public class Camera2D : MonoBehaviour {

        [SerializeField]
        private Transform target;
        public Transform Target {
            get { return target; }
            set { target = value; }
        }

        [SerializeField]
        private int zoomLevel = 4;
        public int ZoomLevel {
            get { return zoomLevel; }
            set {
                const int minZoom = 1;
                const int maxZoom = 10;
                zoomLevel = Math.Min(Math.Max(minZoom, value), maxZoom);
                pixelsPerMeter = zoomLevel * zoomLevel;
            }
        }

        [SerializeField]
        private int pixelsPerMeter = 16; // Eg. setting this to 32 means that an area of 1 meter squared will occupy 32x32 of screen real estate.

        private Camera cam;
        private const float damping = 1;

        void Start() {
            cam = GetComponent<Camera>();
        }

        void Update() {
            cam.orthographicSize = Screen.height / (pixelsPerMeter * 2f);

            if(target != null) {
                Vector3 targetPos = target.position;
                targetPos.z = transform.position.z;

                Vector3 v = Vector3.Lerp(transform.position, targetPos, damping * Time.deltaTime);
                v = Snap(v, pixelsPerMeter);
                transform.position = v + (0.1f/pixelsPerMeter) * new Vector3(1,1,0);
            }

            // Scrollwheel zoom
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            if(screenRect.Contains(Input.mousePosition)) {
                Vector2 scrollDelta = Input.mouseScrollDelta;
                if (scrollDelta.y > 0) {
                    ZoomIn();
                } else if (scrollDelta.y < 0) {
                    ZoomOut();
                }
            }
        }

        private static Vector3 Snap(Vector3 v, int pixelsPerMeter) {
            return new Vector3(
                Snap(v.x, pixelsPerMeter),
                Snap(v.y, pixelsPerMeter),
                Snap(v.z, pixelsPerMeter)
                );
        }

        private static float Snap(float x, int pixelsPerMeter) {
            return Mathf.Round(x * pixelsPerMeter) / pixelsPerMeter;
        }

        [ContextMenu("ZoomIn")]
        public void ZoomIn() {
            ZoomLevel++;
        }

        [ContextMenu("ZoomOut")]
        public void ZoomOut() {
            ZoomLevel--;
        }
    }
}