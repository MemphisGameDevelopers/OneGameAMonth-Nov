//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class Region : MonoBehaviour, VoxelStream
{
		public GameObject chunk;
		public GameObject tree;
		public Chunk[,,] chunks;
		public ItemChunk[,,] itemChunks;
		public byte[,,] data;
		public  int regionXZ = 32;
		public   int regionY = 32;
		public int offsetX;
		public int offsetZ;
		public float distToLoad;
		public float distToUnload;
		public bool isDirty;
		private static VoxelWorld world;

		public static void setWorld (VoxelWorld w)
		{
				world = w;
		}

		public int getBlockOffsetX ()
		{
				return offsetX * regionXZ;
		}

		public int getBlockOffsetZ ()
		{
				return offsetZ * regionXZ;
		}

		public void saveRegion ()
		{
				try {
						if (!Directory.Exists ("C:/Data/")) {
								Directory.CreateDirectory ("C:/Data/");
						}
						string fileNamePath = "C:/Data/" + this.hashString () + ".dat";
						print ("Saving to " + fileNamePath);
						FileStream fs = File.Create (fileNamePath);
						BinaryWriter writer = new BinaryWriter (fs);
						for (int x=0; x<regionXZ; x++) {
								for (int z=0; z<regionXZ; z++) {
										for (int y=0; y<regionY; y++) {
												writer.Write (data [x, y, z]);
										}
								}
						}	
						isDirty = false;
						writer.Close ();
				} catch (Exception ex) {
						Debug.Log (ex.ToString ());
				}
		}

		public void create ()
		{
				data = new byte[regionXZ, regionY, regionXZ];
				chunks = new Chunk[Mathf.FloorToInt (regionXZ / Chunk.chunkSize),
		                  Mathf.FloorToInt (regionY / Chunk.chunkSize),
		                   Mathf.FloorToInt (regionXZ / Chunk.chunkSize)];
				itemChunks = new ItemChunk[Mathf.FloorToInt (regionXZ / Chunk.chunkSize),
		                           Mathf.FloorToInt (regionY / Chunk.chunkSize),
		                           Mathf.FloorToInt (regionXZ / Chunk.chunkSize)];

				
				try {
						string fileNamePath = "C:/Data/" + this.hashString () + ".dat";

						if (File.Exists (fileNamePath)) {
								//Load in data from file if found
								FileStream fs = File.OpenRead (fileNamePath);
								BinaryReader reader = new BinaryReader (fs);
								for (int x=0; x<regionXZ; x++) {
										for (int z=0; z<regionXZ; z++) {
												for (int y=0; y<regionY; y++) {
														data [x, y, z] = reader.ReadByte ();
												}
										}
								}	
								reader.Close ();
						} else {
								//create this region's terrain data.
								createFlatBiome ();
								//createPerlin ();
								
								//Create a dungeon and put it in the scene.
								GameObject dungeonGO = Instantiate (Resources.Load ("Voxel Generators/Dungeon Generator")) as GameObject;
								VoxelStream dungeon = dungeonGO.GetComponent (typeof(VoxelStream)) as VoxelStream;
								dungeon.create ();
								merge (dungeon, this);


						}
				} catch (Exception ex) {
						Debug.Log (ex.ToString ());
				}

				//create Trees
				createTrees ();
		}

		private void merge (VoxelStream source, VoxelStream destination)
		{
				byte[,,] sourceData = source.GetAllBlocks ();
				byte[,,] destData = destination.GetAllBlocks ();
				for (int x = 0; x < sourceData.GetLength(0); x++) {
						for (int y = 0; y < sourceData.GetLength(1); y++) {
								for (int z = 0; z < sourceData.GetLength(2); z++) {
										destData [x, 32 + y, z] = sourceData [x, y, z];
								}
						}
				}
		}
		private void createTrees ()
		{
				LinkedList<Vector3> trees = TreePlanter.generateTrees (world, this);
				foreach (Vector3 position in trees) {
						int x = ((int)position.x / Chunk.chunkSize);
						int y = ((int)position.y / Chunk.chunkSize);
						int z = ((int)position.z / Chunk.chunkSize);
					
						if (itemChunks [x, y, z] == null) {
								ItemChunk itemChunk = new ItemChunk ();
								itemChunks [x, y, z] = itemChunk;
						}
						itemChunks [x, y, z].addItem (position);
				}



		}
	
		private void createFlatBiome ()
		{
				for (int x=0; x<regionXZ; x++) {
						for (int z=0; z<regionXZ; z++) {
								for (int y=0; y<regionY; y++) {
										if (y <= (regionY / 2)) {
												data [x, y, z] = 1;
										}
								}
						}
				}	
		}
	
		private void createPerlin ()
		{
				for (int x=0; x<regionXZ; x++) {
						for (int z=0; z<regionXZ; z++) {
								int stone = PerlinNoise (this.getBlockOffsetX () + x, 0, this.getBlockOffsetZ () + z, 100, 20, 1.2f);
								stone += PerlinNoise (this.getBlockOffsetX () + x, 700, this.getBlockOffsetZ () + z, 20, 4, 0) + 10;
								int dirt = PerlinNoise (this.getBlockOffsetX () + x, 100, this.getBlockOffsetZ () + z, 50, 2, 0);
				
								for (int y=0; y<regionY; y++) {
										if (y <= stone) {
												data [x, y, z] = 1;
										} else if (y <= dirt + stone) { //Changed this line thanks to a comment
												data [x, y, z] = 2;
										}
					
								}
						}
				}
		}
	
		public void GenColumn (int x, int z)
		{
				for (int y=0; y<chunks.GetLength(1); y++) {
			
						//Create a temporary Gameobject for the new chunk instead of using chunks[x,y,z]
						GameObject newChunk = Instantiate (chunk,
			                                   new Vector3 (x * Chunk.chunkSize - 0.5f + getBlockOffsetX (),
			             y * Chunk.chunkSize + 0.5f,
			             z * Chunk.chunkSize - 0.5f + getBlockOffsetZ ()),
			                                   new Quaternion (0, 0, 0, 0)) as GameObject;
				
						newChunk.transform.parent = this.transform;
						chunks [x, y, z] = newChunk.GetComponent ("Chunk") as Chunk;
						chunks [x, y, z].voxels = this;
						chunks [x, y, z].chunkX = x * Chunk.chunkSize;
						chunks [x, y, z].chunkY = y * Chunk.chunkSize;
						chunks [x, y, z].chunkZ = z * Chunk.chunkSize;
						if (itemChunks [x, y, z] != null) {
								ItemChunk itemChunk = itemChunks [x, y, z];
								//itemChunk.addItem (chunks[x,y,z], this, 
								//TODO Re-enable rending item chunks.
						}
						
			
				}
		}
	
		public void UnloadColumn (int x, int z)
		{
				for (int y=0; y<chunks.GetLength(1); y++) {
						GameObject.Destroy (chunks [x, y, z].gameObject);
			
				}
		}
	
		int PerlinNoise (float x, int y, float z, float scale, float height, float power)
		{
				float rValue;
				rValue = Noise.GetNoise (((double)x) / scale, ((double)y) / scale, ((double)z) / scale);
				rValue *= height;
		
				if (power != 0) {
						rValue = Mathf.Pow (rValue, power);
				}
		
				return (int)rValue;
		}

		public Vector3 getBounds ()
		{
				return new Vector3 (data.GetLength (0), data.GetLength (1), data.GetLength (2));
		}
		public byte[,,] GetAllBlocks ()
		{
				return data;
		}
		public byte GetBlockAtCoords (int x, int y, int z)
		{
		
				if (x >= regionXZ || x < 0 || y >= regionY || y < 0 || z >= regionXZ || z < 0) {
						int worldX = x + this.getBlockOffsetX ();
						int worldZ = z + this.getBlockOffsetZ ();
						Region neighbor = world.getRegionAtCoords (worldX, worldZ);
						int[] normalizedCoords = normalizeToLocal (x, y, z);
						return neighbor.GetBlockAtCoords (normalizedCoords);
				} else {
						return data [x, y, z];
				}
		}
		
		private byte GetBlockAtCoords (int[] normalizedCoords)
		{
				byte block = data [normalizedCoords [0], normalizedCoords [1], normalizedCoords [2]];
				return block;

		}

		private int[] normalizeToLocal (int x, int y, int z)
		{
				int[] result = {x,y,z};
				if (x >= regionXZ) {
						result [0] = x - regionXZ;
				} else if (x < 0) {
						result [0] = x + regionXZ;
				}

				if (z >= regionXZ) {
						result [2] = z - regionXZ;
				} else if (z < 0) {
						result [2] = z + regionXZ;
				}
				if (y >= regionY) {
						result [1] = regionY - 1;
				} else if (y < 0) {
						result [1] = 0;
				}

				result [0] = result [0] % regionXZ;
				return result;
		}

		public  int[] convertWorldToLocal (int x, int y, int z)
		{
				int[] result = {x,y,z};
				
				if (x < 0) {
						result [0] = x - getBlockOffsetX ();
				} else {
						result [0] = x % regionXZ;
				}

				result [1] = y;

				if (z < 0) {
						result [2] = z - getBlockOffsetZ ();
				} else {
						result [2] = z % regionXZ;
				}
				return result;
		}

		public void flagChunkForUpdate (int x, int y, int z)
		{

				int chunkDim = regionXZ / Chunk.chunkSize;
				if (x >= chunkDim) {
						world.getRegionAtIndex (this.offsetX + 1, this.offsetZ).chunks [x - chunkDim, y, z].update = true;
				} else if (x < 0) {
						world.getRegionAtIndex (this.offsetX - 1, this.offsetZ).chunks [x + chunkDim, y, z].update = true;
				} else if (z >= chunkDim) {
						world.getRegionAtIndex (this.offsetX, this.offsetZ + 1).chunks [x, y, z - chunkDim].update = true;
				} else if (z < 0) {
						world.getRegionAtIndex (this.offsetX, this.offsetZ - 1).chunks [x, y, z + chunkDim].update = true;
				} else {
						chunks [x, y, z].update = true;
				}

		}

		public string hashString ()
		{
				return this.offsetX + "x" + this.offsetZ;
		}
}


