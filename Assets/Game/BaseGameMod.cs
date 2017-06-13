﻿// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using AmarokGames.Grids;

namespace AmarokGames.GridGame {

    public class BaseGameMod {

        public const string CoreGameModId = "CoreGame";

        public LayerId SolidLayerBool { get; private set; }
        public LayerId TileForegroundLayerUInt { get; private set; }
        public LayerId TileBackgroundLayerUInt { get; private set; }
        public LayerId TerrainGenDebugLayerFloat { get; private set; }

        public Tile TileEmpty { get; private set; }
        public Tile TileStone { get; private set; }
        public Tile TileDirt { get; private set; }
        public Tile TileGrass { get; private set; }

        public void Init(ref LayerConfig layers, TileRegistry tileRegistry, List<IGameSystem> gameSystems) {

            SolidLayerBool = layers.AddLayer("solid", BufferType.Boolean);
            TileForegroundLayerUInt = layers.AddLayer("tileforeground", BufferType.UnsignedInt32);
            TileBackgroundLayerUInt = layers.AddLayer("tilebackground", BufferType.UnsignedInt32);
            TerrainGenDebugLayerFloat = layers.AddLayer("terrainGenDebugLayer", BufferType.Float);

            RegisterTiles(tileRegistry);
        }

        public void PostInit(TileRegistry tileRegistry, List<IGameSystem> gameSystems) {
            RegisterGameSystems(tileRegistry, gameSystems);
        }

        private void RegisterTiles(TileRegistry tileRegistry) {
            TileEmpty = new Tile();
            TileEmpty.CollisionSolid = false;
            TileEmpty.BatchedRendering = false;
            TileEmpty.HumanName = "Empty";
            Texture2D emptyTex = Resources.Load<Texture2D>("Tiles/tile-empty");

            TileStone = new Tile();
            TileStone.CollisionSolid = true;
            TileStone.BatchedRendering = true;
            TileStone.HumanName = "Stone";
            Texture2D stoneTex = Resources.Load<Texture2D>("Tiles/tile-stone");

            Tile concrete = new Tile();
            concrete.CollisionSolid = true;
            concrete.BatchedRendering = true;
            concrete.HumanName = "Concrete";
            Texture2D concreteTex = Resources.Load<Texture2D>("Tiles/tile-concrete");

            TileDirt = new Tile();
            TileDirt.CollisionSolid = true;
            TileDirt.BatchedRendering = true;
            TileDirt.HumanName = "Dirt";
            Texture2D dirtTex = Resources.Load<Texture2D>("Tiles/tile-dirt");

            TileGrass = new Tile();
            TileGrass.CollisionSolid = true;
            TileGrass.BatchedRendering = true;
            TileGrass.HumanName = "Grass";
            Texture2D grassTex = Resources.Load<Texture2D>("Tiles/tile-grass");

            Tile gravel = new Tile();
            gravel.CollisionSolid = true;
            gravel.BatchedRendering = true;
            gravel.HumanName = "Gravel";
            Texture2D gravelTex = Resources.Load<Texture2D>("Tiles/tile-gravel");

            Tile sand = new Tile();
            sand.CollisionSolid = true;
            sand.BatchedRendering = true;
            sand.HumanName = "Sand";
            Texture2D sandTex = Resources.Load<Texture2D>("Tiles/tile-sand");

            Tile wood = new Tile();
            wood.CollisionSolid = true;
            wood.BatchedRendering = true;
            wood.HumanName = "Wood";
            Texture2D woodTex = Resources.Load<Texture2D>("Tiles/tile-wood");

            tileRegistry.RegisterTile(CoreGameModId, "empty", TileEmpty, emptyTex);
            tileRegistry.RegisterTile(CoreGameModId, "stone", TileStone, stoneTex);
            tileRegistry.RegisterTile(CoreGameModId, "concrete", concrete, concreteTex);
            tileRegistry.RegisterTile(CoreGameModId, "dirt", TileDirt, dirtTex);
            tileRegistry.RegisterTile(CoreGameModId, "grass", TileGrass, grassTex);
            tileRegistry.RegisterTile(CoreGameModId, "gravel", gravel, gravelTex);
            tileRegistry.RegisterTile(CoreGameModId, "sand", sand, sandTex);
            tileRegistry.RegisterTile(CoreGameModId, "wood", wood, woodTex);
        }

