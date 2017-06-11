﻿using UnityEngine;

namespace AmarokGames.GridGame {
    class GridEditorSystem : GameSystemBase, IGameSystem {

        private TileRegistry tileRegistry;
        private WorldManagementSystem worldMgr;
        private int tileSelection = 1;

        public static GridEditorSystem Create(TileRegistry tileRegistry, WorldManagementSystem worldMgr) {
            GridEditorSystem sys = GridEditorSystem.Create<GridEditorSystem>();
            sys.tileRegistry = tileRegistry;
            sys.worldMgr = worldMgr;
            return sys;
        }

        public override void TickWorld(World world, int tickRate) {
        }

        public override void UpdateWorld(World world, float deltaTime) {
            const int buttonLeft = 0;
            const int buttonRight = 1;
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

            if (Input.GetMouseButton(buttonLeft)) {
                worldMgr.PlaceTile(world, mouseWorldPos, 0);
            } else if (Input.GetMouseButton(buttonRight)) {
                worldMgr.PlaceTile(world, mouseWorldPos, (ushort)tileSelection);
            }
        }

        void OnGUI() {
            float windowHeight = 80;
            Rect windowSize = new Rect(0, Screen.height - windowHeight, Screen.width * 0.5f, windowHeight);
            Vector2 center = windowSize.center;
            center.x = Screen.width * 0.5f;
            windowSize.center = center;
            GUI.Window(0, windowSize, OnGUIHotbar, "Hotbar");
        }

        private void OnGUIHotbar(int id) {
            Vector2 startOffset = new Vector2(10, 15);
            Vector2 iconSize = new Vector2(64, 64);
            float margin = 5;

            for (int i = 1; i < tileRegistry.GetTileCount(); i++) {
                Rect iconPosition = new Rect(startOffset, iconSize);
                iconPosition.x += (iconSize.x + margin) * (i - 1);
                Tile tile = tileRegistry.GetTileById(i);
                Rect iconUV = tile.IconUV[0];
                bool click = IconButton(iconPosition, tileRegistry.GetAtlas().GetTexture(), iconUV);

                if (click) {
                    Debug.Log("Selected tile: " + i);
                    tileSelection = i;
                }
            }
        }

        private static bool IconButton(Rect position, Texture2D texture, Rect uvArea) {
            GUI.DrawTextureWithTexCoords(position, texture, uvArea);
            Event e = Event.current;
            return e.isMouse && e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 1 && position.Contains(e.mousePosition);
        }

        protected override void Disable() {
        }

        protected override void Enable() {
        }
    }
}