        private void RegisterGameSystems(TileRegistry tileRegistry, List<IGameSystem> gameSystems) {
            {
                // Solid Renderer
                Shader shader = Shader.Find("Sprites/Default");
                Material mat = new Material(shader);
                mat.color = new Color(1, 1, 1, 0.5f);
                GridSolidRendererSystem solidRenderer = GridSolidRendererSystem.Create(mat, SolidLayerBool);
                gameSystems.Add(solidRenderer);
                solidRenderer.Enabled = false;
            }

            {
                Shader shader = Shader.Find("Sprites/Default");
                Material mat = new Material(shader);
                mat.color = new Color(1, 1, 1, 0.5f);
                BufferVisualiserFloat sys = BufferVisualiserFloat.Create(mat, TerrainGenDebugLayerFloat);
                gameSystems.Add(sys);
                sys.Enabled = false;
            }

            {
                // Tile Renderer
                Texture2D textureAtlas = tileRegistry.GetAtlas().GetTexture();

                Shader foregroundShader = Shader.Find("Unlit/Transparent Cutout");
                Material foregroundMaterial = new Material(foregroundShader);
                foregroundMaterial.mainTexture = textureAtlas;
                foregroundMaterial.color = new Color(1, 1, 1, 1);

                Shader backgroundShader = Shader.Find("Sprites/Default");
                Material backgroundMaterial = new Material(backgroundShader);
                backgroundMaterial.mainTexture = textureAtlas;
                backgroundMaterial.color = new Color(0.5f, 0.5f, 0.5f, 1);

                int tileCount = tileRegistry.GetTileCount();
                TileRenderData[] tileData = new TileRenderData[tileCount];
                for (int i = 0; i < tileCount; ++i) {
                    Tile tile = tileRegistry.GetTileById(i);

                    TileRenderData renderData = new TileRenderData();
                    renderData.draw = tile.BatchedRendering;
                    renderData.zLayer = (ushort)i;
                    renderData.variants = TileRegistry.GetTileVariants(tile.SpriteUV);

                    tileData[i] = renderData;
                }

                gameSystems.Add(GridTileRenderSystem.Create(tileData, foregroundMaterial, TileForegroundLayerUInt, 0));
                gameSystems.Add(GridTileRenderSystem.Create(tileData, backgroundMaterial, TileBackgroundLayerUInt, 1));
            }

            gameSystems.Add(GridCollisionSystem.Create(SolidLayerBool));

            WorldManagementSystem worldMgr = WorldManagementSystem.Create(tileRegistry, SolidLayerBool, TileForegroundLayerUInt, TileBackgroundLayerUInt);
            gameSystems.Add(worldMgr);

            PlayerSystem playerSys = PlayerSystem.Create(tileRegistry);
            gameSystems.Add(playerSys);

            //GridEditorSystem gridEditor = GridEditorSystem.Create(tileRegistry, worldMgr, playerSys.LocalPlayer);
            //gameSystems.Add(gridEditor);

            PlayerInventoryUISystem inventoryUI = PlayerInventoryUISystem.Create(playerSys.LocalPlayer);
            gameSystems.Add(inventoryUI);
        }

        public WorldGenerator GetWorldGenerator(TileRegistry tileReg) {
            WorldGenerator worldGen = new WorldGenerator(SolidLayerBool, TileForegroundLayerUInt, TileBackgroundLayerUInt, TerrainGenDebugLayerFloat, 
                tileReg.GetTileId(TileEmpty), tileReg.GetTileId(TileStone), tileReg.GetTileId(TileDirt), tileReg.GetTileId(TileGrass)
                );
            return worldGen;
        }
    }
}
